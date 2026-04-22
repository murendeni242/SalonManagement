using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations
{
    public class CommissionConfiguration : IEntityTypeConfiguration<Commission>
    {
        public void Configure(EntityTypeBuilder<Commission> builder)
        {
            builder.ToTable("Commissions");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.SaleId).IsRequired();
            builder.Property(c => c.StaffId).IsRequired();

            builder.Property(c => c.GrossAmount)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(c => c.Amount)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(c => c.RateApplied)
                .IsRequired()
                .HasColumnType("decimal(10,4)");

            builder.Property(c => c.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(c => c.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(c => c.PaidAt);
            builder.Property(c => c.PaidBy).HasMaxLength(200);
            builder.Property(c => c.CreatedAt).IsRequired();

            builder.HasIndex(c => c.SaleId)
                .IsUnique()
                .HasDatabaseName("IX_Commissions_SaleId");

            builder.HasIndex(c => new { c.StaffId, c.CreatedAt })
                .HasDatabaseName("IX_Commissions_StaffId_CreatedAt");
        }
    }
}
