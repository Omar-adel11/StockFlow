using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.AuthDTOs
{
    public class CheckOtpDTO
    {
        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;
       
       
        [Required]
        public string otp { get; set; } = string.Empty;
    }
}
