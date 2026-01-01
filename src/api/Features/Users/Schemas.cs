namespace Sebigy.Dialogisera.Api.Features.Users;

public record CreateUserRequest(string Name);
public record UpdateUserRequest(string Name, bool IsActive);

public record UserResponse(Guid Id, string Name,  DateTime CreatedAt, bool IsActive);
public record UserListResponse(Guid Id, string Name);