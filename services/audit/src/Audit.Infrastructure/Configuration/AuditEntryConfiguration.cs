using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Infrastructure.Configuration;

public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Timestamp).IsRequired();
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.UserEmail).HasMaxLength(320);
        builder.Property(e => e.Action).IsRequired().HasMaxLength(200);
        builder.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.EntityId).IsRequired();
        builder.Property(e => e.Details).HasColumnType("jsonb");
        builder.Property(e => e.IpAddress).HasMaxLength(45);
        builder.Property(e => e.UserAgent).HasMaxLength(500);
        builder.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.CorrelationId).HasMaxLength(100);

        // Performance indexes for common query patterns
        builder.HasIndex(e => e.OrganizationId);
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ServiceName);
        builder.HasIndex(e => new { e.OrganizationId, e.Timestamp });

        builder.Ignore(e => e.DomainEvents);
    }
}
