using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using static Application.DTOs.ProductDtos;

namespace Application.Services
{
    public class CachedProductService(
        IProductService innerService,
        ICacheService cacheService) : IProductService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        private static string GetProductKey(int id) => $"products:id:{id}";
        private static string GetProductSkuKey(string sku) => $"products:sku:{sku}";
        private static string GetProductsListKey(int businessId) =>
            $"products:biz:{businessId}:list";

        public async Task<ProductResponse> CreateProductAsync(ProductCreateRequest createRequest, int businessId)
        {
            var response = await innerService.CreateProductAsync(createRequest, businessId);

            await InvalidateBusinessListCacheAsync(businessId);
            return response;
        }

        public async Task<ProductResponse?> GetProductByIdAsync(int id)
        {
            return await cacheService.GetOrCreateWithLockAsync(
                GetProductKey(id),
                () => innerService.GetProductByIdAsync(id),
                CacheDuration
            );
        }

        public async Task<ProductResponse?> GetProductBySkuAsync(string sku)
        {
            return await cacheService.GetOrCreateWithLockAsync(
                GetProductSkuKey(sku),
                () => innerService.GetProductBySkuAsync(sku),
                CacheDuration
            );
        }

        public async Task<IReadOnlyCollection<ProductResponse>> GetAllProductsAsync(string? search, int businessId)
        {
            if(!string.IsNullOrWhiteSpace(search))
            {
                return await innerService.GetAllProductsAsync(search,businessId);
            }
            var cacheKey = GetProductsListKey(businessId);
            return await cacheService.GetOrCreateWithLockAsync(
               cacheKey,
               () => innerService.GetAllProductsAsync(search, businessId),
               CacheDuration
           );
        }

        public async Task<ProductResponse?> UpdateProductAsync(int id, ProductUpdateRequest updateRequest, int businessId)
        {
            var updated = await innerService.UpdateProductAsync(id, updateRequest,businessId);

            if (updated != null)
            {
                await cacheService.RemoveAsync(GetProductKey(id));
                await cacheService.RemoveAsync(GetProductSkuKey(updated.ItemSKU));
                await InvalidateBusinessListCacheAsync(businessId);
            }

            return updated;
        }

        public async Task<bool> DeleteProductAsync(int id, int businessId)
        {
            // Fetch first to know the SKU for cache invalidation
            var existing = await innerService.GetProductByIdAsync(id);

            var isDeleted = await innerService.DeleteProductAsync(id, businessId);

            if (isDeleted && existing != null)
            {
                await cacheService.RemoveAsync(GetProductKey(id));
                await cacheService.RemoveAsync(GetProductSkuKey(existing.ItemSKU));
                await InvalidateBusinessListCacheAsync(businessId);
            }

            return isDeleted;
        }

        private async Task InvalidateBusinessListCacheAsync(int businessId)
        {
            await cacheService.RemoveAsync(GetProductsListKey(businessId));
        }
    }
}