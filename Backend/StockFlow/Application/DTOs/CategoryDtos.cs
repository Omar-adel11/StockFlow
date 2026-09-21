using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public static class CategoryDtos
    {
        public record CreateRequest(
            [property: Required, MaxLength(100)] string Name,
            [property: MaxLength(500)] string? Description
        );

        public record UpdateRequest(
            [property: Required, MaxLength(100)] string Name,
            [property: MaxLength(500)] string? Description
        );

        public record Response(int Id, string Name, string? Description);
    }

}
