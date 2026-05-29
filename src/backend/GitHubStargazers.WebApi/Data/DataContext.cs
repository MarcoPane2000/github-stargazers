using GitHubStargazers.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GitHubStargazers.WebApi.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(user => user.FavoriteColor)
            .HasConversion<int>();
    }
}