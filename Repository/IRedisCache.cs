namespace WebCar.Repository
{
    public interface IRedisCache
    {
        Task<byte[]?> Get(string key);
        Task<bool> Add(string key, string value);
        Task Delete(string key);
    }
}
