using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using static Application.DTOs.SalesOrderDtos;

namespace Application.Interfaces
{
    public interface ISalesOrderService
    {
        Task<IReadOnlyCollection<SalesResponse>> GetAllOrdersAsync();
        Task<SalesResponse?> GetOrderByIdAsync(int id);
        Task<SalesResponse> CreateSalesOrderAsync(SalesCreateRequest createRequest, int currentUserId);
        Task<bool> FulfillSalesOrderAsync(int id);
        Task<bool> CancelSalesOrderAsync(int id);
    }
}
