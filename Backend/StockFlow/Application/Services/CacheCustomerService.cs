using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;

namespace Application.Services
{
    public class CachedCustomerService(
        ICustomerService innerService,
        ICacheService cacheService) : ICustomerService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        private static string GetCustomerKey(int id) => $"customers:id:{id}";
        private static string GetCustomersListKey(int businessId) => $"customers:biz:{businessId}:list";

        public async Task<CustomerDtos.CustomerResponse> CreateCustomerAsync(
            CustomerDtos.CustomerCreateRequest createRequest, int businessId)
        {
            var response = await innerService.CreateCustomerAsync(createRequest, businessId);
            await InvalidateBusinessListCacheAsync(businessId);
            return response;
        }

        public async Task<CustomerDtos.CustomerResponse?> GetCustomerAsync(int id)
        {
            return await cacheService.GetOrCreateWithLockAsync(
                GetCustomerKey(id),
                () => innerService.GetCustomerAsync(id),
                CacheDuration
            );
        }

        public async Task<IReadOnlyCollection<CustomerDtos.CustomerResponse>> GetAllCustomersAsync(
            string? search, int businessId)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await innerService.GetAllCustomersAsync(search, businessId);

            var cacheKey = GetCustomersListKey(businessId);
            return await cacheService.GetOrCreateWithLockAsync(
                cacheKey,
                () => innerService.GetAllCustomersAsync(search, businessId),
                CacheDuration
            );
        }

        public async Task<CustomerDtos.CustomerResponse?> UpdateCustomerAsync(
            int id, CustomerDtos.CustomerUpdateRequest updateRequest, int businessId)
        {
            var updated = await innerService.UpdateCustomerAsync(id, updateRequest, businessId);

            if (updated != null)
            {
                await cacheService.RemoveAsync(GetCustomerKey(id));
                await InvalidateBusinessListCacheAsync(businessId);
            }

            return updated;
        }

        public async Task<bool> DeleteCustomerAsync(int id, int businessId)
        {
            var isDeleted = await innerService.DeleteCustomerAsync(id, businessId);

            if (isDeleted)
            {
                await cacheService.RemoveAsync(GetCustomerKey(id));
                await InvalidateBusinessListCacheAsync(businessId);
            }

            return isDeleted;
        }

        #region Address Operations

        public async Task<CustomerDtos.CustomerResponse> AddAddressAsync(
            int customerId, CustomerDtos.AddressSaveRequest request, int businessId)
        {
            var response = await innerService.AddAddressAsync(customerId, request, businessId);

            // Evict both the individual customer and the business list cache
            await cacheService.RemoveAsync(GetCustomerKey(customerId));
            await InvalidateBusinessListCacheAsync(businessId);

            return response;
        }

        public async Task<bool> UpdateAddressAsync(
            int customerId, int addressId, CustomerDtos.AddressSaveRequest request, int businessId)
        {
            var result = await innerService.UpdateAddressAsync(customerId, addressId, request, businessId);

            if (result)
            {
                // Evict both individual customer and business list cache
                await cacheService.RemoveAsync(GetCustomerKey(customerId));
                await InvalidateBusinessListCacheAsync(businessId);
            }

            return result;
        }

        public async Task<bool> DeleteAddressAsync(int customerId, int addressId, int businessId)
        {
            var result = await innerService.DeleteAddressAsync(customerId, addressId, businessId);

            if (result)
            {
                // Evict both individual customer and business list cache
                await cacheService.RemoveAsync(GetCustomerKey(customerId));
                await InvalidateBusinessListCacheAsync(businessId);
            }

            return result;
        }

        #endregion

        private async Task InvalidateBusinessListCacheAsync(int businessId)
        {
            await cacheService.RemoveAsync(GetCustomersListKey(businessId));
        }
    }
}