using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using GitHubStargazers.WebApi.Dtos;

namespace GitHubStargazers.WebApi.Services;

public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;

    public GitHubService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "GitHubExplorer-DotNet");

        var token = config["GitHubSettings:Token"];
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
    }
    public async Task<List<GitHubUserResponse>> SearchUsersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return new();

        var url = $"https://api.github.com/search/users?q={query}+in:login&per_page=5";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode) return new();

        var jsonNode = await response.Content.ReadFromJsonAsync<JsonObject>();
        var itemsJson = jsonNode?["items"]?.ToJsonString();

        if (string.IsNullOrEmpty(itemsJson)) return new();

        return JsonSerializer.Deserialize<List<GitHubUserResponse>>(itemsJson) ?? new();
    }
    public async Task<GitHubUserResponse> SearchUserAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username non valido.");

        var url = $"https://api.github.com/users/{username}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Impossibile trovare l'utente GitHub '{username}'.");
        }

        return await response.Content.ReadFromJsonAsync<GitHubUserResponse>()
               ?? throw new Exception("Errore nella deserializzazione del profilo utente.");
    }
    public async Task<List<GitHubUserRepoResponse>> SearchReposAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return new();

        var url = $"https://api.github.com/users/{username}/repos?per_page=5&sort=updated";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode) return new();

        return await response.Content.ReadFromJsonAsync<List<GitHubUserRepoResponse>>() ?? new();
    }
    public async Task<GitHubUserRepoResponse> SearchRepoAsync(string owner, string repoName)
    {
        var url = $"https://api.github.com/repos/{owner}/{repoName}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Impossibile trovare il repository '/{owner}/{repoName}'.");
        }

        return await response.Content.ReadFromJsonAsync<GitHubUserRepoResponse>()
               ?? throw new Exception("Errore nella deserializzazione del repository.");
    }

    public async Task<List<GitHubUser>> SearchRepoStargazersAsync(string owner, string repoName)
    {
        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repoName)) return new();

        var url = $"https://api.github.com/repos/{owner}/{repoName}/stargazers?per_page=5";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new();

        var gitHubUsers = await response.Content.ReadFromJsonAsync<List<GitHubUserResponse>>() ?? new();

        return gitHubUsers.Select(u => new GitHubUser(
            Owner: u.Owner,
            FullName: u.FullName,
            AvatarUrl: u.AvatarUrl,
            ReposCount: u.ReposCount
        )).ToList();
    }
}