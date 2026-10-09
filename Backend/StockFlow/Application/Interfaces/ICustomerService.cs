using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using static Application.DTOs.CustomerDtos;

namespace Application.Interfaces
{
    public interface ICustomerService
    {
        Task<IReadOnlyCollection<CustomerResponse>> GetAllCustomersAsync(string? search, int businessId);
        Task<CustomerResponse?> GetCustomerAsync(int id);
        Task<CustomerResponse> CreateCustomerAsync(CustomerCreateRequest createRequest, int businessId);
        Task<CustomerResponse?> UpdateCustomerAsync(int id, CustomerUpdateRequest updateRequest,int businessId);
        Task<bool> DeleteCustomerAsync(int id, int businessId);
        Task<CustomerResponse> AddAddressAsync(int customerId, AddressSaveRequest request, int businessId);
        Task<bool> UpdateAddressAsync(int customerId, int addressId, AddressSaveRequest request, int businessId);
        Task<bool> DeleteAddressAsync(int customerId, int addressId, int businessId);
    }
}

