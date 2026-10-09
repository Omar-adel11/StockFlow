using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using static Application.DTOs.SuppliersDtos;

namespace Application.Interfaces
{
    public interface ISupplierService
    {
        Task<IReadOnlyCollection<SupplierResponse>> GetAllSuppliersAsync(string? search,int businessId);
        Task<SupplierResponse?> GetSupplierAsync(int id);
        Task<SupplierResponse> CreateSupplierAsync(SupplierCreateRequest createRequest,int businessId);
        Task<SupplierResponse?> UpdateSupplierAsync(int id, SupplierUpdateRequest updateRequest, int businessId);
        Task<bool> DeleteSupplierAsync(int id, int businessId);
    }
}
