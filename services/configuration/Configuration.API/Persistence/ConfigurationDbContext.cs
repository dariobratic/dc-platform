using Configuration.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Configuration.API.Persistence;

public class ConfigurationDbContext : DbContext
{
    public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrganizationSetting> OrganizationSettings => Set<OrganizationSetting>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("configuration");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigurationDbContext).Assembly);
    }
}
