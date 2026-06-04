using GitHubStargazers.WebApi.Dtos;
using GitHubStargazers.WebApi.Services;
using GitHubStargazers.WebApi.Validators;

namespace GitHubStargazers.WebApi.Endpoints;

public static class AuthEndpoint
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").RequireCors();

        group.MapPost("/register", async (UserRegistrationRequest request, IAuthService authService, HttpContext httpContext, IHostEnvironment env) =>
        {
            var isProduction = env.IsProduction();
            AuthResult? result = await authService.RegisterUserAsync(request);

            if (result is null)
                return Results.Problem(
                    detail: "Email or Username already in use. Please try with different credentials.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Registration failed"
                );

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = result.RefreshTokenExpiry
            };

            httpContext.Response.Cookies.Append("X-Access-Token", result.AccessToken, cookieOptions);
            httpContext.Response.Cookies.Append("X-Refresh-Token", result.RefreshToken, refreshTokenCookieOptions);

            return Results.Created($"/api/users/{result.User.Id}", new { user = result.User });

        }).AddEndpointFilter<ValidationFilter<UserRegistrationRequest>>();

        group.MapPost("/login", async (UserLoginRequest request, IAuthService authService, HttpContext httpContext, IHostEnvironment env) =>
        {
            var isProduction = env.IsProduction();
            AuthResult? result = await authService.LoginUserAsync(request);

            if (result is null)
                return Results.Problem(
                    detail: "Email or password invalid. Please try again.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid credentials"
                );

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = result.RefreshTokenExpiry
            };

            httpContext.Response.Cookies.Append("X-Access-Token", result.AccessToken, cookieOptions);
            httpContext.Response.Cookies.Append("X-Refresh-Token", result.RefreshToken, refreshTokenCookieOptions);

            return Results.Ok(new { user = result.User });

        }).AddEndpointFilter<ValidationFilter<UserLoginRequest>>();

        group.MapPost("/refresh", async (HttpContext httpContext, IAuthService authService, IHostEnvironment env) =>
        {
            var refreshToken = httpContext.Request.Cookies["X-Refresh-Token"];

            if (string.IsNullOrEmpty(refreshToken))
                return Results.Unauthorized();

            AuthResult? result = await authService.RefreshTokenAsync(refreshToken);

            if (result is null)
                return Results.Unauthorized();

            var isProduction = env.IsProduction();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = result.RefreshTokenExpiry
            };

            httpContext.Response.Cookies.Append("X-Access-Token", result.AccessToken, cookieOptions);
            httpContext.Response.Cookies.Append("X-Refresh-Token", result.RefreshToken, refreshTokenCookieOptions);

            return Results.Ok(new { user = result.User });
        });

        group.MapPost("/logout", async (HttpContext httpContext, IAuthService authService) =>
        {
            var refreshToken = httpContext.Request.Cookies["X-Refresh-Token"];

            if (!string.IsNullOrEmpty(refreshToken))
                await authService.RevokeTokenAsync(refreshToken);

            httpContext.Response.Cookies.Delete("X-Access-Token");
            httpContext.Response.Cookies.Delete("X-Refresh-Token");

            return Results.Ok();
        });
    }
}