using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Service entity.
/// </summary>
public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.DurationMinutes).IsRequired();
        builder.Property(s => s.BasePrice).HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.Status).IsRequired().HasMaxLength(20);

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