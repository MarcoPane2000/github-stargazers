using GitHubStargazers.WebApi.Dtos;

namespace GitHubStargazers.WebApi.Services;

public interface IGitHubService
{
    public Task<List<GitHubUserResponse>> SearchUsersAsync(string query);
    public Task<GitHubUserResponse> SearchUserAsync(string username);
    public Task<List<GitHubUserRepoResponse>> SearchReposAsync(string query);
    public Task<GitHubUserRepoResponse> SearchRepoAsync(string userName, string repoName);
    public Task<List<GitHubUser>> SearchRepoStargazersAsync(string owner, string repoName);
}