// Features/Tenants/TenantService.cs

using Microsoft.EntityFrameworkCore;
using Sebigy.Dialogisera.Api.Domain;
using Sebigy.Dialogisera.Api.Features.Users;

namespace Sebigy.Dialogisera.Api.Features.Users;

public class UserService(AppDbContext db)
{
    public async Task<List<UserListResponse>> GetAllAsync()
    {
        return await db.Tenants
            .Select(t => new UserListResponse(t.Id, t.Name))
            .ToListAsync();
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        return await db.Tenants
            .Where(t => t.Id == id)
            .Select(t => new UserResponse(t.Id, t.Name, t.CreatedAt, t.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
        
            CreatedAt = DateTime.UtcNow
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        return new UserResponse(tenant.Id, tenant.Name,  tenant.CreatedAt, tenant.IsActive);
    }

    public async Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var tenant = await db.Tenants.FindAsync(id);
        if (tenant is null) return null;

        tenant.Name = request.Name;
        tenant.IsActive = request.IsActive;
        await db.SaveChangesAsync();

        return new UserResponse(tenant.Id, tenant.Name, tenant.CreatedAt, tenant.IsActive);
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