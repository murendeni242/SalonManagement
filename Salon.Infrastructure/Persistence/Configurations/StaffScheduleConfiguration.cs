using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core table mapping for StaffSchedule.
/// </summary>
public class StaffScheduleConfiguration : IEntityTypeConfiguration<StaffSchedule>
{
    public void Configure(EntityTypeBuilder<StaffSchedule> builder)
    {
        builder.ToTable("StaffSchedules");

        // ── Primary key ───────────────────────────────────────────
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        // ── Foreign key ───────────────────────────────────────────
        builder.Property(s => s.StaffId)
            .IsRequired();

        builder.HasOne(s => s.Staff)
            .WithMany()
            .HasForeignKey(s => s.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Schedule columns ──────────────────────────────────────
        builder.Property(s => s.DayOfWeek)
            .IsRequired()
            .HasConversion<int>(); 

        builder.Property(s => s.StartTime)
            .IsRequired()
            .HasColumnType("time(7)");

        builder.Property(s => s.EndTime)
            .IsRequired()
            .HasColumnType("time(7)");

        // ── Unique constraint ─────────────────────────────────────
        builder.HasIndex(s => new { s.StaffId, s.DayOfWeek })
            .IsUnique()
            .HasDatabaseName("IX_StaffSchedules_StaffId_DayOfWeek");
    }
}