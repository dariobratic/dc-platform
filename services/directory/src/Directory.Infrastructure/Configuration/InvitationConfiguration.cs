using Directory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Directory.Infrastructure.Configuration;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("invitations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(i => i.Role)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(i => i.Token)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(i => i.Token)
            .IsUnique();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(i => i.ExpiresAt).IsRequired();
        builder.Property(i => i.CreatedAt).IsRequired();
        builder.Property(i => i.InvitedBy).IsRequired();

        builder.Ignore(i => i.DomainEvents);
    }
}
