using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sebigy.Dialogisera.Api.Domain;
using Sebigy.Dialogisera.Api.Features.Tenants;
using Sebigy.Dialogisera.Api.Features.Users;
using Sebigy.Dialogisera.Api.Utils;

namespace Sebigy.Dialogisera.Api.Tests.Features.Tenants;

public class TenantTests(CustomWebApplicationFactory factory) 
    : TestBase(factory)
{
    
    
    
    
    
    [Fact]
    public async Task Create_and_retrieve_Tenant_ok()
    {

        await AuthenticateAsync();
        var request = new CreateTenantRequest{
            Name="Test Tenant"
        };
        var response = await Client.PostAsJsonAsync("/tenants",request);
        await response.EnsureSuccessWithDetailsAsync();
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdTenant = await response.Content.ReadFromJsonAsync<TenantResponse>();
        Assert.NotNull(createdTenant);
        Assert.Equal("Test Tenant", createdTenant.Name);
        Assert.NotEqual(Ulid.Empty, createdTenant.Id);

      
    }
    
    
    
    
    
    
    
    
    
    
    
    
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
    public async Task GetById_ReturnsNotFound_WhenDoesNotExist()
    {
        await AuthenticateAsync();
        // Act
        var response = await Client.GetAsync($"/tenants/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}