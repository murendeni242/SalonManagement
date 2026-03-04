using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Sale entity.
/// </summary>
public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.AmountPaid).HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.PaymentMethod).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>();
        builder.Property(s => s.PaidAt).IsRequired();
        builder.Property(s => s.Notes).HasMaxLength(500);
        builder.Property(s => s.ProcessedByStaffId).IsRequired(false);
        builder.Property(s => s.OriginalSaleId).IsRequired(false);

        // ── Audit fields ───────────────────────────────────────────
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.CreatedBy).IsRequired().HasMaxLength(256);
        builder.Property(s => s.UpdatedAt).IsRequired(false);
        builder.Property(s => s.UpdatedBy).IsRequired(false).HasMaxLength(256);

        // Explicitly ignore soft-delete fields on Sale
        builder.Ignore(s => s.IsDeleted);
        builder.Ignore(s => s.DeletedAt);
        builder.Ignore(s => s.DeletedBy);

        // Index for fast "get all sales for this booking" lookups
        builder.HasIndex(s => s.BookingId);
    }
}