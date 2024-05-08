using Minio.DataModel.Args;
using Minio;
using System.Reactive.Linq;
using WebCar.Models;
using System.Security.AccessControl;
using Microsoft.AspNetCore.StaticFiles;

public class MinIOService
{
    private readonly IMinioClient _minio;
    private readonly IConfiguration _configuration;
    private readonly string _bucketName;
    private readonly string _host;

    public MinIOService(IMinioClient minioClient, IConfiguration configuration)
    {
        _minio = minioClient;
        _configuration = configuration;
        _bucketName = _configuration.GetValue<string>("MinIO:BucketName");
        _host = _configuration.GetValue<string>("MinIO:Host");
    }

    public async Task<string> UploadImageAsync(IFormFile imageFile)
    {
        var bucketName = _bucketName;
        // Check Exists bucket
        bool found = await _minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!found)
        {
            // if bucket not Exists, make bucket
            await _minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
        }

        // Get the file extension
        var fileExtension = Path.GetExtension(imageFile.FileName);

        // Set the content type based on the file extension
        var contentType = GetContentTypeFromFileExtension(fileExtension);

        var fileName = Guid.NewGuid().ToString() + fileExtension;

        // Upload object
        await _minio.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(fileName)
            .WithStreamData(imageFile.OpenReadStream())
            .WithObjectSize(imageFile.Length)
            .WithContentType(contentType)
        );

        return $"{_bucketName}/{fileName}";
    }

    public async Task<IFormFile> DownloadImageAsync(string bucketName, string fileName)
    {
        MemoryStream destination = new MemoryStream();

        // Check if object exists
        var objStatReply = await _minio.StatObjectAsync(new StatObjectArgs()
            .WithBucket(bucketName)
            .WithObject(fileName)
        );

        if (objStatReply == null || objStatReply.DeleteMarker)
            throw new Exception("Object not found or deleted");

        // Get object
        await _minio.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(fileName)
            .WithCallbackStream((stream) =>
            {
                stream.CopyTo(destination);
            })
        );

        // Create a FormFile from the MemoryStream
        var formFile = new FormFile(destination, 0, destination.Length, null, Path.GetFileName(fileName))
        {
            Headers = new HeaderDictionary(),
            ContentType = GetContentTypeFromFileExtension(Path.GetExtension(fileName))
        };

        return formFile;
    }

    private string GetContentTypeFromFileExtension(string fileExtension)
    {
        // Dictionary để map file extension với content type
        var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

        // Nếu không tìm thấy content type tương ứng, trả về "application/octet-stream"
        if (!fileExtensionContentTypeProvider.TryGetContentType(fileExtension, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return contentType;
    }
    public async Task<bool> DeleteImageAsync(string bucketName, string objectName)
    {
        var removeObjectArgs = new RemoveObjectArgs()
        .WithBucket(bucketName)
        .WithObject(objectName);

        await _minio.RemoveObjectAsync(removeObjectArgs);
        return true;
    }
}