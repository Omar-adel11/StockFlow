using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.userDtos
{
    public class UserDTO
    {
        [Required]
        public string name { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;
        public string? ImgUrl { get; set; }
        public string? Token { get; set; } 
        public string? refreshToken { get; set; } 
        

    }
}