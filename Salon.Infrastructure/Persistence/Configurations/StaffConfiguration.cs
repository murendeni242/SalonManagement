using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Staff entity.
///
/// Key additions over your original:
/// - Global query filter: WHERE IsDeleted = 0 appended to every query automatically.
///   Call .IgnoreQueryFilters() when you need soft-deleted records.
/// - Unique filtered index on Email — enforces uniqueness only when Email is not null
///   so staff members without email do not conflict with each other.
/// - Index on Status for the common query pattern "get all Active staff".
/// </summary>
public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FirstName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(s => s.LastName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(s => s.Phone)
               .HasMaxLength(20);

        builder.Property(s => s.Email)
               .HasMaxLength(150);

        builder.Property(s => s.Role)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(s => s.Status)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(s => s.Specialisations)
               .HasMaxLength(500);

        // Soft-delete global query filter — every query gets WHERE IsDeleted = 0 for free
        builder.HasQueryFilter(s => !s.IsDeleted);

        // Unique email — but only when email is not null (staff without email don't conflict)
        builder.HasIndex(s => s.Email)
               .IsUnique()
               .HasFilter("[Email] IS NOT NULL");

        // Fast lookup: "get all Active staff" is the most common list query
        builder.HasIndex(s => s.Status);
    }
}