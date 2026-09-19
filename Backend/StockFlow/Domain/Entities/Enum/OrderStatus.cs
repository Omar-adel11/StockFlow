namespace Domain.Entities.Enum
{
    public enum OrderStatus
    {
        Pending,
        Completed, // "Received" for a PurchaseOrder, "Fulfilled" for a SalesOrder
        Cancelled
    }
}
