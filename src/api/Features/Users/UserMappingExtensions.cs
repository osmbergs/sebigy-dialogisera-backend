using Sebigy.Dialogisera.Api.Domain;

namespace Sebigy.Dialogisera.Api.Features.Users;

public static class UserMappingExtensions
{
    public static UserResponse ToResponse(this User user) => new(
        user.Id,
        user.TenantId,
        user.Email,
        user.Name ?? string.Empty,
        user.CreatedAt,
        user.IsActive
    );
    
    public static UserListResponse ToListResponse(this User user) => new(
        user.Id,
        user.Email
    );
}