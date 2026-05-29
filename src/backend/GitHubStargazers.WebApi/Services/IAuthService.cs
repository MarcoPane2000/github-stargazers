using GitHubStargazers.WebApi.Dtos;
using GitHubStargazers.WebApi.Models;

namespace GitHubStargazers.WebApi.Services;

public interface IAuthService
{
    public Task<AuthResult?> RegisterUserAsync(UserRegistrationRequest user);
    public Task<AuthResult?> LoginUserAsync(UserLoginRequest user);
    public Task<AuthResult?> RefreshTokenAsync(string refreshToken);
    public Task RevokeTokenAsync(string refreshToken);
}