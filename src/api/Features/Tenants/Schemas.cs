namespace Sebigy.Dialogisera.Api.Features.Tenants;

// Request DTOs
public record CreateTenantRequest(string Name);
public record UpdateTenantRequest(string Name, bool IsActive);

// Response DTOs
public record TenantResponse(Guid Id, string Name,  DateTime CreatedAt, bool IsActive);
public record TenantListResponse(Guid Id, string Name);