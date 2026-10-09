using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using static Application.DTOs.WarehouseDtos;

namespace Application.Interfaces
{
    public interface IWarehouseService
    {
        Task<IReadOnlyCollection<WarehouseResponse>> GetAllWarehousesAsync(string? search,int businessId);
        Task<WarehouseResponse?> GetWarehouseAsync(int id);
        Task<WarehouseResponse> CreateWarehouseAsync(WarehouseCreateRequest createRequest, int businessId);
        Task<WarehouseResponse?> UpdateWarehouseAsync(int id, WarehouseUpdateRequest updateRequest,int businessId);
        Task<bool> DeleteWarehouseAsync(int id, int businessId);
    }
}
