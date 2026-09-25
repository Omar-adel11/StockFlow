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
        Task<IReadOnlyCollection<WarehouseResponse>> GetAllWarehousesAsync();
        Task<WarehouseResponse?> GetWarehouseAsync(int id);
        Task<WarehouseResponse> CreateWarehouseAsync(WarehouseCreateRequest createRequest, int businessId);
        Task<WarehouseResponse?> UpdateWarehouseAsync(int id, WarehouseUpdateRequest updateRequest);
        Task<bool> DeleteWarehouseAsync(int id);
    }
}
