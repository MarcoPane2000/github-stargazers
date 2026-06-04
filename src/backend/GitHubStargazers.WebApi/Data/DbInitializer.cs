using GitHubStargazers.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GitHubStargazers.WebApi.Data;

public static class DbInitializer
{
    public static void Initialize(DataContext context, IConfiguration configuration)
    {
        // context.Database.Migrate();

        context.Database.EnsureCreated();

        if (context.Users.Any())
        {
            return;
        }

        var seedSection = configuration.GetSection("DefaultUser");
        string username = seedSection["Username"] ?? "root";
        string email = seedSection["Email"] ?? "root@stargazers.com";
        string password = seedSection["Password"] ?? "root";

        if (!int.TryParse(seedSection["FavouriteColor"], out int favouriteColorValue))
        {
            favouriteColorValue = 0;
        }

        var defaultUser = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),

            FavoriteColor = (FavoriteColor)favouriteColorValue,

            RefreshToken = Guid.NewGuid().ToString(),
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
        };

        context.Users.Add(defaultUser);
        context.SaveChanges();
    }
}