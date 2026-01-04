namespace Sebigy.Dialogisera.Api.Domain;

public class User
{
    public Ulid Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }

    public string? Name { get; set; }

    
    
    public required DateTime CreatedAt { get; set; }

    public required bool IsActive { get; set; } = true;
    
}