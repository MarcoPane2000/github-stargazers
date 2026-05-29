using FluentAssertions;
using GitHubStargazers.WebApi.Dtos;
using GitHubStargazers.WebApi.Models;
using GitHubStargazers.WebApi.Services;
using Moq;

namespace GitHubStargazers.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userServiceMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenEmailAlreadyExists_ReturnsNull()
    {
        UserRegistrationRequest request = new("TestUser", "existing@test.com", "Password123!", FavoriteColor.Blue);

        _userServiceMock
            .Setup(s => s.GetByEmailOrUsernameAsync(request.Email))
            .ReturnsAsync(new User { Email = request.Email });

        var result = await _sut.RegisterUserAsync(request);

        result.Should().BeNull();
        _userServiceMock.Verify(s => s.CreateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenEmailIsNew_CreatesUserAndReturnsAuthResult()
    {
        UserRegistrationRequest request = new("NewUser", "new@test.com", "Password123!", FavoriteColor.Blue);


        var createdUser = new User
        {
            Id = 1,
            Username = request.Username,
            Email = request.Email,
            RefreshToken = "refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
        };

        _userServiceMock
            .Setup(s => s.GetByEmailOrUsernameAsync(request.Email))
            .ReturnsAsync((User?)null);

        _userServiceMock
            .Setup(s => s.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns("refresh-token");

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns("access-token");

        var result = await _sut.RegisterUserAsync(request);

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.User.Email.Should().Be(request.Email);

        _userServiceMock.Verify(s => s.CreateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_PasswordIsHashed_NotStoredAsPlainText()
    {
        UserRegistrationRequest request = new("TestUser", "existing@test.com", "Password123!", FavoriteColor.Blue);


        User? capturedUser = null;

        _userServiceMock
            .Setup(s => s.GetByEmailOrUsernameAsync(request.Email))
            .ReturnsAsync((User?)null);

        _userServiceMock
            .Setup(s => s.CreateUserAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => u);

        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("r");
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("a");

        await _sut.RegisterUserAsync(request);

        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBe(request.Password);
        BCrypt.Net.BCrypt.Verify(request.Password, capturedUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task LoginUserAsync_WhenUserNotFound_ReturnsNull()
    {
        // Arrange
        UserLoginRequest request = new("ghost", "pass");

        _userServiceMock
            .Setup(s => s.GetByEmailOrUsernameAsync(request.EmailOrUsername))
            .ReturnsAsync((User?)null);

        var result = await _sut.LoginUserAsync(request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginUserAsync_WhenPasswordIsWrong_ReturnsNull()
    {
        UserLoginRequest request = new("ghost", "wrongPass");

        var user = new User
        {
            Email = "user@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass")
        };

        _userServiceMock
            .Setup(s => s.GetByEmailOrUsernameAsync(request.EmailOrUsername))
            .ReturnsAsync(user);

        var result = await _sut.LoginUserAsync(request);

        result.Should().BeNull();
        _userServiceMock.Verify(s => s.UpdateRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task LoginUserAsync_WhenCredentialsAreValid_ReturnsAuthResultAndUpdatesToken()
    {
        var password = "CorrectPass123!";
        UserLoginRequest request = new("ghost", password);

        var user = new User
        {
            Id = 1,
            Email = "user@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _userServiceMock
            .Setup(s => s.GetByEmailOrUsernameAsync(request.EmailOrUsername))
            .ReturnsAsync(user);

        _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns("access-token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh-token");

        var result = await _sut.LoginUserAsync(request);

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("new-refresh-token");

        _userServiceMock.Verify(
            s => s.UpdateRefreshTokenAsync(user.Id, "new-refresh-token", It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenNotFound_ReturnsNull()
    {
        _userServiceMock
            .Setup(s => s.GetByRefreshTokenAsync("invalid-token"))
            .ReturnsAsync((User?)null);

        var result = await _sut.RefreshTokenAsync("invalid-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsExpired_ReturnsNull()
    {
        var user = new User
        {
            Id = 1,
            RefreshToken = "expired-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1) // scaduto ieri
        };

        _userServiceMock
            .Setup(s => s.GetByRefreshTokenAsync("expired-token"))
            .ReturnsAsync(user);

        var result = await _sut.RefreshTokenAsync("expired-token");

        result.Should().BeNull();
        _userServiceMock.Verify(s => s.UpdateRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsValid_ReturnsNewAuthResultAndRotatesToken()
    {
        var user = new User
        {
            Id = 1,
            Email = "user@test.com",
            RefreshToken = "valid-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(5)
        };

        _userServiceMock
            .Setup(s => s.GetByRefreshTokenAsync("valid-token"))
            .ReturnsAsync(user);

        _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns("new-access-token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh-token");

        var result = await _sut.RefreshTokenAsync("valid-token");

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");

        _userServiceMock.Verify(
            s => s.UpdateRefreshTokenAsync(user.Id, "new-refresh-token", It.IsAny<DateTime>()),
            Times.Once);
    }
    [Fact]
    public async Task RevokeTokenAsync_WhenUserNotFound_DoesNothing()
    {
        _userServiceMock
            .Setup(s => s.GetByRefreshTokenAsync("ghost-token"))
            .ReturnsAsync((User?)null);

        await _sut.RevokeTokenAsync("ghost-token");

        _userServiceMock.Verify(s => s.UpdateRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task RevokeTokenAsync_WhenUserFound_ClearsRefreshToken()
    {
        var user = new User { Id = 1, RefreshToken = "valid-token" };

        _userServiceMock
            .Setup(s => s.GetByRefreshTokenAsync("valid-token"))
            .ReturnsAsync(user);

        await _sut.RevokeTokenAsync("valid-token");

        _userServiceMock.Verify(
            s => s.UpdateRefreshTokenAsync(user.Id, string.Empty, It.IsAny<DateTime>()),
            Times.Once);
    }
}