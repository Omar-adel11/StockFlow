using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using static Application.DTOs.InventoryDtos;

namespace Application.Interfaces
{
    public interface IInventoryService
    {
        // Gets stock levels for all products across warehouses
        Task<IReadOnlyCollection<InventoryResponse>> GetInventoryOverviewAsync();

        // Gets stock levels for a specific product across all warehouses
        Task<IReadOnlyCollection<InventoryResponse>> GetInventoryByProductAsync(int productId);

        // Gets stock levels for all products in a specific warehouse
        Task<IReadOnlyCollection<InventoryResponse>> GetInventoryByWarehouseAsync(int warehouseId);

        // Low stock alert query for dashboard
        Task<IReadOnlyCollection<LowStockResponse>> GetLowStockItemsAsync();
    
}
}
