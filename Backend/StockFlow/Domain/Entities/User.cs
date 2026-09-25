using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using static System.Collections.Specialized.BitVector32;

namespace Domain.Entities
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; } = string.Empty;
        public string? ImgUrl { get; set; }

        //nav properties
        public int? BusinessId { get; set; }
        public Business? Business { get; set; }

        public bool IsActive { get; set; } = true;
      
    }
}
