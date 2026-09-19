using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> entity)
        {
            entity.ToTable("Categories");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(500);
        }
    }
}
