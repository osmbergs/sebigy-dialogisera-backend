namespace Sebigy.Dialogisera.Api.Features.Users;

public record CreateUserRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string Name { get; init; }
    
}
public record UpdateUserRequest(
    string Name,
    bool IsActive
    );

public record UserResponse(
    Ulid Id,
    string Email,
    string Name,
    DateTime CreatedAt,
    bool IsActive
    );

public record UserListResponse(
    Ulid Id,
    string Name);