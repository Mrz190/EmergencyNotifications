using API.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace API.Services
{
    public class RedisService : IRedisService
    {
        private readonly IDistributedCache _cache;
        public RedisService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetTokenAsync(string key, string token, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromHours(1)
            };

            await _cache.SetStringAsync(key, token, options);
        }

        public async Task<string> GetTokenAsync(string key)
        {
            return await _cache.GetStringAsync(key);
        }

        public async Task RemoveTokenAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
