using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebCar.DbContext;
using WebCar.Dtos;
using WebCar.Dtos.Car;
using WebCar.Models;
using WebCar.Repository;


namespace WebCar.Services
{
    public class carService : ICarService
    {
        //Tiêm để sử dụng 
        private readonly myDbContext _dbContext;
        private readonly IRedisCache _cache;
        public carService(myDbContext dbContext, IRedisCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<AuthServiceResponseDto> createCarAsync(CarDto carDto)
        {
            try
            {

                // Tạo một đối tượng Car từ dữ liệu đầu vào
                var car = new Car
                {
                    ten = carDto.ten, // Chú ý đến việc map dữ liệu từ DTO vào entity
                    hinh = carDto.hinh,
                    phienBan = carDto.phienBan,
                    namSanXuat = carDto.namSanXuat,
                    dungTich = carDto.dungTich,
                    hopSo = carDto.hopSo,
                    kieuDang = carDto.kieuDang,
                    tinhTrang = carDto.tinhTrang,
                    nhienLieu = carDto.nhienLieu,
                    kichThuoc = carDto.kichThuoc,
                    soGhe = carDto.soGhe,
                    gia = carDto.gia,
                    CarCompanyId = carDto.maHangXe,
                };

                // Thêm đối tượng Car vào DbContext
                _dbContext.Cars.Add(car);
                // Lưu thay đổi vào cơ sở dữ liệu
                await _dbContext.SaveChangesAsync();
                await _cache.Delete("allCars");

                return new AuthServiceResponseDto { IsSucceed = true, Message = "Tạo Car thành công" };
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return new AuthServiceResponseDto { IsSucceed = false, Message = "Đã xảy ra lỗi khi tạo Car", responseData = new List<string> { ex.Message } };
            }
        }
        public async Task<AuthServiceResponseDto> getCarByIdAsync(int carId)
        {
            try
            {
                //kiem tra da co du lieu dc luu trong cache chua
                var cachedData = await _cache.Get($"Car_{carId}");
                if (cachedData != null)
                {
                    //da ton tai. thi convert du lieu tu byte qua json
                    var car = JsonSerializer.Deserialize<Car>(cachedData);
                    //tra du lieu ra 
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin Car từ cache thành công",
                        responseData = car
                    };
                }
                //chua co du lieu trong cache thi lay tu database
                var cars = await _dbContext.Cars.FirstOrDefaultAsync(c => c.Id == carId);

                if (cars != null)
                {
                    // add du lieu tu data vao cache 
                    await _cache.Add($"Car_{carId}", JsonSerializer.Serialize(cars));
                    //tra du lieu 
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin Car từ database thành công",
                        responseData = cars
                    };
                }
                else
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy Car với ID {carId}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthServiceResponseDto
                {
                    IsSucceed = false,
                    Message = $"Đã xảy ra lỗi khi lấy thông tin Car: {ex.Message}"
                };
            }
        }
        public async Task<AuthServiceResponseDto> getAllCarAsync()
        {
            try
            {
                //kiem tra ton tai
                var cachedData = await _cache.Get("allCars");
                if (cachedData != null)
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin tất cả Cars từ cache thành công",
                        responseData = JsonSerializer.Deserialize<List<Car>>(cachedData)
                    };
                }
                //chua co cache thi lay tu data
                var cars = await _dbContext.Cars.ToListAsync();

                if (cars != null)
                {
                    //luu cache
                    await _cache.Add("allCars", JsonSerializer.Serialize(cars));
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin tất cả Cars từ database thành công",
                        responseData = cars
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

        public async Task<AuthServiceResponseDto> updateCarAsync(int carId, CarDto carDto)
        {
            try
            {
                var existingCar = await _dbContext.Cars.FindAsync(carId);

                if (existingCar == null)
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy Car với ID {carId}"
                    };
                }

                // Update the properties of the existing car 
                existingCar.ten = carDto.ten; // Chú ý đến việc map dữ liệu từ DTO vào entity
                existingCar.hinh = carDto.hinh;
                existingCar.phienBan = carDto.phienBan;
                existingCar.namSanXuat = carDto.namSanXuat;
                existingCar.dungTich = carDto.dungTich;
                existingCar.hopSo = carDto.hopSo;
                existingCar.kieuDang = carDto.kieuDang;
                existingCar.tinhTrang = carDto.tinhTrang;
                existingCar.nhienLieu = carDto.nhienLieu;
                existingCar.kichThuoc = carDto.kichThuoc;
                existingCar.soGhe = carDto.soGhe;
                existingCar.gia = carDto.gia;

                // Save changes to the database
                await _dbContext.SaveChangesAsync();
                await _cache.Delete("allCars");
                await _cache.Delete($"Car_{carId}");

                return new AuthServiceResponseDto
                {
                    IsSucceed = true,
                    Message = "Cập nhật thông tin Car thành công"
                };
            }
            catch (Exception ex)
            {
                return new AuthServiceResponseDto
                {
                    IsSucceed = false,
                    Message = $"Đã xảy ra lỗi khi cập nhật thông tin Car: {ex.Message}"
                };
            }
        }
        public async Task<AuthServiceResponseDto> deleteCarAsync(int carId)
        {
            try
            {
                var existingCar = await _dbContext.Cars.FindAsync(carId);

                if (existingCar == null)
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy Car với ID {carId}"
                    };
                }
                else
                {
                    _dbContext.Cars.Remove(existingCar);

                    // Save changes to the database
                    await _dbContext.SaveChangesAsync();
                    // Xoa cache cu neu co
                    await _cache.Delete("allCars");
                    await _cache.Delete($"Car_{carId}");

                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Xoá Car thành công"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthServiceResponseDto
                {
                    IsSucceed = false,
                    Message = $"Đã xảy ra lỗi khi xoá Car: {ex.Message}"
                };
            }
        }
    }
}
