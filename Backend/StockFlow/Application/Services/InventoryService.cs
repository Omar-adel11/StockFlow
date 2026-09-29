using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class InventoryService(IAppDbContext _context) : IInventoryService
    {
        private DbSet<InventoryItem> InventoryItems => _context.InventoryItems;

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

            return await query
                .Select(ToInventoryResponse)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<InventoryDtos.InventoryResponse>> GetInventoryByProductAsync(int productId)
            => await GetInventoryAsync(productId: productId);

        public async Task<IReadOnlyCollection<InventoryDtos.InventoryResponse>> GetInventoryByWarehouseAsync(int warehouseId)
            => await GetInventoryAsync(warehouseId: warehouseId);

        public async Task<IReadOnlyCollection<InventoryDtos.InventoryResponse>> GetInventoryOverviewAsync()
            => await GetInventoryAsync();

        public async Task<IReadOnlyCollection<InventoryDtos.LowStockResponse>> GetLowStockItemsAsync()
        {
            return await InventoryItems
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
        }
    }
}