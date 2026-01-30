using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Configuration.API.Persistence;

public class ConfigurationDbContextFactory : IDesignTimeDbContextFactory<ConfigurationDbContext>
{
    public ConfigurationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=dc_platform;Username=postgres;Password=postgres;Search Path=configuration");

        return new ConfigurationDbContext(optionsBuilder.Options);
    }
}
