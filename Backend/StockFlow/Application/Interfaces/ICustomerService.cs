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
        Task<IReadOnlyCollection<CustomerResponse>> GetAllCustomersAsync();
        Task<CustomerResponse?> GetCustomerAsync(int id);
        Task<CustomerResponse> CreateCustomerAsync(CustomerCreateRequest createRequest);
        Task<CustomerResponse?> UpdateCustomerAsync(int id, CustomerUpdateRequest updateRequest);
        Task<bool> DeleteCustomerAsync(int id);
    }
}
