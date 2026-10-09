using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;

namespace Application.Services
{
    public class CachedSupplierService(
        ISupplierService innerService,
        ICacheService cacheService) : ISupplierService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        private static string GetSupplierKey(int id) => $"suppliers:id:{id}";
        private static string GetSuppliersListKey(int businessId) =>
            $"suppliers:biz:{businessId}:list";

        public async Task<SuppliersDtos.SupplierResponse> CreateSupplierAsync(
            SuppliersDtos.SupplierCreateRequest createRequest, int businessId)
        {
            var response = await innerService.CreateSupplierAsync(createRequest, businessId);

            await InvalidateBusinessListCacheAsync(businessId);
            return response;
        }

        public async Task<SuppliersDtos.SupplierResponse?> GetSupplierAsync(int id)
        {
            return await cacheService.GetOrCreateWithLockAsync(
                GetSupplierKey(id),
                () => innerService.GetSupplierAsync(id),
                CacheDuration
            );
        }

        public async Task<IReadOnlyCollection<SuppliersDtos.SupplierResponse>> GetAllSuppliersAsync(
            string? search, int businessId)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                return await innerService.GetAllSuppliersAsync(search, businessId);
            }

            var cacheKey = GetSuppliersListKey(businessId);
            return await cacheService.GetOrCreateWithLockAsync(
                cacheKey,
                () => innerService.GetAllSuppliersAsync(search, businessId),
                CacheDuration
            );
        }

        public async Task<SuppliersDtos.SupplierResponse?> UpdateSupplierAsync(
            int id, SuppliersDtos.SupplierUpdateRequest updateRequest, int businessId)
        {
            var updated = await innerService.UpdateSupplierAsync(id, updateRequest, businessId);

            if (updated != null)
            {
                await cacheService.RemoveAsync(GetSupplierKey(id));
                await InvalidateBusinessListCacheAsync(businessId);
            }

            return updated;
        }

        public async Task<bool> DeleteSupplierAsync(int id, int businessId)
        {
            var isDeleted = await innerService.DeleteSupplierAsync(id, businessId);

            if (isDeleted)
            {
                await cacheService.RemoveAsync(GetSupplierKey(id));
                await InvalidateBusinessListCacheAsync(businessId);
            }

            return isDeleted;
        }

        private async Task InvalidateBusinessListCacheAsync(int businessId)
        {
            await cacheService.RemoveAsync(GetSuppliersListKey(businessId));
        }
    }
}
