using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class WarehouseDtos
    {
        public record WarehouseCreateRequest(
        [Required][MaxLength(150)] string WarehouseName,
        [Required][MaxLength(300)] string LocationAddress
    );

        public record WarehouseUpdateRequest(
            [Required][MaxLength(150)] string WarehouseName,
            [Required][MaxLength(300)] string LocationAddress,
            bool IsActive
        );

        public record WarehouseResponse(
            int Id,
            string WarehouseName,
            string LocationAddress,
            bool IsActive
        );
    }
}
