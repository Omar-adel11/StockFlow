using System;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces.AuthInterfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Persistence.Repository
{
    public class CacheRepository(
        IConnectionMultiplexer connection,
        ILogger<CacheRepository> logger) : ICacheRepository
    {
        private IDatabase? GetDatabase()
        {
            try
            {
                if (connection.IsConnected)
                {
                    return connection.GetDatabase();
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis connection is not active.");
            }
            return null;
        }

        public async Task<string?> GetAsync(string Key)
        {
            try
            {
                var db = GetDatabase();
                if (db == null) return null;

                var value = await db.StringGetAsync(Key);
                return value.HasValue ? value.ToString() : null;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis GetAsync failed for key '{Key}'. Falling back to DB.", Key);
                return null;
            }
        }

        public async Task SetAsync(string Key, object Value, TimeSpan? duration)
        {
            try
            {
                var db = GetDatabase();
                if (db == null) return;

                var RedisValue = JsonSerializer.Serialize(Value);
                await db.StringSetAsync(Key, RedisValue, duration);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis SetAsync failed for key '{Key}'. Proceeding without caching.", Key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                var db = GetDatabase();
                if (db == null) return;

                await db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis RemoveAsync failed for key '{Key}'.", key);
            }
        }

        public Task RemoveAsyncByValue(string value)
        {
            throw new NotImplementedException();
        }
    }
}