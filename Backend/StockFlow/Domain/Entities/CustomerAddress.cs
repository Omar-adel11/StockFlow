using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CustomerAddress
    {
        public int Id { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? State { get; set; } = string.Empty;
        public string? ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = true;

        // Foreign Key
        public int CustomerId { get; set; }

        // Navigation property back to parent
        public Customer Customer { get; set; } = null!;
        public int? BusinessId { get; set; }
        public Business? Business { get; set; }
    }
}
