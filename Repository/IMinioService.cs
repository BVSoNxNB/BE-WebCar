using Minio.DataModel;

namespace WebCar.Repository
{
    public interface IMinioService
    {
        Task StoreObjectAsync(string bucketName, string objectName, byte[] data);
        Task<byte[]> GetObjectAsync(string bucketName, string objectName);
        Task<List<Item>> ListObjectsAsync(string bucketName);
        Task RemoveObjectAsync(string bucketName, string objectName);
    }
}
