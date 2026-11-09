using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions.NotFound;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class InventoryService(IAppDbContext _context, ILogger<InventoryService> _logger) : IInventoryService
    {
        private DbSet<InventoryItem> InventoryItems => _context.InventoryItems;
        private DbSet<Warehouse> Warehouses => _context.Warehouses;
        private DbSet<Product> Products => _context.Products;

        // Reusable Expression Tree for LINQ-to-SQL translation
        private static readonly Expression<Func<InventoryItem, InventoryDtos.InventoryResponse>> ToInventoryResponse =
            i => new InventoryDtos.InventoryResponse(
                i.ProductId,
                i.Product != null ? i.Product.Name : string.Empty,
                i.Product != null ? i.Product.SKU : string.Empty,
                i.WarehouseId,
                i.Warehouse != null ? i.Warehouse.Name : string.Empty,
                i.QuantityOnHand,
                i.Product != null ? i.Product.ReorderLevel : 0,
                i.QuantityOnHand <= (i.Product != null ? i.Product.ReorderLevel : 0)
            );



        public async Task<IReadOnlyCollection<InventoryDtos.InventoryResponse>> GetInventoryAsync(
       int? productId = null,
       int? warehouseId = null,
       bool lowStockOnly = false,
       string? searchTerm = null)
        {
            var query = InventoryItems.Include(i => i.Product)
                .AsNoTracking();

            if (productId.HasValue)
            {
                query = query.Where(i => i.ProductId == productId.Value);
            }

            if (warehouseId.HasValue)
            {
                query = query.Where(i => i.WarehouseId == warehouseId.Value);
            }

            if (lowStockOnly)
            {
                query = query.Where(i => i.QuantityOnHand <= i.Product.ReorderLevel);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(i =>
                    i.Product.Name.Contains(searchTerm) ||
                    i.Product.SKU.Contains(searchTerm));
            }

            var items = await query.Select(ToInventoryResponse).ToListAsync();
            _logger.LogDebug("Retrieved {ResultCount} inventory records. ProductFilterApplied: {ProductFilterApplied}, WarehouseFilterApplied: {WarehouseFilterApplied}, LowStockOnly: {LowStockOnly}, SearchApplied: {SearchApplied}", items.Count, productId.HasValue, warehouseId.HasValue, lowStockOnly, !string.IsNullOrWhiteSpace(searchTerm));
            return items;
        }

        public async Task<IReadOnlyCollection<InventoryDtos.InventoryResponse>> GetInventoryByProductAsync(int productId)
            => await GetInventoryAsync(productId: productId);

        public async Task<IReadOnlyCollection<InventoryDtos.InventoryResponse>> GetInventoryByWarehouseAsync(int warehouseId)
            => await GetInventoryAsync(warehouseId: warehouseId);

        public async Task<IReadOnlyCollection<InventoryDtos.InventoryResponse>> GetInventoryOverviewAsync()
            => await GetInventoryAsync();

        public async Task<IReadOnlyCollection<InventoryDtos.LowStockResponse>> GetLowStockItemsAsync()
        {
            var items = await InventoryItems
                .AsNoTracking()
                .Where(i => i.QuantityOnHand <= (i.Product != null ? i.Product.ReorderLevel : 0))
                .Select(i => new InventoryDtos.LowStockResponse(
                    i.ProductId,
                    i.Product != null ? i.Product.Name : string.Empty,
                    i.Product != null ? i.Product.SKU : string.Empty,
                    i.WarehouseId,
                    i.Warehouse != null ? i.Warehouse.Name : string.Empty,
                    i.QuantityOnHand,
                    i.Product != null ? i.Product.ReorderLevel : 0
                ))
                .ToListAsync();
            _logger.LogDebug("Retrieved {ResultCount} low-stock inventory records", items.Count);
            return items;
        }

       
    }
}