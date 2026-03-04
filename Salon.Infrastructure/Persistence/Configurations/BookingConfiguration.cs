using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Booking entity.
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        // ── Core booking fields ────────────────────────────────────
        builder.Property(b => b.TotalPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(b => b.BookingDate).IsRequired();
        builder.Property(b => b.StartTime).IsRequired();
        builder.Property(b => b.EndTime).IsRequired();
        builder.Property(b => b.Status).IsRequired().HasConversion<string>();
        builder.Property(b => b.Notes).HasMaxLength(500);

        // ── Audit fields (from AuditableEntity) ───────────────────
        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.CreatedBy).IsRequired().HasMaxLength(256);
        builder.Property(b => b.UpdatedAt).IsRequired(false);
        builder.Property(b => b.UpdatedBy).IsRequired(false).HasMaxLength(256);
        builder.Property(b => b.DeletedBy).IsRequired(false).HasMaxLength(256);
        builder.Property(b => b.DeletedAt).IsRequired(false);
        builder.Property(b => b.IsDeleted).IsRequired().HasDefaultValue(false);

        // ── Indexes ────────────────────────────────────────────────
        // Speeds up the overlap detection query in BookingRepository
        builder.HasIndex(b => new { b.StaffId, b.BookingDate, b.IsDeleted });
    }
}