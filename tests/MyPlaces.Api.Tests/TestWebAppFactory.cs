using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MyPlaces.Api.Common;

namespace MyPlaces.Api.Tests;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    // Fixed DB name per factory instance so all requests within a test class share the same store
    private readonly string _dbName = "TestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all EF/DB provider-related descriptors to avoid dual-provider conflict
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>))
                .ToList();

            foreach (var d in descriptorsToRemove)
                services.Remove(d);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });

        builder.UseEnvironment("Testing");
    }
}
