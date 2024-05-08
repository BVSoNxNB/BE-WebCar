using Microsoft.EntityFrameworkCore;
using WebCar.DbContext;
using WebCar.Dtos;
using WebCar.Dtos.Car;
using WebCar.Models;
using System.Text.Json;
using WebCar.Repository;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.StaticFiles;

namespace WebCar.Services
{
    public class carCompanyService : ICarCompanyService
    {
        private readonly myDbContext _dbContext;
        private readonly IRedisCache _cache;
        
            private readonly MinIOService _minIOService;
        private readonly IConfiguration _configuration;

        public carCompanyService(myDbContext dbContext, IRedisCache cache, IConfiguration configuration, MinIOService minIOService)
        {
            _dbContext = dbContext;
            _cache = cache;
            _configuration = configuration;
            _minIOService = minIOService;
        }

        public async Task<AuthServiceResponseDto> createCarCompanyAsync(CarCompanyDto carCompanyDto)
        {
            try
            {
                // Kiểm tra xem có file ảnh được tải lên hay không
                if (carCompanyDto.LogoFile != null && carCompanyDto.LogoFile.Length > 0)
                {
                    // Lưu trữ ảnh logo của công ty ô tô trên MinIO
                    var logoUrl = await _minIOService.UploadImageAsync(carCompanyDto.LogoFile);
                    // Tạo đối tượng CarCompany từ CarCompanyDto
                    var carCompany = new CarCompany
                    {
                        name = carCompanyDto.name,
                        logo = logoUrl,
                        LogoFile = carCompanyDto.LogoFile
                    };

                    // Lưu vào database
                    _dbContext.CarCompanies.Add(carCompany);
                    await _dbContext.SaveChangesAsync();
                    await _cache.Delete("allCarCompanies");

                    // Trả về khi gọi API
                    return new AuthServiceResponseDto { IsSucceed = true, Message = "Tạo CarCompany thành công" };
                }
                else
                {
                    // Nếu không có file ảnh được tải lên, trả về lỗi
                    return new AuthServiceResponseDto { IsSucceed = false, Message = "Vui lòng tải lên file ảnh" };
                }
            }
            catch (Exception ex)
            {
                return new AuthServiceResponseDto { IsSucceed = false, Message = $"Đã xảy ra lỗi khi tạo CarCompany: {ex.Message}" };
            }
        }

        public async Task<AuthServiceResponseDto> getCarCompanyByIdAsync(int carCompanyId)
        {
            try
            {
                //kiem tra da co du lieu dc luu trong cache chua
                var cachedData = await _cache.Get($"CarCompany_{carCompanyId}");
                if (cachedData != null)
                {
                    //da ton tai. thi convert du lieu tu byte qua json
                    var carCompany = JsonSerializer.Deserialize<CarCompany>(cachedData);
                    //tra du lieu ra 
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin CarCompany từ cache thành công",
                        responseData = carCompany
                    };
                }

                //chua co du lieu trong cache thi lay tu database
                var carCompanys = await _dbContext.CarCompanies.FirstOrDefaultAsync(c => c.Id == carCompanyId);

                if (carCompanys != null)
                {
                    // Trả về đối tượng CarCompany bao gồm cả LogoFile
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin CarCompany từ database thành công",
                        responseData = carCompanys
                    };
                }
                else
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy CarCompany với ID {carCompanyId}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthServiceResponseDto
                {
                    IsSucceed = false,
                    Message = $"Đã xảy ra lỗi khi lấy thông tin CarCompany: {ex.Message}"
                };
            }
        }

        public async Task<AuthServiceResponseDto> getAllCarCompanyAsync()
        {
            try
            {
                //kiem tra ton tai
                var cachedData = await _cache.Get("allCarCompanies");
                if (cachedData != null)
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin tất cả CarCompanies từ cache thành công",
                        responseData = JsonSerializer.Deserialize<List<CarCompany>>(cachedData)
                    };
                }
                //chua co cache thi lay tu data
                var carCompanies = await _dbContext.CarCompanies.ToListAsync();

                if (carCompanies != null)
                {
                    //luu cache
                    await _cache.Add("allCarCompanies", JsonSerializer.Serialize(carCompanies));
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin tất cả CarCompanies từ database thành công",
                        responseData = carCompanies
                    };
                }
                else
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy bất kỳ CarCompany nào"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthServiceResponseDto
                {
                    IsSucceed = false,
                    Message = $"Đã xảy ra lỗi khi lấy thông tin CarCompanies: {ex.Message}"
                };
            }
        }

        public async Task<AuthServiceResponseDto> updateCarCompanyAsync(int carCompanyId, CarCompanyDto carCompanyDto)
        {
            try
            {
                // Lấy CarCompany từ database
                var existingCarCompany = await _dbContext.CarCompanies.FindAsync(carCompanyId);

                if (existingCarCompany == null)
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy CarCompany với ID {carCompanyId}"
                    };
                }

                // Xử lý cập nhật ảnh logo mới (nếu có)
                string logoUrl = existingCarCompany.logo;
                if (carCompanyDto.LogoFile != null && carCompanyDto.LogoFile.Length > 0)
                {
                    var segments = logoUrl.Split('/');
                    var bucketName = segments[0];
                    var objectName = segments[1];

                    // Xóa object trên MinIO
                    await _minIOService.DeleteImageAsync(bucketName, objectName);

                    logoUrl = await _minIOService.UploadImageAsync(carCompanyDto.LogoFile);

                }

                // Cập nhật thông tin CarCompany
                existingCarCompany.name = carCompanyDto.name;
                existingCarCompany.logo = logoUrl;

                // Lưu thay đổi vào database
                await _dbContext.SaveChangesAsync();
                await _cache.Delete("allCarCompanies");
                await _cache.Delete($"CarCompany_{carCompanyId}");

                return new AuthServiceResponseDto
                {
                    IsSucceed = true,
                    Message = "Cập nhật thông tin CarCompany thành công"
                };
            }
            catch (Exception ex)
            {
                return new AuthServiceResponseDto
                {
                    IsSucceed = false,
                    Message = $"Đã xảy ra lỗi khi cập nhật thông tin CarCompany: {ex.Message}"
                };
            }
        }

        public async Task<AuthServiceResponseDto> deleteCarCompanyAsync(int carCompanyId)
        {
            try
            {
                // Tìm dữ liệu trong database qua id để xóa
                var existingCarCompany = await _dbContext.CarCompanies.FindAsync(carCompanyId);

                // Kiểm tra tồn tại
                if (existingCarCompany == null)
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy CarCompany với ID {carCompanyId}"
                    };
                }

                // Lấy tên bucket và tên object từ URL logo
                var logoUrl = existingCarCompany.logo;
                var segments = logoUrl.Split('/');
                var bucketName = segments[0];
                var objectName = segments[1];

                // Xóa object trên MinIO
                await _minIOService.DeleteImageAsync(bucketName, objectName);

                // Xóa dữ liệu
                _dbContext.CarCompanies.Remove(existingCarCompany);
                await _dbContext.SaveChangesAsync();
                await _cache.Delete("allCarCompanies");
                await _cache.Delete("allCars");
                await _cache.Delete($"CarCompany_{carCompanyId}");

                return new AuthServiceResponseDto
                {
                    IsSucceed = true,
                    Message = "Xóa CarCompany thành công"
                };
            }
            catch (Exception ex)
            {
                return new AuthServiceResponseDto
                {
                    IsSucceed = false,
                    Message = $"Đã xảy ra lỗi khi xóa CarCompany: {ex.Message}"
                };
            }
        }
    }
}
