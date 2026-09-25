using System.Collections.Generic;
using Domain.common;

namespace Domain.Entities
{
    public class Warehouse : ISoftDelete
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        // Soft-delete flag - false means "hidden from new order/inventory
        // dropdowns" rather than "removed", so historical orders that
        // reference this warehouse still resolve correctly.
        public bool IsActive { get; set; } = true;

        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
        public int? BusinessId { get; set; }
        public Business? Business { get; set; }
    }
}
