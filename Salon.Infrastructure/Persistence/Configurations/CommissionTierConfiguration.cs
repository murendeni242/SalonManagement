using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence.Configurations
{
    public class CommissionTierConfiguration : IEntityTypeConfiguration<CommissionTier>
    {
        public void Configure(EntityTypeBuilder<CommissionTier> builder)
        {
            builder.ToTable("CommissionTiers");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.CommissionRuleId).IsRequired();
            builder.Property(t => t.MinServices).IsRequired();
            builder.Property(t => t.MaxServices);

            builder.Property(t => t.Percentage)
                .IsRequired()
                .HasColumnType("decimal(5,2)");
        }
    }
}
