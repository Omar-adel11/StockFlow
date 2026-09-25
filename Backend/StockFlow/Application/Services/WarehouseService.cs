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

namespace Application.Services
{
    public class WarehouseService(IAppDbContext _context) : IWarehouseService
    {
        private DbSet<Warehouse> WarehouseSet => _context.Warehouses;

        // Expression Tree for SQL projection in GetAllWarehousesAsync
        private static readonly Expression<Func<Warehouse, WarehouseDtos.WarehouseResponse>> MapToResponseExpression =
            w => new WarehouseDtos.WarehouseResponse(
                w.Id,
                w.Name,
                w.Location,
                w.IsActive
            );

        public async Task<IReadOnlyCollection<WarehouseDtos.WarehouseResponse>> GetAllWarehousesAsync()
        {
            return await WarehouseSet
                .AsNoTracking()
                .Select(MapToResponseExpression)
                .ToListAsync();
        }

        public async Task<WarehouseDtos.WarehouseResponse?> GetWarehouseAsync(int id)
        {
            var warehouse = await GetWarehouseEntityAsync(id);
            return MapToResponse(warehouse);
        }

        public async Task<WarehouseDtos.WarehouseResponse> CreateWarehouseAsync(WarehouseDtos.WarehouseCreateRequest createRequest, int businessId)
        {
            var warehouse = new Warehouse
            {
                Name = createRequest.WarehouseName,
                Location = createRequest.LocationAddress,
                IsActive = true,
                BusinessId = businessId
            };

            await WarehouseSet.AddAsync(warehouse);
            await _context.SaveChangesAsync();

            return MapToResponse(warehouse);
        }

        public async Task<WarehouseDtos.WarehouseResponse?> UpdateWarehouseAsync(int id, WarehouseDtos.WarehouseUpdateRequest updateRequest)
        {
            var warehouse = await GetWarehouseEntityAsync(id);

            // Handle Soft Delete / Deactivation safety check
            if (!updateRequest.IsActive && warehouse.IsActive)
            {
                await ValidateNoActiveStockAsync(warehouse.Id, warehouse.Name);
            }

            warehouse.Name = updateRequest.WarehouseName;
            warehouse.Location = updateRequest.LocationAddress;
            warehouse.IsActive = updateRequest.IsActive;

            await _context.SaveChangesAsync();

            return MapToResponse(warehouse);
        }

        public async Task<bool> DeleteWarehouseAsync(int id)
        {
            var warehouse = await GetWarehouseEntityAsync(id);
           

            await ValidateNoActiveStockAsync(warehouse.Id, warehouse.Name);

            _context.Warehouses.Remove(warehouse);
            return await _context.SaveChangesAsync() > 0;
        }

        #region Helper Methods

        private async Task ValidateNoActiveStockAsync(int warehouseId, string warehouseName)
        {
            var hasInventory = await _context.InventoryItems
                .AnyAsync(i => i.WarehouseId == warehouseId && i.QuantityOnHand > 0);

            if (hasInventory)
            {
                throw new InvalidOperationException($"Cannot deactivate or delete warehouse '{warehouseName}' because it currently holds active stock.");
            }
        }

        private async Task<Warehouse> GetWarehouseEntityAsync(int id)
        {
            var warehouse = await WarehouseSet.FirstOrDefaultAsync(w => w.Id == id);
            if (warehouse == null)
            {
                throw new WarehouseNotFoundException();
            }

            return warehouse;
        }

        private static WarehouseDtos.WarehouseResponse MapToResponse(Warehouse warehouse)
        {
            return new WarehouseDtos.WarehouseResponse(
                warehouse.Id,
                warehouse.Name,
                warehouse.Location,
                warehouse.IsActive
            );
        }

        #endregion
    }
}