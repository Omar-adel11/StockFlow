using System.Collections.Generic;
using Domain.common;

namespace Domain.Entities
{
    public class Category : ISoftDelete
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public ICollection<Product> Products { get; set; } = new List<Product>();
        public int? BusinessId { get; set; }
        public Business? Business { get; set; }

        public bool IsActive { get; set; }
    }
}
