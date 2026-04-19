using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyPlaces.Api.Common;

namespace MyPlaces.Api.Tests;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    private string _dbName = "TestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext
            var descriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Use in-memory database with a unique name per factory
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase(_dbName));
        });
    }

    public override async ValueTask DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }
}
