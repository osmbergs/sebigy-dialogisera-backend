namespace Sebigy.Dialogisera.Api.Domain;


public enum UserType
{
    Normal,
    Admin ,
    Root
}

public class User
{
    public Ulid Id { get; set; }
    
    public Ulid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!; 
    
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }

    public string? Name { get; set; }

    public required UserType Type { get; set; }
    
    public required DateTime CreatedAt { get; set; }

    public required bool IsActive { get; set; } = true;
    
}