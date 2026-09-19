using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> entity)
        {
            entity.ToTable("Customers");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Email).HasMaxLength(200);
            entity.Property(c => c.Phone).HasMaxLength(30);
        }
    }
}
