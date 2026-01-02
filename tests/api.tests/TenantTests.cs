using System.Net;
using System.Net.Http.Json;
using Sebigy.Dialogisera.Api.Domain;
using Sebigy.Dialogisera.Api.Features.Tenants;

namespace Sebigy.Dialogisera.Api.Tests.Features.Tenants;

public class TenantTests(CustomWebApplicationFactory factory) 
    : TestBase(factory)
{
    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoTenants()
    {

        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/tenants");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var tenants = await response.Content.ReadFromJsonAsync<List<TenantListResponse>>();
        Assert.NotNull(tenants);
        Assert.Empty(tenants);
    }

    [Fact]
    public async Task Create_ReturnsTenant_WithValidRequest()
    {
        await AuthenticateAsync();
        
        // Arrange
        var request = new CreateTenantRequest("Test Tenant");

        // Act
        var response = await Client.PostAsJsonAsync("/tenants", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var tenant = await response.Content.ReadFromJsonAsync<TenantResponse>();
        Assert.NotNull(tenant);
        Assert.Equal("Test Tenant", tenant.Name);
        Assert.NotEqual(Guid.Empty, tenant.Id);
    }

    [Fact]
    public async Task GetById_ReturnsTenant_WhenExists()
    {
        await AuthenticateAsync();
        
        // Arrange — seed data directly via DbContext
        var tenantId = Guid.NewGuid();
        await ExecuteDbContextAsync(async db =>
        {
            db.Tenants.Add(new Tenant
            {
                Id = tenantId,
                Name = "Seeded Tenant",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        });

        // Act
        var response = await Client.GetAsync($"/tenants/{tenantId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var tenant = await response.Content.ReadFromJsonAsync<TenantResponse>();
        Assert.Equal("Seeded Tenant", tenant?.Name);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenDoesNotExist()
    {
        await AuthenticateAsync();
        // Act
        var response = await Client.GetAsync($"/tenants/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}