// Domain/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Sebigy.Dialogisera.Api.Domain;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Ulid>()
            .HaveConversion<UlidToStringConverter>();
    }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(u => u.Tenant)
                .WithMany()                          // No collection on Tenant
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent deleting Tenant with Users
        });
        
        modelBuilder.Entity<User>()
            .Property(u => u.Type)
            .HasConversion<string>();
    }
}

public class UlidToStringConverter : ValueConverter<Ulid, string>
{
    public UlidToStringConverter() 
        : base(
            ulid => ulid.ToString(),
            str => Ulid.Parse(str))
    { }
}