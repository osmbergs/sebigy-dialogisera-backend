namespace Sebigy.Dialogisera.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Authentication");

        group.MapPost("/login", (LoginRequest request, AuthService service) =>
        {
            var result = service.Authenticate(request);
            return result is not null
                ? Results.Ok(result)
                : Results.Unauthorized();
        });

        group.MapPost("/refresh", (RefreshRequest request, AuthService service) =>
        {
            var result = service.Refresh(request);
            return result is not null
                ? Results.Ok(result)
                : Results.Unauthorized();
        });
    }
}