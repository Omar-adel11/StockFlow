using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<IdentityRole<int>> Roles { get; }
        DbSet<IdentityUserRole<int>> UserRoles { get; }
        DbSet<Product> Products { get; }
        DbSet<Plan> Plans { get; }
        DbSet<Category> Categories { get; }
        DbSet<CustomerAddress> CustomerAddresses { get; }
        DbSet<Supplier> Suppliers { get; }
        DbSet<Warehouse> Warehouses { get; }
        DbSet<InventoryItem> InventoryItems { get; }
        DbSet<StockMovement> StockMovements { get; }
        DbSet<PurchaseOrder> PurchaseOrders { get; }
        DbSet<SalesOrder> SalesOrders { get; }
        DbSet<Customer> Customers { get; }
        DbSet<Business> Business { get; }
        DbSet<TeamInvitation> TeamInvitations { get; }
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
