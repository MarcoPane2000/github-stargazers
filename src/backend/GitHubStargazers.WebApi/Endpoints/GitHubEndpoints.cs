using GitHubStargazers.WebApi.Dtos;
using GitHubStargazers.WebApi.Services;
using Mapster;

namespace GitHubStargazers.WebApi.Endpoints;

public static class GitHubEndpoint
{
    public static void MapGitHubEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stargazers").RequireAuthorization().RequireCors();

        group.MapGet("/users", async (string q, IGitHubService gitHubService) =>
        {
            var users = await gitHubService.SearchUsersAsync(q);
            return Results.Ok(users.Select(user => user.Adapt<GitHubUser>()));
        });

        group.MapGet("/user", async (string username, IGitHubService gitHubService) =>
        {
            var user = await gitHubService.SearchUserAsync(username);
            return Results.Ok(user.Adapt<GitHubUser>());
        });

        group.MapGet("/repos/{user}", async (string user, IGitHubService gitHubService) =>
        {
            var repos = await gitHubService.SearchReposAsync(user);
            return Results.Ok(repos.Select(repo => repo.Adapt<GitHubUserRepo>()));
        });

        group.MapGet("/repos/{owner}/{repoName}/stargazers", async (string owner, string repoName, IGitHubService gitHubService) =>
        {
            var stargazers = await gitHubService.SearchRepoStargazersAsync(owner, repoName);
            return Results.Ok(stargazers);
        });
    }
}