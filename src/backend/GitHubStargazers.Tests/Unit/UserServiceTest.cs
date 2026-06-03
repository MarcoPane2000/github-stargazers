using FluentAssertions;
using GitHubStargazers.WebApi.Data;
using GitHubStargazers.WebApi.Models;
using GitHubStargazers.WebApi.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GitHubStargazers.UnitTests.Services;

public class UserServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DataContext _context;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new DataContext(options);

        _context.Database.EnsureCreated();

        _sut = new UserService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsUser()
    {
        var user = new User { Username = "TestUser", Email = "test@test.com", PasswordHash = "hash", RefreshToken = "" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Username.Should().Be("TestUser");
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailOrUsernameAsync_WhenMatchesByEmail_ReturnsUser()
    {
        var user = new User { Username = "TheUser", Email = "find@test.com", PasswordHash = "hash", RefreshToken = "" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByEmailOrUsernameAsync("find@test.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("find@test.com");
    }

    [Fact]
    public async Task GetByEmailOrUsernameAsync_WhenMatchesByUsername_ReturnsUser()
    {
        var user = new User { Username = "FindMe", Email = "findme@test.com", PasswordHash = "hash", RefreshToken = "" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByEmailOrUsernameAsync("FindMe");

        result.Should().NotBeNull();
        result!.Username.Should().Be("FindMe");
    }

    [Fact]
    public async Task GetByEmailOrUsernameAsync_IsCaseInsensitive()
    {
        var user = new User { Username = "CaseSensitive", Email = "case@test.com", PasswordHash = "hash", RefreshToken = "" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var byEmailUppercase = await _sut.GetByEmailOrUsernameAsync("CASE@TEST.COM");
        var byUsernameUppercase = await _sut.GetByEmailOrUsernameAsync("casesensitive");

        byEmailUppercase.Should().NotBeNull();
        byUsernameUppercase.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByEmailOrUsernameAsync_WhenNoMatch_ReturnsNull()
    {
        var result = await _sut.GetByEmailOrUsernameAsync("ghost@test.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateUserAsync_PersistsUserAndReturnsIt()
    {
        // Arrange
        var user = new User { Username = "NewUser", Email = "new@test.com", PasswordHash = "hash", RefreshToken = "" };

        // Act
        var result = await _sut.CreateUserAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var persisted = await _context.Users.FindAsync(result.Id);
        persisted.Should().NotBeNull();
        persisted!.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task UpdateRefreshTokenAsync_WhenUserExists_UpdatesToken()
    {
        var user = new User { Username = "TokenUser", Email = "token@test.com", PasswordHash = "hash", RefreshToken = "old-token" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var newExpiry = DateTime.UtcNow.AddDays(7);

        await _sut.UpdateRefreshTokenAsync(user.Id, "new-token", newExpiry);

        var updated = await _context.Users.FindAsync(user.Id);
        updated!.RefreshToken.Should().Be("new-token");
        updated.RefreshTokenExpiryTime.Should().BeCloseTo(newExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateRefreshTokenAsync_WhenUserDoesNotExist_DoesNotThrow()
    {
        // Act
        var act = async () => await _sut.UpdateRefreshTokenAsync(999, "token", DateTime.UtcNow);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetByRefreshTokenAsync_WhenTokenExists_ReturnsUser()
    {
        var user = new User { Username = "RefUser", Email = "ref@test.com", PasswordHash = "hash", RefreshToken = "my-refresh-token" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByRefreshTokenAsync("my-refresh-token");

        result.Should().NotBeNull();
        result!.RefreshToken.Should().Be("my-refresh-token");
    }

    [Fact]
    public async Task GetByRefreshTokenAsync_WhenTokenDoesNotExist_ReturnsNull()
    {
        var result = await _sut.GetByRefreshTokenAsync("non-existent-token");
        result.Should().BeNull();
    }
}