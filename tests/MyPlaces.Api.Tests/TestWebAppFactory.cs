using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyPlaces.Api.Common;
using Testcontainers.PostgreSql;

namespace MyPlaces.Api.Tests;

public class TestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:18-alpine")
        .WithDatabase("myplaces_test")
        .WithUsername("myplaces")
        .WithPassword("myplaces_test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use the same key as Program.cs reads in AddDbContext
        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            _postgres.GetConnectionString());

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
