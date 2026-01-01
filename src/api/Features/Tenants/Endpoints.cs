namespace Sebigy.Dialogisera.Api.Features.Tenants;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/tenants").WithTags("Tenants").RequireAuthorization(); 

        group.MapGet("/", async (TenantService service) =>
            Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id:guid}", async (Guid id, TenantService service) =>
            await service.GetByIdAsync(id) is { } tenant
                ? Results.Ok(tenant)
                : Results.NotFound());

        group.MapPost("/", async (CreateTenantRequest request, TenantService service) =>
        {
            var tenant = await service.CreateAsync(request);
            return Results.Created($"/tenants/{tenant.Id}", tenant);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateTenantRequest request, TenantService service) =>
            await service.UpdateAsync(id, request) is { } tenant
                ? Results.Ok(tenant)
                : Results.NotFound());

        group.MapDelete("/{id:guid}", async (Guid id, TenantService service) =>
            await service.DeleteAsync(id)
                ? Results.NoContent()
                : Results.NotFound());
    }
}