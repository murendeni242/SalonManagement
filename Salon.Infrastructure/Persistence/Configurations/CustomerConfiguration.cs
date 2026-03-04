using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Customer entity.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Phone).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.Property(c => c.Notes).HasMaxLength(2000);
        builder.Property(c => c.DateOfBirth).IsRequired(false);
        builder.Property(c => c.LastVisitAt).IsRequired(false);

        // Unique email when provided
        builder.HasIndex(c => c.Email).IsUnique().HasFilter("[Email] IS NOT NULL");

        // ── Audit fields ───────────────────────────────────────────
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.CreatedBy).IsRequired().HasMaxLength(256);
        builder.Property(c => c.UpdatedAt).IsRequired(false);
        builder.Property(c => c.UpdatedBy).IsRequired(false).HasMaxLength(256);
        builder.Property(c => c.DeletedBy).IsRequired(false).HasMaxLength(256);
        builder.Property(c => c.DeletedAt).IsRequired(false);
        builder.Property(c => c.IsDeleted).IsRequired().HasDefaultValue(false);
    }
}