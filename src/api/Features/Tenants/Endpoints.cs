namespace Sebigy.Dialogisera.Api.Features.Tenants;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/tenants").WithTags("Tenants").RequireAuthorization(); 

        group.MapGet("/", async (TenantService service) =>
            Results.Ok(await service.GetTenants()));

        group.MapGet("/{id}", async (Ulid id, TenantService service) =>
            await service.GetTenantById(id) is { } tenant
                ? Results.Ok(tenant)
                : Results.NotFound());

        group.MapPost("/", async (CreateTenantRequest request, TenantService service) =>
        {
            var tenant = await service.CreateTenant(request);
            return Results.Created($"/tenants/{tenant.Id}", tenant);
        });

        group.MapPut("/{id}", async (Ulid id, UpdateTenantRequest request, TenantService service) =>
            await service.UpdateTenant(id, request) is { } tenant
                ? Results.Ok(tenant)
                : Results.NotFound());

        group.MapDelete("/{id:guid}", async (Ulid id, TenantService service) =>
            await service.DeleteTenant(id)
                ? Results.NoContent()
                : Results.NotFound());
    }
}