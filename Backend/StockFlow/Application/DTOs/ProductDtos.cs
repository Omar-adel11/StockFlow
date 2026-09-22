using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public static class ProductDtos
    {
        public record ProductCreateRequest(
            [Required][MaxLength(100)] string ItemSKU,
            [Required][MaxLength(200)] string Name,
            [Range(0, double.MaxValue)] decimal UnitSellingPrice,
            [Range(0, int.MaxValue)] int ReorderLevel,
            [Required] int CategoryId,
            int? PreferredSupplierId
        );

        public record ProductUpdateRequest(
            [Required][MaxLength(200)] string Name,
            [Range(0, double.MaxValue)] decimal UnitSellingPrice,
            [Range(0, int.MaxValue)] int ReorderLevel,
            [Required] int CategoryId,
            int? PreferredSupplierId,
            bool IsActive
        );

        public record ProductResponse(
            int Id,
            string ItemSKU,
            string Name,
            decimal UnitSellingPrice,
            int ReorderLevel,
            int CategoryId,
            string CategoryName,
            int? PreferredSupplierId,
            string? PreferredSupplierName,
            bool IsActive
        );
    }
}
