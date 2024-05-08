//using Microsoft.AspNetCore.Mvc;
//using Minio;
//using Minio.DataModel.Args;

//[ApiController]
//[Route("api/[controller]")]
//public class MinioController : ControllerBase
//{
//    private readonly MinioClient _minioClient;

//    public MinioController(MinioClient minioClient)
//    {
//        _minioClient = minioClient;
//    }

//    [HttpPost("upload")]
//    public async Task<IActionResult> Upload(IFormFile file)
//    {
//        try
//        {
//            // Tải file lên MinIO Server
//            await _minioClient.PutObjectAsync("my-bucket", file.FileName, file.OpenReadStream(), file.Length, file.ContentType);
//            return Ok("File uploaded successfully!");
//        }
//        catch (Exception ex)
//        {
//            return StatusCode(500, $"Internal server error: {ex.Message}");
//        }
//    }

//    [HttpGet("download")]
//    public async Task<IActionResult> Download(string fileName)
//    {
//        try
//        {
//            // Tải file từ MinIO Server
//            var stream = new MemoryStream();
//            await _minioClient.GetObjectAsync("my-bucket", fileName, (sendStream) =>
//            {
//                sendStream.CopyTo(stream);
//                return Task.CompletedTask;
//            });
//            stream.Position = 0;
//            return File(stream, "application/octet-stream", fileName);
//        }
//        catch (Exception ex)
//        {
//            return NotFound($"File '{fileName}' not found: {ex.Message}");
//        }
//    }
//}
