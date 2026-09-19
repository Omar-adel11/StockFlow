using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class PlanFeatureConfiguration : IEntityTypeConfiguration<PlanFeature>
    {
        public void Configure(EntityTypeBuilder<PlanFeature> entity)
        {
            entity.ToTable("PlanFeatures");

            entity.HasKey(f => f.Id);

            entity.Property(f => f.Name).IsRequired().HasMaxLength(200);
        }
    }
}
