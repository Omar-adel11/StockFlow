using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
        public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
        public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}