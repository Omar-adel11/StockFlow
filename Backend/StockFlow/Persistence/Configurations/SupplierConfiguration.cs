using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> entity)
        {
            entity.ToTable("Suppliers");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name).IsRequired().HasMaxLength(150);
            entity.Property(s => s.ContactEmail).HasMaxLength(200);
            entity.Property(s => s.ContactPhone).HasMaxLength(30);
            entity.Property(s => s.Address).HasMaxLength(300);
            entity.Property(s => s.IsActive).IsRequired();
        }
    }
}
