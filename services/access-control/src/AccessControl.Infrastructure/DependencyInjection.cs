using AccessControl.Application.Interfaces;
using AccessControl.Infrastructure.Persistence;
using AccessControl.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccessControl.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AccessControlDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("AccessControlDb"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "access_control")));

        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleAssignmentRepository, RoleAssignmentRepository>();

        return services;
    }
}
