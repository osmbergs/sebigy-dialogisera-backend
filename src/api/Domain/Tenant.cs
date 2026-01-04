namespace Sebigy.Dialogisera.Api.Domain;

public class Tenant
{
    public Ulid Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
 //   // Navigation properties
   // public ICollection<Customer> Customers { get; set; } = [];
}