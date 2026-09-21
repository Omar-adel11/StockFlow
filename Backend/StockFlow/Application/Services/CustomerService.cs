using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions.NotFound;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CustomerService(IAppDbContext _context) : ICustomerService
    {
        private DbSet<Customer> CustomerSet => _context.Customers;
        private DbSet<CustomerAddress> AddressSet => _context.CustomerAddresses;

        public async Task<CustomerDtos.CustomerResponse> CreateCustomerAsync(CustomerDtos.CustomerCreateRequest createRequest)
        {
            var customer = new Customer
            {
                Name = createRequest.Name,
                Email = createRequest.Email,
                Phone = createRequest.Phone,
                Addresses = createRequest.Addresses.Select(a => new CustomerAddress
                {
                    Street = a.Street,
                    City = a.City,
                    State = a.State,
                    ZipCode = a.ZipCode,
                    Country = a.Country,
                    IsDefault = a.IsDefault
                }).ToList()
            };

            await CustomerSet.AddAsync(customer);
            await _context.SaveChangesAsync();

            return MapToResponse(customer);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await CustomerSet.FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null)
            {
                throw new CustomerNotFound();
            }

            CustomerSet.Remove(customer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IReadOnlyCollection<CustomerDtos.CustomerResponse>> GetAllCustomersAsync()
        {
            var customers = await CustomerSet
                .AsNoTracking()
                .Include(c => c.Addresses)
                .ToListAsync();

            return customers.Select(MapToResponse).ToList();
        }

        public async Task<CustomerDtos.CustomerResponse?> GetCustomerAsync(int id)
        {
            var customer = await GetCustomerEntityAsync(id);
            return MapToResponse(customer);
        }

        public async Task<CustomerDtos.CustomerResponse?> UpdateCustomerAsync(int id, CustomerDtos.CustomerUpdateRequest updateRequest)
        {
            var customer = await GetCustomerEntityAsync(id);

            customer.Name = updateRequest.Name;
            customer.Email = updateRequest.Email;
            customer.Phone = updateRequest.Phone;

            await _context.SaveChangesAsync();

            return MapToResponse(customer);
        }

        #region Address Operations

        public async Task<CustomerDtos.CustomerResponse> AddAddressAsync(int customerId, CustomerDtos.AddressSaveRequest request)
        {
            var customer = await GetCustomerEntityAsync(customerId);

            if (request.IsDefault)
            {
                UnsetDefaultAddresses(customer.Addresses);
            }

            customer.Addresses.Add(new CustomerAddress
            {
                Street = request.Street,
                City = request.City,
                State = request.State,
                ZipCode = request.ZipCode,
                Country = request.Country,
                IsDefault = request.IsDefault
            });

            await _context.SaveChangesAsync();
            return MapToResponse(customer);
        }

        public async Task<bool> UpdateAddressAsync(int customerId, int addressId, CustomerDtos.AddressSaveRequest request)
        {
            var customer = await GetCustomerEntityAsync(customerId);
            var address = customer.Addresses.FirstOrDefault(a => a.Id == addressId);

            if (address == null)
            {
                throw new AddressNotFoundException();
            }

            if (request.IsDefault && !address.IsDefault)
            {
                UnsetDefaultAddresses(customer.Addresses);
            }

            address.Street = request.Street;
            address.City = request.City;
            address.State = request.State;
            address.ZipCode = request.ZipCode;
            address.Country = request.Country;
            address.IsDefault = request.IsDefault;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAddressAsync(int customerId, int addressId)
        {
            var address = await AddressSet.FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId);
            if (address == null)
            {
                throw new AddressNotFoundException();
            }

            AddressSet.Remove(address);
            return await _context.SaveChangesAsync() > 0;
        }

        #endregion

        #region Helper Methods

        private async Task<Customer> GetCustomerEntityAsync(int id)
        {
            var customer = await CustomerSet
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                throw new CustomerNotFound();
            }

            return customer;
        }

        private static void UnsetDefaultAddresses(IEnumerable<CustomerAddress> addresses)
        {
            foreach (var addr in addresses)
            {
                addr.IsDefault = false;
            }
        }

        private static CustomerDtos.CustomerResponse MapToResponse(Customer customer)
        {
            return new CustomerDtos.CustomerResponse(
                customer.Id,
                customer.Name,
                customer.Email,
                customer.Phone,
                customer.Addresses.Select(a => new CustomerDtos.AddressResponse(
                    a.Id,
                    a.Street,
                    a.City,
                    a.State,
                    a.ZipCode,
                    a.Country,
                    a.IsDefault
                )).ToList()
            );
        }

        #endregion
    }
}