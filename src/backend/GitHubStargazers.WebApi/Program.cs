using GitHubStargazers.WebApi.Dtos;
using GitHubStargazers.WebApi.Models;
using GitHubStargazers.WebApi.Extensions;
using Mapster;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddAppInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddAppSecurity(builder.Configuration);

TypeAdapterConfig<UserRegistrationRequest, User>.NewConfig().IgnoreNullValues(true);

var app = builder.Build();

app.UseAppMiddlewarePipeline();

app.MapEndpoints();
app.InitializeDatabase();

app.Run();