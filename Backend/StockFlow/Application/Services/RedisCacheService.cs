using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Interfaces.AuthInterfaces;


namespace Application.Caching
{
    public class RedisCacheService(ICacheRepository cacheRepository) : ICacheService
    {

        public async Task<string?> GetAsync(string Key)
        {
            var value = await cacheRepository.GetAsync(Key);
            return value == null ? null : value;
        }



        public async Task SetCacheValueAsync(string Key, object Value, TimeSpan? duration)
        {
            await cacheRepository.SetAsync(Key, Value, duration);
        }

        public async Task RemoveAsync(string key)
        {
            await cacheRepository.RemoveAsync(key);
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? duration = null)
        {
            var cachedJson = await cacheRepository.GetAsync(key);

            if (cachedJson is not null)
            {
                return JsonSerializer.Deserialize<T>(cachedJson)!;
            }

            var value = await factory();

            // CacheRepository.SetAsync already handles JSON serialization internally,
            // so we just hand it the raw object.
            await cacheRepository.SetAsync(key, value!, duration);

            return value;
        }

   
    }
}
