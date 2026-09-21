using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.DTOs.CategoryDtos;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IReadOnlyCollection<Response>> GetAllCategoriesAsync();
        Task<Response?> GetCategoryAsync(int id);
        Task<Response> CreateCategoryAsync(CreateRequest createRequest);
        Task<Response?> UpdateCategoryAsync(int Id, UpdateRequest updateRequest);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
