using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sebigy.Dialogisera.Api.Domain;
using Sebigy.Dialogisera.Api.Features.Tenants;
using Sebigy.Dialogisera.Api.Features.Users;
using Sebigy.Dialogisera.Api.Utils;

namespace Sebigy.Dialogisera.Api.Tests.Features.Tenants;

public class UserTests(CustomWebApplicationFactory factory) 
    : TestBase(factory)
{
    
    [Fact]
    public async Task Create_and_retrieve_user_ok()
    {

        await AuthenticateAsync();
        var request = new CreateUserRequest{
            Email="test@test.com",
            Password="password"
            };
        var response = await Client.PostAsJsonAsync("/users",request);
        await response.EnsureSuccessWithDetailsAsync();
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdUser = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(createdUser);
        Assert.Equal("test@test.com", createdUser.Email);
        Assert.NotEqual(Ulid.Empty, createdUser.Id);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
        var userInDb = await db.Users.FirstOrDefaultAsync(u => u.Id == createdUser.Id);
    
        Assert.NotNull(userInDb);
        Assert.True(CryptHelper.IsHashEqual("password", userInDb.PasswordHash));
      
    }


}