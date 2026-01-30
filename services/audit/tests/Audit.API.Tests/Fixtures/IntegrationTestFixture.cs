using Audit.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Audit.API.Tests.Fixtures;

public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("dc_platform_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public HttpClient Client { get; private set; } = null!;
    private WebApplicationFactory<Program> _factory = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AuditDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    var optionsDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions));
                    if (optionsDescriptor != null)
                        services.Remove(optionsDescriptor);

                    services.AddDbContext<AuditDbContext>(options =>
                    {
                        options.UseNpgsql(_postgres.GetConnectionString(),
                            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "audit"));
                    });

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
                    db.Database.Migrate();
                });

                builder.UseEnvironment("Development");
            });

        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    public AuditDbContext CreateDbContext()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    }
}
