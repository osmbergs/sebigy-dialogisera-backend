// Features/Tenants/TenantService.cs

using Microsoft.EntityFrameworkCore;
using Sebigy.Dialogisera.Api.Domain;

namespace Sebigy.Dialogisera.Api.Features.Tenants;

public class TenantService(AppDbContext db)
{
    public async Task<List<TenantListResponse>> GetAllAsync()
    {
        return await db.Tenants
            .Select(t => new TenantListResponse(t.Id, t.Name))
            .ToListAsync();
    }

    public async Task<TenantResponse?> GetByIdAsync(Guid id)
    {
        return await db.Tenants
            .Where(t => t.Id == id)
            .Select(t => new TenantResponse(t.Id, t.Name, t.CreatedAt, t.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task<TenantResponse> CreateAsync(CreateTenantRequest request)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
        
            CreatedAt = DateTime.UtcNow
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        return new TenantResponse(tenant.Id, tenant.Name,  tenant.CreatedAt, tenant.IsActive);
    }

    public async Task<TenantResponse?> UpdateAsync(Guid id, UpdateTenantRequest request)
    {
        var tenant = await db.Tenants.FindAsync(id);
        if (tenant is null) return null;

        tenant.Name = request.Name;
        tenant.IsActive = request.IsActive;
        await db.SaveChangesAsync();

        return new TenantResponse(tenant.Id, tenant.Name, tenant.CreatedAt, tenant.IsActive);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tenant = await db.Tenants.FindAsync(id);
        if (tenant is null) return false;

        db.Tenants.Remove(tenant);
        await db.SaveChangesAsync();
        return true;
    }
}