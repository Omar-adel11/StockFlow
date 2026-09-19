using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class PlanConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> entity)
        {
            entity.ToTable("Plans");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Price).HasColumnType("decimal(10,2)");
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.IsActive).IsRequired();

            entity.Property(p => p.BillingCycle)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            entity.HasMany(p => p.Features)
                  .WithOne(f => f.Plan)
                  .HasForeignKey(f => f.PlanId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
