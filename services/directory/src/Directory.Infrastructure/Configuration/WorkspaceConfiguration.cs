using Directory.Domain.Entities;
using Directory.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Directory.Infrastructure.Configuration;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("workspaces");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Slug)
            .HasConversion(
                slug => slug.Value,
                value => Slug.FromExisting(value))
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(w => new { w.OrganizationId, w.Slug })
            .IsUnique();

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(w => w.CreatedAt).IsRequired();

        builder.HasMany(w => w.Memberships)
            .WithOne(m => m.Workspace)
            .HasForeignKey(m => m.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Memberships)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(w => w.Invitations)
            .WithOne(i => i.Workspace)
            .HasForeignKey(i => i.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Invitations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(w => w.DeletedAt == null);

        builder.Ignore(w => w.DomainEvents);
    }
}
