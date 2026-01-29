using System.Text.Json;
using Directory.Domain.Entities;
using Directory.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Directory.Infrastructure.Configuration;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Slug)
            .HasConversion(
                slug => slug.Value,
                value => Slug.FromExisting(value))
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(o => o.Slug)
            .IsUnique();

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.Settings)
            .HasConversion(
                settings => JsonSerializer.Serialize(settings.Values, JsonOptions),
                json => OrganizationSettings.Create(
                    JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions)
                    ?? new Dictionary<string, string>()))
            .HasColumnType("jsonb");

        builder.Property(o => o.CreatedAt).IsRequired();

        builder.HasMany(o => o.Workspaces)
            .WithOne(w => w.Organization)
            .HasForeignKey(w => w.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(o => o.Workspaces)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(o => o.DeletedAt == null);

        builder.Ignore(o => o.DomainEvents);
    }
}
