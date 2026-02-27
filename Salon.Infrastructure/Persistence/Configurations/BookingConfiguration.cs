using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Booking entity.
///
/// Key additions over your original:
/// - Global query filter: WHERE IsDeleted = 0 is automatically appended to
///   every LINQ query. Call .IgnoreQueryFilters() when you need deleted rows.
/// - Composite index on (StaffId, BookingDate, IsDeleted) speeds up the overlap check.
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.TotalPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(b => b.BookingDate).IsRequired();
        builder.Property(b => b.StartTime).IsRequired();
        builder.Property(b => b.EndTime).IsRequired();
        builder.Property(b => b.Status).IsRequired();
        builder.Property(b => b.Notes).HasMaxLength(500);

        // Soft-delete global query filter — every query gets WHERE IsDeleted = 0 for free
        builder.HasQueryFilter(b => !b.IsDeleted);

        // Speeds up the overlap detection query in BookingRepository
        builder.HasIndex(b => new { b.StaffId, b.BookingDate, b.IsDeleted });
    }
}