using GitHubStargazers.WebApi.Models;

namespace GitHubStargazers.WebApi.Services;

public interface IUserService
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailOrUsernameAsync(string nameOrUsername);
    Task<User> CreateUserAsync(User user);
    Task UpdateRefreshTokenAsync(int userId, string token, DateTime expiry);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
}