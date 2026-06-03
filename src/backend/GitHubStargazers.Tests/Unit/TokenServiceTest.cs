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

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyString()
    {
        var user = new User { Id = 1, Email = "user@test.com" };

        var token = _sut.GenerateAccessToken(user);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateAccessToken_ContainsCorrectClaims()
    {
        var user = new User { Id = 42, Email = "claims@test.com" };

        var tokenString = _sut.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        jwt.Claims.Should().Contain(c =>
            (c.Type == "nameid" || c.Type == ClaimTypes.NameIdentifier) && c.Value == user.Id.ToString());

        jwt.Claims.Should().Contain(c =>
            (c.Type == "email" || c.Type == ClaimTypes.Email) && c.Value == user.Email);
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectIssuerAndAudience()
    {
        var user = new User { Id = 1, Email = "user@test.com" };

        var tokenString = _sut.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");
    }

    [Fact]
    public void GenerateAccessToken_ExpiresInAboutOneHour()
    {
        var user = new User { Id = 1, Email = "user@test.com" };
        var before = DateTime.UtcNow.AddHours(1).AddSeconds(-5);
        var after = DateTime.UtcNow.AddHours(1).AddSeconds(5);

        var tokenString = _sut.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        jwt.ValidTo.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void GenerateAccessToken_IsValidatableWithCorrectKey()
    {
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

        var tokenString = _sut.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var act = () => handler.ValidateToken(tokenString, validationParams, out _);

        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateAccessToken_TwoDifferentUsers_ProduceDifferentTokens()
    {
        var user1 = new User { Id = 1, Email = "user1@test.com" };
        var user2 = new User { Id = 2, Email = "user2@test.com" };

        var token1 = _sut.GenerateAccessToken(user1);
        var token2 = _sut.GenerateAccessToken(user2);

        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateRefreshToken();

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateRefreshToken_IsBase64Encoded()
    {
        var token = _sut.GenerateRefreshToken();
        var act = () => Convert.FromBase64String(token);

        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_EachCallProducesUniqueToken()
    {
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();
        var token3 = _sut.GenerateRefreshToken();

        token1.Should().NotBe(token2);
        token2.Should().NotBe(token3);
        token1.Should().NotBe(token3);
    }

    [Fact]
    public void GenerateRefreshToken_HasExpectedByteLength()
    {
        var token = _sut.GenerateRefreshToken();
        var bytes = Convert.FromBase64String(token);

        bytes.Length.Should().Be(32);
    }
}