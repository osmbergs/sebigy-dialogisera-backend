// Features/Tenants/TenantService.cs

using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Sebigy.Dialogisera.Api.Domain;
using Sebigy.Dialogisera.Api.Features.Users;
using Sebigy.Dialogisera.Api.Utils;

namespace Sebigy.Dialogisera.Api.Features.Users;

public class UserService(AppDbContext db,ISessionContextAccessor sessionContext) : ServiceBase(db, sessionContext)
{
    public async Task<List<UserListResponse>> GetUsers()
    {
        return await db.Users
            .Select(u => u.ToListResponse())
            .ToListAsync();
    }

    public async Task<UserResponse?> GetUserById(Ulid id)
    {
        return await db.Users
            .Where(u => u.Id == id)
            .Select(u => new UserResponse(
                u.Id, 
                u.TenantId,u.Email,u.Name, u.CreatedAt, u.IsActive))
            .FirstOrDefaultAsync();
    }

    
    public async Task<UserResponse> CreateUser(CreateUserRequest request)
    {
        RequireRoot();
            
        var user = new User
        {
            Id = Ulid.NewUlid(),
            Email=request.Email,
            Name = request.Name,
            PasswordHash = CryptHelper.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive=true,
            Type = request.Type,
            TenantId=request.TenantId
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user.ToResponse();
    }

    
    
    
    public async Task<UserResponse?> UpdateUser(Ulid id, UpdateUserRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return null;

        user.Name = request.Name;
        user.IsActive = request.IsActive;
        await db.SaveChangesAsync();

        return user.ToResponse();
    }

    public async Task<bool> DeleteUser(Ulid id)
    {
        var tenant = await db.Tenants.FindAsync(id);
        if (tenant is null) return false;

        db.Tenants.Remove(tenant);
        await db.SaveChangesAsync();
        return true;
    }
}