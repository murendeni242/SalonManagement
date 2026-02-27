using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the shared AuditLog table.
///
/// The composite index on (EntityName, EntityId, ChangedAt) makes
/// "give me all Booking #5 changes" a fast index seek regardless of
/// how many total audit rows exist in the table.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(500).IsRequired();
        builder.Property(a => a.ChangedBy).HasMaxLength(256).IsRequired();
        builder.Property(a => a.ChangedAt).IsRequired();

        // OldValues / NewValues are JSON — no fixed max length required
        builder.Property(a => a.OldValues);
        builder.Property(a => a.NewValues);

        // Fast lookup: all audit entries for a specific entity record
        builder.HasIndex(a => new { a.EntityName, a.EntityId, a.ChangedAt });
    }
}