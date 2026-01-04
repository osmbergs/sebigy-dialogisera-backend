namespace Sebigy.Dialogisera.Api.Features.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users").WithTags("Users").RequireAuthorization();

        group.MapGet("/", async (UserService service) =>
            Results.Ok(await service.GetUsers()));

        group.MapGet("/{id}", async (Ulid id, UserService service) =>
            await service.GetUserById(id) is { } tenant
                ? Results.Ok(tenant)
                : Results.NotFound());

        group.MapPost("/", async (CreateUserRequest request, UserService service) =>
        {
            var tenant = await service.CreateUser(request);
            return Results.Created($"/tenants/{tenant.Id}", tenant);
        });

        group.MapPut("/{id}", async (Ulid id, UpdateUserRequest request, UserService service) =>
            await service.UpdateUser(id, request) is { } tenant
                ? Results.Ok(tenant)
                : Results.NotFound());

        group.MapDelete("/{id}", async (Ulid id, UserService service) =>
            await service.DeleteUser(id)
                ? Results.NoContent()
                : Results.NotFound());
    }
}