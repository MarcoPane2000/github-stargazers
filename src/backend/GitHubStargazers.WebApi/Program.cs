using GitHubStargazers.WebApi.Dtos;
using GitHubStargazers.WebApi.Models;
using GitHubStargazers.WebApi.Extensions;
using Mapster;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddAppSecurity(builder.Configuration);

TypeAdapterConfig<UserRegistrationRequest, User>.NewConfig().IgnoreNullValues(true);

var app = builder.Build();

app.InitializeDatabase();

app.UseAppMiddlewarePipeline();

app.MapEndpoints();

app.Run();