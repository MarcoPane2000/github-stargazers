using GitHubStargazers.WebApi.Dtos;
using GitHubStargazers.WebApi.Models;
using Mapster;

namespace GitHubStargazers.WebApi.Services;

public class AuthService(IUserService userService, ITokenService tokenService) : IAuthService
{
    private readonly IUserService _userService = userService;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<AuthResult?> RegisterUserAsync(UserRegistrationRequest request)
    {
        var existingUser = await _userService.GetByEmailOrUsernameAsync(request.Email);
        if (existingUser != null) return null;

        User newUser = request.Adapt<User>();
        newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        newUser.RefreshToken = _tokenService.GenerateRefreshToken();
        newUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        var createdUser = await _userService.CreateUserAsync(newUser);

        var accessToken = _tokenService.GenerateAccessToken(createdUser);

        return new AuthResult(
            createdUser.Adapt<UserResponse>(),
            accessToken,
            createdUser.RefreshToken,
            createdUser.RefreshTokenExpiryTime
        );
    }

    public async Task<AuthResult?> LoginUserAsync(UserLoginRequest request)
    {
        var user = await _userService.GetByEmailOrUsernameAsync(request.EmailOrUsername);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _userService.UpdateRefreshTokenAsync(user.Id, newRefreshToken, refreshTokenExpiry);

        return new AuthResult(
            user.Adapt<UserResponse>(),
            newAccessToken,
            newRefreshToken,
            refreshTokenExpiry
        );
    }
    
    public async Task<AuthResult?> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userService.GetByRefreshTokenAsync(refreshToken);

        if (user is null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            return null;

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newExpiry = DateTime.UtcNow.AddDays(7);

        await _userService.UpdateRefreshTokenAsync(user.Id, newRefreshToken, newExpiry);

        return new AuthResult(
            user.Adapt<UserResponse>(),
            newAccessToken,
            newRefreshToken,
            newExpiry
        );
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var user = await _userService.GetByRefreshTokenAsync(refreshToken);

        if (user is null) return;

        await _userService.UpdateRefreshTokenAsync(user.Id, string.Empty, DateTime.UtcNow);
    }
}