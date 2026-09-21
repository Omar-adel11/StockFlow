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
        [property: Required, MaxLength(150)] string WarehouseName,
        [property: Required, MaxLength(300)] string LocationAddress
    );

        public record WarehouseUpdateRequest(
            [property: Required, MaxLength(150)] string WarehouseName,
            [property: Required, MaxLength(300)] string LocationAddress,
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
