using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Sale entity.
///
/// Design notes:
/// - No soft-delete filter here. Sales are financial records — they are never
///   deleted from the database. Voided and Refunded statuses handle "removal".
/// - OriginalSaleId is a self-referencing FK so refund records link back
///   to the payment they refunded.
/// - Index on BookingId for fast payment history lookup per booking.
/// - Index on PaidAt for efficient date-range queries on the revenue dashboard.
/// </summary>
public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.AmountPaid)
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(s => s.PaymentMethod)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(s => s.Status)
               .IsRequired();

        builder.Property(s => s.PaidAt)
               .IsRequired();

        builder.Property(s => s.Notes)
               .HasMaxLength(500);

        // Self-referencing FK: refund records point back to the original sale
        builder.HasOne<Sale>()
               .WithMany()
               .HasForeignKey(s => s.OriginalSaleId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        // Fast lookup: all payments for a booking
        builder.HasIndex(s => s.BookingId);

        // Fast lookup: date-range queries for revenue reports
        builder.HasIndex(s => s.PaidAt);
    }
}