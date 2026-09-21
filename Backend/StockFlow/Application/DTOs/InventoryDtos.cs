using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class InventoryDtos
    {
        public record InventoryResponse(
        int ProductId,
        string ProductName,
        string SKU,
        int WarehouseId,
        string WarehouseName,
        int QuantityOnHand,
        int ReorderLevel,
        bool IsLowStock
    );

        public record LowStockResponse(
            int ProductId,
            string ProductName,
            string SKU,
            int WarehouseId,
            string WarehouseName,
            int QuantityOnHand,
            int ReorderLevel
        );
    
}
}
