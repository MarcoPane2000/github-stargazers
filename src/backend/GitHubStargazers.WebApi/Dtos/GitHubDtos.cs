using System.Text.Json.Serialization;

namespace GitHubStargazers.WebApi.Dtos;

public record GitHubUserResponse(
    [property: JsonPropertyName("login")] string Owner,
    [property: JsonPropertyName("name")] string FullName,
    [property: JsonPropertyName("avatar_url")] string AvatarUrl,
    [property: JsonPropertyName("public_repos")] int ReposCount
);

public record GitHubRepoOwner(
    [property: JsonPropertyName("login")] string Login
);

public record GitHubUserRepoResponse(
    [property: JsonPropertyName("owner")] GitHubRepoOwner Owner,
    [property: JsonPropertyName("name")] string RepoName,
    [property: JsonPropertyName("description")] string RepoDescription,
    [property: JsonPropertyName("html_url")] string GitHubRepoLink,
    [property: JsonPropertyName("stargazers_count")] int StargazersCount
);

public record GitHubUser(
    string Owner,
    string FullName,
    string AvatarUrl,
    int ReposCount
);

public record GitHubUserRepo(
    string Owner,
    string RepoName,
    string RepoDescription,
    string GitHubRepoLink,
    int StargazersCount
);