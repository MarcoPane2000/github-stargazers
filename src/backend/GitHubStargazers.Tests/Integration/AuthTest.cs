using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GitHubStargazers.WebApi.Data;
using GitHubStargazers.WebApi.Dtos;
using GitHubStargazers.WebApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GitHubStargazers.IntegrationTests;

public class AuthScenarioTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IGitHubService> _gitHubServiceMock = new();

    public AuthScenarioTests(WebApplicationFactory<Program> factory)
    {
        // tmp DB
        var sqliteConnection = new SqliteConnection("Filename=:memory:");
        sqliteConnection.Open();

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<DataContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<DataContext>(options =>
                {
                    options.UseSqlite(sqliteConnection);
                });

                _gitHubServiceMock
                    .Setup(s => s.SearchReposAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(new List<GitHubUserRepoResponse>()));

                services.AddSingleton(_gitHubServiceMock.Object);
            });
        });

        using (var scope = customizedFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            db.Database.EnsureCreated();
        }

        _client = customizedFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
            HandleCookies = true
        });
    }

    [Fact]
    public async Task Complete_User_Auth_LifeCycle_Scenario()
    {
        // 1. Registration
        var registerRequest = new
        {
            Username = "ScenarioUser",
            Email = "scenario@test.com",
            Password = "PasswordSicura123!",
            FavoriteColor = 1
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        registerResponse.Headers.Contains("Set-Cookie").Should().BeTrue();

        // 2. Login
        var loginRequest = new
        {
            EmailOrUsername = "ScenarioUser",
            Password = "PasswordSicura123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResponse.Headers.Contains("Set-Cookie").Should().BeTrue();

        // 3. Access to auth protected route
        var protectedResponse = await _client.GetAsync("/api/stargazers/repos/torvalds");
        protectedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Refresh
        var refreshResponse = await _client.PostAsync("/api/auth/refresh", null);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        refreshResponse.Headers.Contains("Set-Cookie").Should().BeTrue();

        // 5. Logout
        var logoutResponse = await _client.PostAsync("/api/auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Fail access to auth protected route
        var finalProtectedResponse = await _client.GetAsync("/api/stargazers/repos/torvalds");
        finalProtectedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}