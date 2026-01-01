using Microsoft.EntityFrameworkCore;
using Sebigy.Dialogisera.Api.Domain;

namespace Sebigy.Dialogisera.Api.Domain;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Customers => Set<User>();
}