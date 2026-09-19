using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using static Application.DTOs.StockMovementDtos;

namespace Application.Interfaces
{
    public interface IStockMovementService
    {
        // Read operations for dashboards and audit reports
        Task<IReadOnlyCollection<StockMovementResponse>> GetMovementsByProductAsync(int productId);
        Task<IReadOnlyCollection<StockMovementResponse>> GetMovementsByWarehouseAsync(int warehouseId);
        Task<IReadOnlyCollection<StockMovementResponse>> GetRecentMovementsAsync(int count = 20);

        // Manual stock adjustment (e.g., damaged stock, audit correction)
        Task<bool> CreateManualAdjustmentAsync(ManualAdjustmentRequest request, int userId);
    }

}
