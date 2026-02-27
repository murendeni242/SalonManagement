using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Customer entity.
///
/// Key additions over your original:
/// - Global query filter: WHERE IsDeleted = 0 appended to every query automatically.
/// - Unique filtered index on Email (only when not null — walk-in customers don't conflict).
/// - Index on Phone for the fast reception lookup: "what's your number?"
/// - Index on LastName + FirstName for the paged alphabetical list.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.LastName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.Phone)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(c => c.Email)
               .HasMaxLength(150);

        builder.Property(c => c.Notes)
               .HasMaxLength(2000);

        builder.Property(c => c.DateOfBirth)
               .HasColumnType("date"); // date only, no time component

        // Soft-delete global query filter
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Unique email — only when email is not null (walk-ins without email don't conflict)
        builder.HasIndex(c => c.Email)
               .IsUnique()
               .HasFilter("[Email] IS NOT NULL");

        // Fast reception lookup: "what's your number?" — most common search pattern
        builder.HasIndex(c => c.Phone);

        // Alphabetical list ordering
        builder.HasIndex(c => new { c.LastName, c.FirstName });
    }
}