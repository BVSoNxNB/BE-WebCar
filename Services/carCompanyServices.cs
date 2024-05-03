using Microsoft.EntityFrameworkCore;
using WebCar.DbContext;
using WebCar.Dtos;
using WebCar.Dtos.Car;
using WebCar.Models;
using System.Text.Json;
using WebCar.Repository;

namespace WebCar.Services
{
    public class carCompanyService : ICarCompanyService
    {
        private readonly myDbContext _dbContext;
        private readonly IRedisCache _cache;

        public carCompanyService(myDbContext dbContext, IRedisCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<AuthServiceResponseDto> createCarCompanyAsync(CarCompanyDto carCompanyDto)
        {
            try
            {
                // nhan du lieu qua Dto
                var carCompany = new CarCompany
                {
                    name = carCompanyDto.ten,
                    logo = carCompanyDto.logo
                };
                //luu vao database
                _dbContext.CarCompanies.Add(carCompany);
                await _dbContext.SaveChangesAsync();
                await _cache.Delete("allCarCompanies");
                //tra ve khi goi api 
                return new AuthServiceResponseDto { IsSucceed = true, Message = "Tạo CarCompany thành công" };
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
                    // add du lieu tu data vao cache 
                    await _cache.Add($"CarCompany_{carCompanyId}", JsonSerializer.Serialize(carCompanys));
                    //tra du lieu 
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
                //lay du lieu tu data qua id
                var existingCarCompany = await _dbContext.CarCompanies.FindAsync(carCompanyId);

                if (existingCarCompany == null)
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy CarCompany với ID {carCompanyId}"
                    };
                }
                //gan du lieu moi 
                existingCarCompany.name = carCompanyDto.ten;
                existingCarCompany.logo = carCompanyDto.logo;
                //luu du lieu vao data
                await _dbContext.SaveChangesAsync();
                await _cache.Delete("allCarCompanies");
                await _cache.Delete($"CarCompany_{carCompanyId}");
                //tra du lieu
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
                //tim du lieu trong data qua id de xoa
                var existingCarCompany = await _dbContext.CarCompanies.FindAsync(carCompanyId);
                //kiem tra ton tai
                if (existingCarCompany == null)
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy CarCompany với ID {carCompanyId}"
                    };
                }
                //xoa du lieu
                _dbContext.CarCompanies.Remove(existingCarCompany);
                await _dbContext.SaveChangesAsync();
                await _cache.Delete("allCarCompanies");
                await _cache.Delete($"CarCompany_{carCompanyId}");

                //tra du lieu ra man hinh
                return new AuthServiceResponseDto
                {
                    IsSucceed = true,
                    Message = "Xoá CarCompany thành công"
                };
            }
            catch (Exception ex)
            {
                return new AuthServiceResponseDto
                {
                    IsSucceed = false,
                    Message = $"Đã xảy ra lỗi khi xoá CarCompany: {ex.Message}"
                };
            }
        }
    }
}
