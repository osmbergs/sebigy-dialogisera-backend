namespace Sebigy.Dialogisera.Api.Features.Tenants;

// Request DTOs
public record CreateTenantRequest
{
    public required string Name { get; init; }
}

public record UpdateTenantRequest
{
    public string Name { get; init; }
    public bool IsActive { get; init; }
    
}

// Response DTOs
public record TenantResponse(Ulid Id, string Name,  DateTime CreatedAt, bool IsActive);
public record TenantListResponse(Ulid Id, string Name);