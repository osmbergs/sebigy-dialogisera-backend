// Features/Tenants/TenantService.cs

using Microsoft.EntityFrameworkCore;
using Sebigy.Dialogisera.Api.Domain;
using Sebigy.Dialogisera.Api.Utils;

namespace Sebigy.Dialogisera.Api.Features.Tenants;

public class TenantService(AppDbContext db,ISessionContextAccessor sessionContext)  : ServiceBase(db, sessionContext)
{
    public async Task<List<TenantListResponse>> GetTenants()
    {
        return await db.Tenants
            .Select(t => new TenantListResponse(t.Id, t.Name))
            .ToListAsync();
    }

    public async Task<TenantResponse?> GetTenantById(Ulid id)
    {
        return await db.Tenants
            .Where(t => t.Id == id)
            .Select(t => new TenantResponse(t.Id, t.Name, t.CreatedAt, t.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task<TenantResponse> CreateTenant(CreateTenantRequest request)
    {
        var tenant = new Tenant
        {
            Id = Ulid.NewUlid(),
            Name = request.Name,
        
            CreatedAt = DateTime.UtcNow
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        return new TenantResponse(tenant.Id, tenant.Name,  tenant.CreatedAt, tenant.IsActive);
    }

    public async Task<TenantResponse?> UpdateTenant(Ulid id, UpdateTenantRequest request)
    {
        var tenant = await db.Tenants.FindAsync(id);
        if (tenant is null) return null;

        tenant.Name = request.Name;
        tenant.IsActive = request.IsActive;
        await db.SaveChangesAsync();

        return new TenantResponse(tenant.Id, tenant.Name, tenant.CreatedAt, tenant.IsActive);
    }

    public async Task<bool> DeleteTenant(Ulid id)
    {
        var tenant = await db.Tenants.FindAsync(id);
        if (tenant is null) return false;

        db.Tenants.Remove(tenant);
        await db.SaveChangesAsync();
        return true;
    }
}