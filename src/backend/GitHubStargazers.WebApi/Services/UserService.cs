using GitHubStargazers.WebApi.Data;
using GitHubStargazers.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GitHubStargazers.WebApi.Services;

public class UserService(DataContext context) : IUserService
{
    private readonly DataContext _context = context;

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }
    public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername)
    {
        return await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email.ToLower() == emailOrUsername.ToLower() ||
                    u.Username.ToLower() == emailOrUsername.ToLower()
                );
    }
    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    public async Task UpdateRefreshTokenAsync(int userId, string token, DateTime expiry)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.RefreshToken = token;
            user.RefreshTokenExpiryTime = expiry;

            await _context.SaveChangesAsync();
        }
    }
    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }
}