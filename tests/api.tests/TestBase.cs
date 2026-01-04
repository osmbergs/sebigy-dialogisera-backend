using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Sebigy.Dialogisera.Api.Domain;
using Sebigy.Dialogisera.Api.Features.Auth;
using Sebigy.Dialogisera.Api.Utils;

namespace Sebigy.Dialogisera.Api.Tests;

public abstract class TestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    private Respawner _respawner = null!;
    private NpgsqlConnection _dbConnection = null!;

    
    
    
    protected const string DefaultTestEmail = "root@test.com";
    protected const string DefaultTestPassword = "root";
    
    // Seeded entity IDs (available for tests that need to reference them)
//    protected static readonly Ulid RootTenantId = Ulid.NewUlid();
  //  protected static readonly Ulid RootUserId = Ulid.NewUlid();


    protected  static Tenant RootTenant;
    protected static User RootUser;
    protected static Tenant TestTenant1;
    protected static User TestTenant1AdminUser1;
    protected static User TestTenant1NormalUser1;
    
    protected static Tenant TestTenant2;
    protected static User TestTenant2AdminUser1;
    protected static User TestTenant2NormalUser1;

    
    
    
    protected TestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    /// <summary>
    /// Authenticates with provided credentials and sets the Authorization header on the default client
    /// </summary>
    protected async Task<TokenResponse> AuthenticateAsync(
        string email = DefaultTestEmail, 
        string password = DefaultTestPassword)
    {
        var response = await Client.PostAsJsonAsync("/auth/login", 
            new LoginRequest(email, password));
        
        await response.EnsureSuccessWithDetailsAsync();
        
        
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        
        Client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", tokenResponse!.AccessToken);

        return tokenResponse;
    }

    /// <summary>
    /// Gets a token without setting it on the default client (useful for testing with multiple users)
    /// </summary>
    protected async Task<TokenResponse?> GetTokenAsync(string email, string password)
    {
        using var client = Factory.CreateClient();
        
        var response = await client.PostAsJsonAsync("/auth/login", 
            new LoginRequest(email, password));
        
        if (!response.IsSuccessStatusCode)
            return null;
        
        return await response.Content.ReadFromJsonAsync<TokenResponse>();
    }

    /// <summary>
    /// Creates a new HttpClient authenticated as a specific user
    /// </summary>
    protected async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password)
    {
        var token = await GetTokenAsync(email, password);
        if (token == null)
            throw new InvalidOperationException($"Failed to authenticate as {email}");

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token.AccessToken);
        
        return client;
    }

    /// <summary>
    /// Creates a new HttpClient without authentication
    /// </summary>
    protected HttpClient CreateUnauthenticatedClient()
    {
        return Factory.CreateClient();
    }

    /// <summary>
    /// Clears authentication from the default client
    /// </summary>
    protected void ClearAuthentication()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    public async Task InitializeAsync()
    {
        _dbConnection = new NpgsqlConnection(Factory.ConnectionString);
        await _dbConnection.OpenAsync();
        
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
        
        await _respawner.ResetAsync(_dbConnection);
        ClearAuthentication();
        await SeedTestDataAsync();
    }
    
    
    private async Task SeedTestDataAsync()
    {
        await ExecuteDbContextAsync(async db =>
        {
             RootTenant= new Tenant
            {
                Id = Ulid.NewUlid(),
                Name = "Root Tenant",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
             
            TestTenant1= new Tenant
            {
                Id = Ulid.NewUlid(),
                Name = "Test Tenant 1",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            TestTenant2= new Tenant
            {
                Id = Ulid.NewUlid(),
                Name = "Test Tenant 2",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            
            RootUser = new User
            {
                Id = Ulid.NewUlid(),
                TenantId = RootTenant.Id,
                Email = DefaultTestEmail,
                PasswordHash = CryptHelper.HashPassword(DefaultTestPassword),
                Name = "Root User",
                Type = UserType.Root,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            
            TestTenant1AdminUser1 = new User
            {
                Id = Ulid.NewUlid(),
                TenantId = TestTenant1.Id,
                Email = "test_tenant_1_admin_user@test.com",
                PasswordHash = CryptHelper.HashPassword("test"),
                Name = "TestTenant1AdminUser1",
                Type = UserType.Admin,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            TestTenant1NormalUser1 = new User
            {
                Id = Ulid.NewUlid(),
                TenantId = TestTenant1.Id,
                Email = "test_tenant_1_normal_user@test.com",
                PasswordHash = CryptHelper.HashPassword("test"),
                Name = "TestTenant1NormalUser1",
                Type = UserType.Normal,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            TestTenant2AdminUser1 = new User
            {
                Id = Ulid.NewUlid(),
                TenantId = TestTenant2.Id,
                Email = "test_tenant_2_admin_user@test.com",
                PasswordHash = CryptHelper.HashPassword("test"),
                Name = "TestTenant2AdminUser1",
                Type = UserType.Admin,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            TestTenant2NormalUser1 = new User
            {
                Id = Ulid.NewUlid(),
                TenantId = TestTenant2.Id,
                Email = "test_tenant_2_normal_user@test.com",
                PasswordHash = CryptHelper.HashPassword("test"),
                Name = "TestTenant2NormalUser1",
                Type = UserType.Normal,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            db.Tenants.Add(RootTenant);
            db.Tenants.Add(TestTenant1);
            db.Tenants.Add(TestTenant2);
            db.Users.Add(RootUser);
            db.Users.Add(TestTenant1AdminUser1);
            db.Users.Add(TestTenant1NormalUser1);
            db.Users.Add(TestTenant2AdminUser1);
            db.Users.Add(TestTenant2NormalUser1);
            
            await db.SaveChangesAsync();
        });
    }

    
    

    public async Task DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
    }

    protected async Task ExecuteDbContextAsync(Func<AppDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await action(db);
    }
}