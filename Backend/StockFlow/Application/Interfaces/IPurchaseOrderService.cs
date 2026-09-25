using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using static Application.DTOs.PurchaseOrderDtos;

namespace Application.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<IReadOnlyCollection<PurchaseResponse>> GetAllOrdersAsync(int count);
        Task<PurchaseResponse?> GetOrderByIdAsync(int id);
        Task<PurchaseResponse> CreatePurchaseOrderAsync(PurchaseCreateRequest createRequest, int currentUserId, int businessId);
        Task<bool> ReceivePurchaseOrderAsync(int id, int businessId);
        Task<bool> CancelPurchaseOrderAsync(int id);
    }
}
