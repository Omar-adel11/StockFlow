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

        public async Task<IReadOnlyCollection<InventoryDtos.InventoryResponse>> GetInventoryByProductAsync(int productId)
        {
            return await InventoryItems
                .AsNoTracking()
                .Where(i => i.ProductId == productId)
                .Select(ToInventoryResponse)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<InventoryDtos.InventoryResponse>> GetInventoryByWarehouseAsync(int warehouseId)
        {
            return await InventoryItems
                .AsNoTracking()
                .Where(i => i.WarehouseId == warehouseId)
                .Select(ToInventoryResponse)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<InventoryDtos.InventoryResponse>> GetInventoryOverviewAsync()
        {
            return await InventoryItems
                .AsNoTracking()
                .Select(ToInventoryResponse)
                .ToListAsync();
        }

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