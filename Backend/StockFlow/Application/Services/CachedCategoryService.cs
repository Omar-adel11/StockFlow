using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;

namespace Application.Services
{
    public class CachedCategoryService(
        ICategoryService innerService,
        ICacheService cacheService) : ICategoryService
    {
        // Category data is relatively static -> High TTL (e.g., 2 hours)
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(2);

        // Helper key generators incorporating BusinessId scoping
        private static string GetCategoryKey(int id) => $"categories:id:{id}";
        private static string GetCategoriesListKey(int businessId) =>
            $"categories:biz:{businessId}:list";

        public async Task<CategoryDtos.Response> CreateCategoryAsync(CategoryDtos.CreateRequest createRequest, int businessId)
        {
            var response = await innerService.CreateCategoryAsync(createRequest, businessId);

            // Invalidate the business list cache since a new item was added
            await InvalidateBusinessListCacheAsync(businessId);

            return response;
        }

        public async Task<CategoryDtos.Response?> GetCategoryAsync(int id)
        {
            string cacheKey = GetCategoryKey(id);

            return await cacheService.GetOrCreateWithLockAsync(
                cacheKey,
                () => innerService.GetCategoryAsync(id),
                CacheDuration
            );
        }

        public async Task<IReadOnlyCollection<CategoryDtos.Response>> GetAllCategoriesAsync(string? search,int businessId)
        {
            if(!string.IsNullOrWhiteSpace(search))
            {
                // If search is provided, bypass caching and fetch directly
                return await innerService.GetAllCategoriesAsync(search,businessId);
            }
            var cacheKey = GetCategoriesListKey(businessId);

            return await cacheService.GetOrCreateWithLockAsync(
               cacheKey,
               () => innerService.GetAllCategoriesAsync(search,businessId),
               CacheDuration
           );
        }

        public async Task<CategoryDtos.Response?> UpdateCategoryAsync(int id, CategoryDtos.UpdateRequest updateRequest,int businessId)
        {
            var updatedCategory = await innerService.UpdateCategoryAsync(id, updateRequest,businessId);

            if (updatedCategory != null)
            {
                // Invalidate single category key
                await cacheService.RemoveAsync(GetCategoryKey(id));
                await InvalidateBusinessListCacheAsync(businessId);
            }

            return updatedCategory;
        }

        public async Task<bool> DeleteCategoryAsync(int id,int businessId)
        {
            var isDeleted = await innerService.DeleteCategoryAsync(id,businessId);

            if (isDeleted)
            {
                // Invalidate single category key
                await cacheService.RemoveAsync(GetCategoryKey(id));
                await InvalidateBusinessListCacheAsync(businessId); // Assuming businessId can be derived or passed in some way
            }

            return isDeleted;
        }

        private async Task InvalidateBusinessListCacheAsync(int businessId)
        {
            // Invalidate default list key
            await cacheService.RemoveAsync(GetCategoriesListKey(businessId));
        }
    }
}