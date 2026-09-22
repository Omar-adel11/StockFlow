using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PurchaseOrderDtos
    {
        public record PurchaseItemRequest(
        [Required] int ProductId,
        [Range(1, int.MaxValue)] int QuantityOrdered,
        [Range(0.01, double.MaxValue)] decimal AgreedUnitPrice
    );

        public record PurchaseItemResponse(
            int ProductId,
            string ProductName,
            string SKU,
            int QuantityOrdered,
            decimal AgreedUnitPrice,
            decimal LineTotal
        );

        public record PurchaseCreateRequest(
            [Required] int SupplierId,
            [Required] int WarehouseId,
            [Required][MinLength(1)] IReadOnlyCollection<PurchaseItemRequest> Items
        );

        public record PurchaseResponse(
            int Id,
            string PONumber,
            int SupplierId,
            string SupplierName,
            int WarehouseId,
            string WarehouseName,
            string OrderStatus,
            DateTime OrderDate,
            decimal TotalCostAmount,
            IReadOnlyCollection<PurchaseItemResponse> Items
        );
    }
}
