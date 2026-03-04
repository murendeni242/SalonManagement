using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Staff entity.
/// </summary>
public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.LastName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Phone).HasMaxLength(20);
        builder.Property(s => s.Email).HasMaxLength(256);
        builder.Property(s => s.Role).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Status).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Specialisations).HasMaxLength(500);

        builder.HasIndex(s => s.Email).IsUnique().HasFilter("[Email] IS NOT NULL");

        // ── Audit fields ───────────────────────────────────────────
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.CreatedBy).IsRequired().HasMaxLength(256);
        builder.Property(s => s.UpdatedAt).IsRequired(false);
        builder.Property(s => s.UpdatedBy).IsRequired(false).HasMaxLength(256);
        builder.Property(s => s.DeletedBy).IsRequired(false).HasMaxLength(256);
        builder.Property(s => s.DeletedAt).IsRequired(false);
        builder.Property(s => s.IsDeleted).IsRequired().HasDefaultValue(false);
    }
}