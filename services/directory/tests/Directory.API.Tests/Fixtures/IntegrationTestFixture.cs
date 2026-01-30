using Directory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Directory.API.Tests.Fixtures;

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
                    // Remove the real DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<DirectoryDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Remove any DbContextOptions
                    var optionsDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions));
                    if (optionsDescriptor != null)
                        services.Remove(optionsDescriptor);

                    // Add DbContext with Testcontainers PostgreSQL
                    services.AddDbContext<DirectoryDbContext>(options =>
                    {
                        options.UseNpgsql(_postgres.GetConnectionString(),
                            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "directory"));
                    });

                    // Ensure database is created and migrations applied
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<DirectoryDbContext>();
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

    /// <summary>
    /// Creates a scoped DbContext for direct database verification in tests.
    /// </summary>
    public DirectoryDbContext CreateDbContext()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<DirectoryDbContext>();
    }
}
