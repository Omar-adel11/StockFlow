using System;
using Domain.Entities.Enum;

namespace Domain.Entities
{
    public class StockMovement
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        // Positive = stock added (purchase received, upward adjustment).
        // Negative = stock removed (sale fulfilled, downward adjustment).
        public int ChangeQuantity { get; set; }

        public StockMovementReason Reason { get; set; }

        // The Id of whatever caused this row - a PurchaseOrderId for
        // Reason.Purchase, a SalesOrderId for Reason.Sale, null for a
        // manual Reason.Adjustment. Deliberately not a real foreign key,
        // since it points at a different table depending on Reason.
        public int? ReferenceId { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public string ExecutedByUserId {  get; set; }
        public string ExecutedByUserName { get; set; }
        public int? BusinessId { get; set; }
        public Business? Business { get; set; }
    }
}
