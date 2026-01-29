using Audit.Application.Interfaces;
using Audit.Infrastructure.Persistence;
using Audit.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuditDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("AuditDb"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "audit"));
        });

        services.AddScoped<IAuditEntryRepository, AuditEntryRepository>();

        return services;
    }
}
