using Directory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Directory.Infrastructure.Configuration;

public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("memberships");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.UserId).IsRequired();

        builder.Property(m => m.Role)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.JoinedAt).IsRequired();

        builder.HasIndex(m => new { m.WorkspaceId, m.UserId })
            .IsUnique();
    }
}
