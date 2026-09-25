using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.DTOs.ProductDtos;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<IReadOnlyCollection<ProductResponse>> GetAllProductsAsync();
        Task<ProductResponse?> GetProductByIdAsync(int id);
        Task<ProductResponse?> GetProductBySkuAsync(string sku);
        Task<ProductResponse> CreateProductAsync(ProductCreateRequest createRequest,int businessId);
        Task<ProductResponse?> UpdateProductAsync(int id, ProductUpdateRequest updateRequest);
        Task<bool> DeleteProductAsync(int id);
    }
}
