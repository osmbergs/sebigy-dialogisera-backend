namespace Sebigy.Dialogisera.Api.Domain;

public class SessionContext
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public Guid? TenantId { get; init; }
}