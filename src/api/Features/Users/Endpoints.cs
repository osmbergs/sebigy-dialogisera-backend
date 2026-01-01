namespace Sebigy.Dialogisera.Api.Features.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users").WithTags("Users").RequireAuthorization();

        group.MapGet("/", async (UserService service) =>
            Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id:guid}", async (Guid id, UserService service) =>
            await service.GetByIdAsync(id) is { } tenant
                ? Results.Ok(tenant)
                : Results.NotFound());

        group.MapPost("/", async (CreateUserRequest request, UserService service) =>
        {
            var tenant = await service.CreateAsync(request);
            return Results.Created($"/tenants/{tenant.Id}", tenant);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest request, UserService service) =>
            await service.UpdateAsync(id, request) is { } tenant
                ? Results.Ok(tenant)
                : Results.NotFound());

        group.MapDelete("/{id:guid}", async (Guid id, UserService service) =>
            await service.DeleteAsync(id)
                ? Results.NoContent()
                : Results.NotFound());
    }
}