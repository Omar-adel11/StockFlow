using System;
using System.Linq.Expressions;
using System.Reflection;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>, IAppDbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Business> Business => Set<Business>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
        public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
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
        public DbSet<TeamInvitation> TeamInvitations { get; set; }

        public int? CurrentBusinessId
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("business_id")?.Value;
                return int.TryParse(claim, out var businessId) ? businessId : null;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                if (clrType == typeof(User)) continue;

                var businessIdProperty = clrType.GetProperty("BusinessId");
                bool hasBusinessId = businessIdProperty != null &&
                                    (businessIdProperty.PropertyType == typeof(int) ||
                                     businessIdProperty.PropertyType == typeof(int?));

                var isActiveProperty = clrType.GetProperty("IsActive");
                bool hasIsActive = isActiveProperty != null &&
                                   isActiveProperty.PropertyType == typeof(bool);

                if (hasBusinessId || hasIsActive)
                {
                    var method = typeof(AppDbContext)
                        .GetMethod(nameof(GetGlobalQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)?
                        .MakeGenericMethod(clrType);

                    var filter = method?.Invoke(this, new object[] { hasBusinessId, hasIsActive });

                    if (filter != null)
                    {
                        modelBuilder.Entity(clrType).HasQueryFilter((LambdaExpression)filter);
                    }
                }
            }
        }

        private Expression<Func<TEntity, bool>> GetGlobalQueryFilter<TEntity>(
            bool hasBusinessId,
            bool hasIsActive)
            where TEntity : class
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "e");
            Expression? finalExpression = null;

            if (hasBusinessId)
            {
                MemberExpression businessIdProperty = Expression.Property(parameter, "BusinessId");
                MemberExpression currentBusinessIdProperty = Expression.Property(Expression.Constant(this), nameof(CurrentBusinessId));

                Expression tenantEqualExpression = Expression.Equal(
                    Expression.Convert(businessIdProperty, typeof(int?)),
                    currentBusinessIdProperty
                );

                finalExpression = tenantEqualExpression;
            }

            if (hasIsActive)
            {
                MemberExpression isActiveProperty = Expression.Property(parameter, "IsActive");
                Expression isActiveExpression = Expression.Equal(isActiveProperty, Expression.Constant(true));

                finalExpression = finalExpression == null
                    ? isActiveExpression
                    : Expression.AndAlso(finalExpression, isActiveExpression);
            }

            return Expression.Lambda<Func<TEntity, bool>>(finalExpression!, parameter);
        }
    }
}