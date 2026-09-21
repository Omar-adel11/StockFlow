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
        Task<IReadOnlyCollection<SupplierResponse>> GetAllSuppliersAsync();
        Task<SupplierResponse?> GetSupplierAsync(int id);
        Task<SupplierResponse> CreateSupplierAsync(SupplierCreateRequest createRequest);
        Task<SupplierResponse?> UpdateSupplierAsync(int id, SupplierUpdateRequest updateRequest);
        Task<bool> DeleteSupplierAsync(int id);
    }
}
