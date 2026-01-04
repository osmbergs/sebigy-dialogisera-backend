using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sebigy.Dialogisera.Api.Domain;
using Sebigy.Dialogisera.Api.Features.Tenants;
using Sebigy.Dialogisera.Api.Features.Users;
using Sebigy.Dialogisera.Api.Utils;
using Xunit.Abstractions;

namespace Sebigy.Dialogisera.Api.Tests.Features.Tenants;

public class UserTests : TestBase
{
    private readonly ITestOutputHelper _output;

    public UserTests(CustomWebApplicationFactory factory, ITestOutputHelper output) 
        : base(factory)
    {
        _output = output;
    }

    
    [Fact]
    public async Task Create_and_retrieve_User_with_root_user_ok()
    {

        await AuthenticateAsync();
        
        
        var request = new CreateUserRequest{
            Email="test@test.com",
            Password="password",
            Type=UserType.Normal,
            TenantId=TestTenant1.Id
            };
        var response = await Client.PostAsJsonAsync("/users",request);
        await response.EnsureSuccessWithDetailsAsync();
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdUser = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(createdUser);
        Assert.Equal("test@test.com", createdUser.Email);
        Assert.NotEqual(Ulid.Empty, createdUser.Id);
        Assert.Equal(TestTenant1.Id, createdUser.TenantId);
        
        
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
        var userInDb = await db.Users.FirstOrDefaultAsync(u => u.Id == createdUser.Id);
    
        Assert.NotNull(userInDb);
        Assert.True(CryptHelper.IsHashEqual("password", userInDb.PasswordHash));
      
    }
    
    
    
    [Fact]
    public async Task Create_User_with_normal_user_fail()
    {

        await AuthenticateAsync("test_tenant_1_normal_user@test.com","test");
        
        
        var request = new CreateUserRequest{
            Email="test@test.com",
            Password="password",
            Type=UserType.Normal,
            TenantId=TestTenant1.Id
        };
        var response = await Client.PostAsJsonAsync("/users",request);
        
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Status Code: {response.StatusCode}");
        _output.WriteLine($"Response Body: {content}");
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
      
    }


}