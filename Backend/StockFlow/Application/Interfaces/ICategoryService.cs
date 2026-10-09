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
        Task<IReadOnlyCollection<Response>> GetAllCategoriesAsync(string? search,int businessId);
        Task<Response?> GetCategoryAsync(int id);
        Task<Response> CreateCategoryAsync(CreateRequest createRequest, int BusinessId);
        Task<Response?> UpdateCategoryAsync(int Id, UpdateRequest updateRequest, int businessId);
        Task<bool> DeleteCategoryAsync(int id, int businessId);
    }
}
