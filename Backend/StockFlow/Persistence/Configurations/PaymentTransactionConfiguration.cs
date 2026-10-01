using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("PaymentTransactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            // CRITICAL FOR IDEMPOTENCY: Index on ExternalTransactionId
            builder.HasIndex(t => t.ExternalTransactionId)
                .IsUnique();

            builder.HasOne(t => t.TenantSubscription)
                .WithMany()
                .HasForeignKey(t => t.TenantSubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
