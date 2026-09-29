using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.userDtos
{
    public class UpdateUserProfileDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public IFormFile? file { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
