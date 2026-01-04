using Sebigy.Dialogisera.Api.Domain;

namespace Sebigy.Dialogisera.Api.Features.Users;

public record CreateUserRequest
{
    public required Ulid TenantId { get; init; }

    public required string Email { get; init; }
    public required string Password { get; init; }
    public string Name { get; init; }
    
    public required UserType Type { get; init; }
    
}
public record UpdateUserRequest(
    string Name,
    bool IsActive
    );

public record UserResponse(
    Ulid Id,
    Ulid TenantId,
    string Email,
    string Name,
    DateTime CreatedAt,
    bool IsActive
    );

public record UserListResponse(
    Ulid Id,
    string Name);