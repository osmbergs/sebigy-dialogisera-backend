

using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Sebigy.Dialogisera.Api.Domain;
using Sebigy.Dialogisera.Api.Features.Tenants;
//using Sebigy.Dialogisera.Api.Features.Customers;

var builder = WebApplication.CreateBuilder(args);

// Debug: print connection string
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Connection: {builder.Configuration.GetConnectionString("Default")}");


// Services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<TenantService>();
//builder.Services.AddScoped<CustomerService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); 
}

app.UseHttpsRedirection();

// Map all feature endpoints
app.MapTenantEndpoints();
//app.MapCustomerEndpoints();

app.Run();