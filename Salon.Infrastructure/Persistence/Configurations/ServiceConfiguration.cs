using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Service entity.
///
/// Key addition over your original:
/// - Global query filter: WHERE IsDeleted = 0 is automatically appended to every
///   LINQ query. Call .IgnoreQueryFilters() when you need soft-deleted rows.
///   This is exactly the same approach used for the Booking entity.
/// </summary>
public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(s => s.Description)
               .HasMaxLength(1000);

        builder.Property(s => s.BasePrice)
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(s => s.DurationMinutes)
               .IsRequired();

        builder.Property(s => s.Status)
               .IsRequired()
               .HasMaxLength(20);

        // Soft-delete global query filter — every query gets WHERE IsDeleted = 0 for free
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}