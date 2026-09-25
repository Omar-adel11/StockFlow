using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces.AuthInterfaces;
using StackExchange.Redis;

namespace Persistence.Repository
{
    public class CacheRepository(IConnectionMultiplexer connection) : ICacheRepository
    {
        private readonly IDatabase _database = connection.GetDatabase();
        public async Task<string?> GetAsync(string Key)
        {
            
            var value = await _database.StringGetAsync(Key);
            return value.HasValue ? value.ToString() : null;
        }
        public async Task SetAsync(string Key, object Value, TimeSpan? duration)
        {
            var RedisValue = JsonSerializer.Serialize(Value);
            await _database.StringSetAsync(Key, RedisValue, duration);
        }
        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public Task RemoveAsyncByValue(string value)
        {
            throw new NotImplementedException();
        }

       
    }
}
