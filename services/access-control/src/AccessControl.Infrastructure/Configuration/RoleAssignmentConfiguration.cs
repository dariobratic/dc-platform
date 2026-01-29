using AccessControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessControl.Infrastructure.Configuration;

public class RoleAssignmentConfiguration : IEntityTypeConfiguration<RoleAssignment>
{
    public void Configure(EntityTypeBuilder<RoleAssignment> builder)
    {
        builder.ToTable("role_assignments");

        builder.HasKey(ra => ra.Id);

        builder.Property(ra => ra.RoleId)
            .IsRequired();

        builder.Property(ra => ra.UserId)
            .IsRequired();

        builder.Property(ra => ra.ScopeId)
            .IsRequired();

        builder.Property(ra => ra.ScopeType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ra => ra.AssignedAt)
            .IsRequired();

        builder.Property(ra => ra.AssignedBy)
            .IsRequired();

        builder.HasIndex(ra => new { ra.RoleId, ra.UserId, ra.ScopeId })
            .IsUnique();

        builder.HasOne(ra => ra.Role)
            .WithMany()
            .HasForeignKey(ra => ra.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(ra => ra.DomainEvents);
    }
}
