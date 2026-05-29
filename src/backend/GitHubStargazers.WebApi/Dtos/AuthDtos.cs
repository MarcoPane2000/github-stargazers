using GitHubStargazers.WebApi.Models;

namespace GitHubStargazers.WebApi.Dtos;

public record UserRegistrationRequest(string Username, string Email, string Password, FavoriteColor FavoriteColor);

public record UserLoginRequest(string EmailOrUsername, string Password);

public record UserResponse(int Id, string Username, string Email, FavoriteColor FavoriteColor);

public record AuthResult(
    UserResponse User,
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiry
);