using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations
{
    public class CommissionRuleConfiguration : IEntityTypeConfiguration<CommissionRule>
    {
        public void Configure(EntityTypeBuilder<CommissionRule> builder)
        {
            builder.ToTable("CommissionRules");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.StaffId).IsRequired();

            builder.Property(r => r.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(r => r.RateOrAmount)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(r => r.CreatedAt).IsRequired();
            builder.Property(r => r.UpdatedAt).IsRequired();

            builder.HasIndex(r => r.StaffId)
                .IsUnique()
                .HasDatabaseName("IX_CommissionRules_StaffId");

            builder.HasOne(r => r.Staff)
                .WithMany()
                .HasForeignKey(r => r.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Tiers)
                .WithOne(t => t.CommissionRule)
                .HasForeignKey(t => t.CommissionRuleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
