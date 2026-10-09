using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Interfaces.AuthInterfaces;

namespace Application.Caching
{
    public class RedisCacheService(ICacheRepository cacheRepository) : ICacheService
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<string?> GetAsync(string Key)
        {
            return await cacheRepository.GetAsync(Key);
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

            if (!string.IsNullOrWhiteSpace(cachedJson))
            {
                try
                {
                    var deserialized = JsonSerializer.Deserialize<T>(cachedJson);
                    if (deserialized is not null)
                    {
                        return deserialized;
                    }
                }
                catch
                {
                    // Fall back to factory if JSON payload is corrupted
                }
            }

            var value = await factory();

            if (value is not null)
            {
                await cacheRepository.SetAsync(key, value, duration);
            }

            return value;
        }

        public async Task<T> GetOrCreateWithLockAsync<T>(string key, Func<Task<T>> factory, TimeSpan? duration = null)
        {
            // 1. Initial Cache Check
            var cachedJson = await cacheRepository.GetAsync(key);
            if (!string.IsNullOrWhiteSpace(cachedJson))
            {
                try
                {
                    var deserialized = JsonSerializer.Deserialize<T>(cachedJson);
                    if (deserialized is not null)
                    {
                        return deserialized;
                    }
                }
                catch
                {
                    // Fall back to factory if JSON payload is corrupted
                }
            }

            // 2. Lock for stampede protection
            await _semaphore.WaitAsync();
            try
            {
                // Double-check cache after acquiring lock
                cachedJson = await cacheRepository.GetAsync(key);
                if (!string.IsNullOrWhiteSpace(cachedJson))
                {
                    try
                    {
                        var deserialized = JsonSerializer.Deserialize<T>(cachedJson);
                        if (deserialized is not null)
                        {
                            return deserialized;
                        }
                    }
                    catch
                    {
                        // Fall back to factory if JSON payload is corrupted
                    }
                }

                // 3. Cache Miss / Redis Down -> Fetch directly from Database
                var value = await factory();
                if (value is not null)
                {
                    await cacheRepository.SetAsync(key, value, duration);
                }

                return value;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}