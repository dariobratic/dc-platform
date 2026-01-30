using Configuration.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Configuration.API.Persistence.Configurations;

public class OrganizationSettingConfiguration : IEntityTypeConfiguration<OrganizationSetting>
{
    public void Configure(EntityTypeBuilder<OrganizationSetting> builder)
    {
        builder.ToTable("organization_settings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(4096);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.OrganizationId, x.Key })
            .IsUnique();
    }
}
