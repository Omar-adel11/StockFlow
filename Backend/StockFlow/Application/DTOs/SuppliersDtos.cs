using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class SuppliersDtos
    {
        public record SupplierCreateRequest(
        [Required][MaxLength(150)] string Name,
        [Required][EmailAddress][MaxLength(150)] string ContactEmail,
        [Required][Phone][MaxLength(30)] string ContactPhone,
        [Required][MaxLength(150)] string Address
    );

        public record SupplierUpdateRequest(
            [Required][MaxLength(150)] string Name,
            [Required][EmailAddress][MaxLength(150)] string ContactEmail,
            [Required][Phone][MaxLength(30)] string ContactPhone,
            [Required][MaxLength(150)] string Address,
            bool IsActive
        );

        public record SupplierResponse(
            int Id,
            string Name,
            string ContactEmail,
            string ContactPhone,
            string Address,
            bool IsActive
        );
    }
}
