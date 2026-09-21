using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using static Application.DTOs.StockMovementDtos;

namespace Application.Services
{
    public class StockMovementService(IAppDbContext _context,ICurrentUserService currentUserService) : IStockMovementService
    {
        private DbSet<StockMovement> stockMovements => _context.StockMovements;
        public async Task<bool> CreateManualAdjustmentAsync(ManualAdjustmentRequest request)
        {
            var movement = new StockMovement()
            {
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                ChangeQuantity = request.QuantityChanged,
                Reason = request.Reason,
                ExecutedByUserId = currentUserService.UserId,
                ExecutedByUserName = currentUserService.UserName,
                ReferenceId = request.ReferenceId,
                CreatedAtUtc = DateTime.UtcNow
            };
            await stockMovements.AddAsync(movement);
            return await _context.SaveChangesAsync() > 0;
        }


        public async Task<IReadOnlyCollection<StockMovementResponse>> GetMovementsByProductAsync(int productId)
        {
            return await stockMovements
                 .AsNoTracking()
                 .Where(m => m.ProductId == productId)
                 .OrderByDescending(m => m.CreatedAtUtc)
                 .Select(m => new StockMovementResponse(
                     m.Id,
                     m.ProductId,
                     m.Product != null ? m.Product.Name : string.Empty,
                     m.Product != null ? m.Product.SKU : string.Empty,
                     m.WarehouseId,
                     m.Warehouse != null ? m.Warehouse.Name : string.Empty,
                     m.ChangeQuantity,
                     m.Reason.ToString(),
                     m.ReferenceId,
                     m.CreatedAtUtc,
                     m.ExecutedByUserName
                 ))
                 .ToListAsync();
        }

        public async Task<IReadOnlyCollection<StockMovementResponse>> GetMovementsByWarehouseAsync(int warehouseId)
        {
            return await stockMovements
                .AsNoTracking()
                .Where(m => m.WarehouseId == warehouseId)
                .OrderByDescending(m => m.CreatedAtUtc)
                .Select(m => new StockMovementResponse(
                    m.Id,
                    m.ProductId,
                    m.Product != null ? m.Product.Name : string.Empty,
                    m.Product != null ? m.Product.SKU : string.Empty,
                    m.WarehouseId,
                    m.Warehouse != null ? m.Warehouse.Name : string.Empty,
                    m.ChangeQuantity,
                    m.Reason.ToString(),
                    m.ReferenceId,
                    m.CreatedAtUtc,
                    m.ExecutedByUserName
                ))
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<StockMovementResponse>> GetRecentMovementsAsync(int count = 20)
        {
            return await stockMovements
                .AsNoTracking()
                .OrderByDescending(m => m.CreatedAtUtc)
                .Take(count)
                .Select(m => new StockMovementResponse(
                    m.Id,
                    m.ProductId,
                    m.Product != null ? m.Product.Name : string.Empty,
                    m.Product != null ? m.Product.SKU : string.Empty,
                    m.WarehouseId,
                    m.Warehouse != null ? m.Warehouse.Name : string.Empty,
                    m.ChangeQuantity,
                    m.Reason.ToString(),
                    m.ReferenceId,
                    m.CreatedAtUtc,
                    m.ExecutedByUserName
                ))
                .ToListAsync();
        }

        #region Old Code You Want To See The Generated Sql Query From It
        //public async Task<IReadOnlyCollection<StockMovementResponse>> GetMovementsByProductAsync(int productId)
        //{
        //    var result = await stockMovements.AsNoTracking()
        //                                     .Include(m => m.Product).Include(m=>m.Warehouse)
        //                                     .Where(m=>m.ProductId == productId).OrderByDescending(m=>m.CreatedAtUtc).ToListAsync();
        //    return result.Select(MapToResponse).ToList();
        //}

        //public async Task<IReadOnlyCollection<StockMovementResponse>> GetMovementsByWarehouseAsync(int warehouseId)
        //{
        //    var result = await stockMovements.AsNoTracking()
        //                                     .Include(m => m.Product).Include(m => m.Warehouse)
        //                                     .Where(m => m.WarehouseId == warehouseId).OrderByDescending(m => m.CreatedAtUtc).ToListAsync();
        //    return result.Select(MapToResponse).ToList();
        //}

        //public async Task<IReadOnlyCollection<StockMovementResponse>> GetRecentMovementsAsync(int count = 20)
        //{
        //    var result = await stockMovements.AsNoTracking()
        //                                     .Include(m => m.Product).Include(m => m.Warehouse)
        //                                     .OrderByDescending(m=>m.CreatedAtUtc).Take(count).ToListAsync();
        //    return result.Select(MapToResponse).ToList();
        //} 
        #endregion

        private StockMovementResponse MapToResponse(StockMovement stockMovement)
        {
            return new StockMovementResponse(stockMovement.Id,
                stockMovement.ProductId, stockMovement.Product.Name ?? string.Empty,
                stockMovement.Product.SKU ?? string.Empty, stockMovement.WarehouseId,
                stockMovement.Warehouse.Name ?? string.Empty, stockMovement.ChangeQuantity,
                stockMovement.Reason.ToString(),stockMovement.ReferenceId,
                stockMovement.CreatedAtUtc, stockMovement.ExecutedByUserName);
        }
    }
}
