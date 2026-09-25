using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.AuthDTOs
{
    public class SignupDTO
    {

        [Required]
        [MaxLength(200)]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        public string name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(password))]
        public string confirmPassword { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber {  get; set; }
        public IFormFile? file { get; set; }

    }
}
