using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;

namespace Application.Services
{
    public class CachedWarehouseService(
        IWarehouseService innerService,
        ICacheService cacheService) : IWarehouseService
    {
        // Warehouses rarely change -> long TTL
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(4);

        private static string GetWarehouseKey(int id) => $"warehouses:id:{id}";
        private static string GetWarehousesListKey(int businessId) =>
            $"warehouses:biz:{businessId}:list";

        public async Task<WarehouseDtos.WarehouseResponse> CreateWarehouseAsync(
            WarehouseDtos.WarehouseCreateRequest createRequest, int businessId)
        {
            var response = await innerService.CreateWarehouseAsync(createRequest, businessId);

            await InvalidateBusinessListCacheAsync(businessId);
            return response;
        }

        public async Task<WarehouseDtos.WarehouseResponse?> GetWarehouseAsync(int id)
        {
            return await cacheService.GetOrCreateWithLockAsync(
                GetWarehouseKey(id),
                () => innerService.GetWarehouseAsync(id),
                CacheDuration
            );
        }

        public async Task<IReadOnlyCollection<WarehouseDtos.WarehouseResponse>> GetAllWarehousesAsync(
            string? search, int businessId)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                return await innerService.GetAllWarehousesAsync(search, businessId);
            }

            var cacheKey = GetWarehousesListKey(businessId);
            return await cacheService.GetOrCreateWithLockAsync(
                cacheKey,
                () => innerService.GetAllWarehousesAsync(search, businessId),
                CacheDuration
            );
        }

        public async Task<WarehouseDtos.WarehouseResponse?> UpdateWarehouseAsync(
            int id, WarehouseDtos.WarehouseUpdateRequest updateRequest, int businessId)
        {
            var updated = await innerService.UpdateWarehouseAsync(id, updateRequest, businessId);

            if (updated != null)
            {
                await cacheService.RemoveAsync(GetWarehouseKey(id));
                await InvalidateBusinessListCacheAsync(businessId);
            }

            return updated;
        }

        public async Task<bool> DeleteWarehouseAsync(int id, int businessId)
        {
            var isDeleted = await innerService.DeleteWarehouseAsync(id, businessId);

            if (isDeleted)
            {
                await cacheService.RemoveAsync(GetWarehouseKey(id));
                await InvalidateBusinessListCacheAsync(businessId);
            }

            return isDeleted;
        }

        private async Task InvalidateBusinessListCacheAsync(int businessId)
        {
            await cacheService.RemoveAsync(GetWarehousesListKey(businessId));
        }
    }
}