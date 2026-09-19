using System.Collections.Generic;

namespace Domain.Entities
{
    public class Warehouse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        // Soft-delete flag - false means "hidden from new order/inventory
        // dropdowns" rather than "removed", so historical orders that
        // reference this warehouse still resolve correctly.
        public bool IsActive { get; set; } = true;

        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }
}
