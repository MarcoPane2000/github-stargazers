using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using GitHubStargazers.WebApi.Dtos;
using GitHubStargazers.WebApi.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace GitHubStargazers.Tests.Unit;

public class GitHubServiceTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly HttpClient _httpClient;

    public GitHubServiceTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _configMock = new Mock<IConfiguration>();

        _httpClient = new HttpClient(_handlerMock.Object);

        _configMock.Setup(c => c["GitHubSettings:Token"]).Returns((string?)null);
    }

    private GitHubService CreateService() => new(_httpClient, _configMock.Object);

    private void SetupHttpResponse(HttpStatusCode statusCode, object? responseContent)
    {
        var httpResponse = new HttpResponseMessage(statusCode);
        if (responseContent != null)
        {
            httpResponse.Content = JsonContent.Create(responseContent);
        }

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);
    }

    [Fact]
    public void Constructor_ShouldAddHeaders_AndIncludeTokenIfPresent()
    {
        _configMock.Setup(c => c["GitHubSettings:Token"]).Returns("fake-github-token");

        var service = CreateService();

        _httpClient.DefaultRequestHeaders.UserAgent.ToString().Should().Contain("GitHubExplorer-DotNet");
        _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        _httpClient.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        _httpClient.DefaultRequestHeaders.Authorization!.Parameter.Should().Be("fake-github-token");
    }

    [Fact]
    public async Task SearchUsersAsync_WithEmptyQuery_ShouldReturnEmptyList()
    {
        var result = await CreateService().SearchUsersAsync("");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchUsersAsync_OnSuccess_ShouldReturnDeserializedUsers()
    {
        var expectedUsers = new List<GitHubUserResponse>
        {
            new("Owner", "FullName", "AvatarUrl", 50)
        };

        var apiResponse = new JsonObject { ["items"] = JsonSerializer.SerializeToNode(expectedUsers) };
        SetupHttpResponse(HttpStatusCode.OK, apiResponse);

        // Act
        var result = await CreateService().SearchUsersAsync("linus");

        // Assert
        result.Should().HaveCount(1);
        result.First().Owner.Should().Be("Owner");
    }

    [Fact]
    public async Task SearchUsersAsync_OnHttpError_ShouldReturnEmptyList()
    {
        SetupHttpResponse(HttpStatusCode.InternalServerError, null);

        var result = await CreateService().SearchUsersAsync("test");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchUserAsync_WithEmptyUsername_ShouldThrowArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService().SearchUserAsync("  "));
    }

    [Fact]
    public async Task SearchUserAsync_OnSuccess_ShouldReturnUser()
    {
        var expectedUser = new GitHubUserResponse("Owner", "FullName", "AvatarUrl", 42);
        SetupHttpResponse(HttpStatusCode.OK, expectedUser);

        var result = await CreateService().SearchUserAsync("torvalds");

        result.Should().NotBeNull();
        result.FullName.Should().Be("FullName");
    }

    [Fact]
    public async Task SearchUserAsync_OnNotFound_ShouldThrowException()
    {
        SetupHttpResponse(HttpStatusCode.NotFound, null);

        var act = () => CreateService().SearchUserAsync("unknown_user");
        await act.Should().ThrowAsync<Exception>().WithMessage("*Impossibile trovare l'utente*");
    }

    [Fact]
    public async Task SearchReposAsync_OnSuccess_ShouldReturnRepositories()
    {
        var expectedRepos = new List<GitHubUserRepoResponse>
        {
            new(new GitHubRepoOwner("Owner"), "RepoName", "RepoDescription", "RepoLink", 100000)
        };
        SetupHttpResponse(HttpStatusCode.OK, expectedRepos);

        var result = await CreateService().SearchReposAsync("torvalds");

        result.Should().HaveCount(1);
        result.First().RepoName.Should().Be("RepoName");
    }

    [Fact]
    public async Task SearchReposAsync_OnHttpError_ShouldReturnEmptyList()
    {
        SetupHttpResponse(HttpStatusCode.BadRequest, null);

        var result = await CreateService().SearchReposAsync("error_user");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchRepoAsync_OnSuccess_ShouldReturnRepository()
    {
        var expectedRepo = new GitHubUserRepoResponse(new GitHubRepoOwner("Owner"), "RepoName", "RepoDescription", "RepoLink", 100000);
        SetupHttpResponse(HttpStatusCode.OK, expectedRepo);

        var result = await CreateService().SearchRepoAsync("Owner", "RepoName");

        result.Should().NotBeNull();
        result.Owner.Login.Should().Be("Owner");
    }

    [Fact]
    public async Task SearchRepoAsync_OnNotFound_ShouldThrowException()
    {
        SetupHttpResponse(HttpStatusCode.NotFound, null);

        var act = () => CreateService().SearchRepoAsync("Owner", "wrong-repo");
        await act.Should().ThrowAsync<Exception>().WithMessage("*Impossibile trovare il repository*");
    }

    [Fact]
    public async Task SearchRepoStargazersAsync_OnSuccess_ShouldMapToGitHubUserList()
    {
        var apiUsers = new List<GitHubUserResponse>
        {
            new("Owner", "FullName", "AvatarUrl", 5)
        };
        SetupHttpResponse(HttpStatusCode.OK, apiUsers);

        var result = await CreateService().SearchRepoStargazersAsync("Owner", "RepoName");

        result.Should().HaveCount(1);
        result.First().Owner.Should().Be("Owner");
        result.First().FullName.Should().Be("FullName");
    }

    [Fact]
    public async Task SearchRepoStargazersAsync_WithInvalidInput_ShouldReturnEmptyList()
    {
        var result = await CreateService().SearchRepoStargazersAsync("", "RepoName");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchRepoStargazersAsync_OnHttpError_ShouldReturnEmptyList()
    {
        SetupHttpResponse(HttpStatusCode.Forbidden, null);

        var result = await CreateService().SearchRepoStargazersAsync("Owner", "RepoName");

        result.Should().BeEmpty();
    }
}