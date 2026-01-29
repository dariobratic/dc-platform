using Directory.Application.Interfaces;
using Directory.Infrastructure.Persistence;
using Directory.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Directory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DirectoryDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DirectoryDb"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "directory")));

        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();

        return services;
    }
}
