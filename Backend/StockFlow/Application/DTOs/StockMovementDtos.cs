using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;

namespace Application.DTOs
{
    public class StockMovementDtos
    {
        public record ManualAdjustmentRequest(
        int ProductId,
        int WarehouseId,
        int QuantityChanged, // e.g., -5 for damaged, +10 for found stock
        StockMovementReason Reason,
        int? ReferenceId
    );

        public record StockMovementResponse(
            int Id,
            int ProductId,
            string ProductName,
            string SKU,
            int WarehouseId,
            string WarehouseName,
            int QuantityChanged,
            string MovementReason,
            int? ReferenceId,
            DateTime TimestampUtc,
            string ExecutedByUserName
        );
    }
}
