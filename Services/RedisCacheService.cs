using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using WebCar.Repository;

namespace WebCar.Services
{
    public class RedisCacheService : IRedisCache
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task<bool> Add(string key, string value)
        {
            // Setting up the cache options
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5)) // setup time het han 5p cho cache
                .SetSlidingExpiration(TimeSpan.FromMinutes(1));// setup time het han 1p ke tu khi gan nhat truy van
            var dataToCache = Encoding.UTF8.GetBytes(value); // chuyen data -> byte
            await _cache.SetAsync(key, dataToCache, options);// them cache
            return true;
        }

        public async Task<byte[]?> Get(string key)
        {
            return await _cache.GetAsync(key); // tra du lieu kieu byte thong qua key 
        }
        public async Task Delete(string key)
        {
            await _cache.RemoveAsync(key); //Xoa Cache
        }
    }
}
