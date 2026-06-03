using GitHubStargazers.WebApi.Models;

namespace GitHubStargazers.WebApi.Services;

public interface ITokenService
{
    public string GenerateAccessToken(User user);
    public string GenerateRefreshToken();
}