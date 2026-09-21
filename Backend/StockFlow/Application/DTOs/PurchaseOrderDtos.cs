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
        [property: Required] int ProductId,
        [property: Range(1, int.MaxValue)] int QuantityOrdered,
        [property: Range(0.01, double.MaxValue)] decimal AgreedUnitPrice
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
            [property: Required] int SupplierId,
            [property: Required] int WarehouseId,
            [property: Required, MinLength(1)] IReadOnlyCollection<PurchaseItemRequest> Items
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
