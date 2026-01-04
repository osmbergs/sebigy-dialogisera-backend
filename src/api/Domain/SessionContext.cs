namespace Sebigy.Dialogisera.Api.Domain;

public class SessionContext
{
    public required Ulid UserId { get; init; }
    public required Ulid TenantId { get; init; }
    public required UserType UserType { get; init; }
    
}