using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Sebigy.Dialogisera.Api.Domain;
using Sebigy.Dialogisera.Api.Features.Auth;

namespace Sebigy.Dialogisera.Api.Tests;

public abstract class TestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    private Respawner _respawner = null!;
    private NpgsqlConnection _dbConnection = null!;

    
    
    
    
    // Default test credentials
    protected const string DefaultTestEmail = "admin@example.com";
    protected const string DefaultTestPassword = "password";

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