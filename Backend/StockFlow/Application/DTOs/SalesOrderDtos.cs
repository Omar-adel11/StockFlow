using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class SalesOrderDtos
    {
        public record SalesItemRequest(
        [Required] int ProductId,
        [Range(1, int.MaxValue)] int QuantitySold,
        [Range(0.01, double.MaxValue)] decimal BilledUnitPrice
    );

        public record SalesItemResponse(
            int ProductId,
            string ProductName,
            string SKU,
            int QuantitySold,
            decimal BilledUnitPrice,
            decimal LineTotal
        );

        public record SalesCreateRequest(
            [Required] int CustomerId,
            [Required] int WarehouseId,
            [Required][MinLength(1)] IReadOnlyCollection<SalesItemRequest> Items
        );

        public record SalesResponse(
            int Id,
            string InvoiceNumber,
            int CustomerId,
            string CustomerName,
            int WarehouseId,
            string WarehouseName,
            string OrderStatus,
            DateTime OrderDate,
            decimal TotalSaleAmount,
            IReadOnlyCollection<SalesItemResponse> Items
        );
    }
}
