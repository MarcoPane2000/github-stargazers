using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using GitHubStargazers.WebApi.Models;
using GitHubStargazers.WebApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace GitHubStargazers.UnitTests.Services;

public class TokenServiceTests
{
    private readonly TokenService _sut;
    private readonly IConfiguration _config;

    public TokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "super-secret-key-for-testing-purposes-only-32chars!" },
            { "Jwt:Issuer", "test-issuer" },
            { "Jwt:Audience", "test-audience" }
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _sut = new TokenService(_config);
    }

    // -------------------------------------------------------------------------
    // GenerateAccessToken
    // -------------------------------------------------------------------------

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyString()
    {
        // Arrange
        var user = new User { Id = 1, Email = "user@test.com" };

        // Act
        var token = _sut.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateAccessToken_ContainsCorrectClaims()
    {
        // Arrange
        var user = new User { Id = 42, Email = "claims@test.com" };

        // Act
        var tokenString = _sut.GenerateAccessToken(user);

        // Assert - decodifica il JWT e verifica i claim
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        // USIAMO LE CHIAVI REALI: "nameid" e "email" (o "unique_name" in base a come li hai mappati)
        jwt.Claims.Should().Contain(c =>
            (c.Type == "nameid" || c.Type == ClaimTypes.NameIdentifier) && c.Value == user.Id.ToString());

        jwt.Claims.Should().Contain(c =>
            (c.Type == "email" || c.Type == ClaimTypes.Email) && c.Value == user.Email);
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectIssuerAndAudience()
    {
        // Arrange
        var user = new User { Id = 1, Email = "user@test.com" };

        // Act
        var tokenString = _sut.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");
    }

    [Fact]
    public void GenerateAccessToken_ExpiresInAboutOneHour()
    {
        // Arrange
        var user = new User { Id = 1, Email = "user@test.com" };
        var before = DateTime.UtcNow.AddHours(1).AddSeconds(-5);
        var after = DateTime.UtcNow.AddHours(1).AddSeconds(5);

        // Act
        var tokenString = _sut.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        jwt.ValidTo.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void GenerateAccessToken_IsValidatableWithCorrectKey()
    {
        // Arrange
        var user = new User { Id = 1, Email = "user@test.com" };
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        // Act
        var tokenString = _sut.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var act = () => handler.ValidateToken(tokenString, validationParams, out _);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateAccessToken_TwoDifferentUsers_ProduceDifferentTokens()
    {
        // Arrange
        var user1 = new User { Id = 1, Email = "user1@test.com" };
        var user2 = new User { Id = 2, Email = "user2@test.com" };

        // Act
        var token1 = _sut.GenerateAccessToken(user1);
        var token2 = _sut.GenerateAccessToken(user2);

        // Assert
        token1.Should().NotBe(token2);
    }

    // -------------------------------------------------------------------------
    // GenerateRefreshToken
    // -------------------------------------------------------------------------

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        // Act
        var token = _sut.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateRefreshToken_IsBase64Encoded()
    {
        // Act
        var token = _sut.GenerateRefreshToken();
        var act = () => Convert.FromBase64String(token);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_EachCallProducesUniqueToken()
    {
        // Act
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();
        var token3 = _sut.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
        token2.Should().NotBe(token3);
        token1.Should().NotBe(token3);
    }

    [Fact]
    public void GenerateRefreshToken_HasExpectedByteLength()
    {
        // Act - il token è 32 byte in base64 = 44 caratteri con padding
        var token = _sut.GenerateRefreshToken();
        var bytes = Convert.FromBase64String(token);

        // Assert
        bytes.Length.Should().Be(32);
    }
}