using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Product> Products { get; }
        DbSet<Category> Categories { get; }
        DbSet<CustomerAddress> CustomerAddresses { get; }
        DbSet<Supplier> Suppliers { get; }
        DbSet<Warehouse> Warehouses { get; }
        DbSet<InventoryItem> InventoryItems { get; }
        DbSet<StockMovement> StockMovements { get; }
        DbSet<PurchaseOrder> PurchaseOrders { get; }
        DbSet<SalesOrder> SalesOrders { get; }
        DbSet<Customer> Customers { get; }
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
