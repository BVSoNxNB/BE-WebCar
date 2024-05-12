using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
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
        private readonly MinIOService _minIOService;

        public carService(myDbContext dbContext, IRedisCache cache, MinIOService minIOService)
        {
            _dbContext = dbContext;
            _cache = cache;
            _minIOService = minIOService;
        }

        public async Task<AuthServiceResponseDto> createCarAsync(CarDto carDto)
        {
            try
            {
                // Lưu trữ ảnh của xe trên MinIO
                List<string> imageUrls = new List<string>();
                if (carDto.hinhAnh != null && carDto.hinhAnh.Any())
                {
                    foreach (var imageFile in carDto.hinhAnh)
                    {
                        var imageUrl = await _minIOService.UploadImageAsync(imageFile);
                        imageUrls.Add(imageUrl);
                    }
                }

                // Tạo một đối tượng Car từ dữ liệu đầu vào
                var car = new Car
                {
                    ten = carDto.ten, 
                    hinh = imageUrls,
                    hinhAnh = carDto.hinhAnh,
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
                var cars = await _dbContext.Cars.FirstOrDefaultAsync(c => c.id == carId);

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
        public async Task<AuthServiceResponseDto> getCarByIdCarCompanyAsync(int carCompanyId)
        {
            try
            {
                //kiem tra da co du lieu dc luu trong cache chua
                var cachedData = await _cache.Get($"CarByCarcompanyId_{carCompanyId}");
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
                var cars = await _dbContext.Cars
                                            .Where(c => c.CarCompanyId == carCompanyId)
                                            .ToListAsync();

                if (cars != null)
                {
                    // add du lieu tu data vao cache 
                    //await _cache.Add($"CarByCarcompanyId_{carCompanyId}", JsonSerializer.Serialize(cars));
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
                        Message = $"Không tìm thấy Car với ID {carCompanyId}"
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
                // Lấy danh sách ảnh cũ từ existingCar
                var existingImageUrls = existingCar.hinh.ToList();

                // Lấy danh sách ảnh mới từ carDto
                var newImageUrls = new List<string>();
                if (carDto.hinhAnh != null && carDto.hinhAnh.Any())
                {
                    foreach (var imageFile in carDto.hinhAnh)
                    {
                        var fileName = Path.GetFileName(imageFile.FileName);
                        var existingImageUrl = existingImageUrls.FirstOrDefault(url => url.EndsWith(fileName));

                        if (existingImageUrl != null)
                        {
                            // Giữ lại ảnh cũ nếu nó vẫn còn trong danh sách mới
                            newImageUrls.Add(existingImageUrl);
                        }
                        else
                        {
                            // Thêm mới ảnh nếu nó chưa tồn tại
                            var imageUrl = await _minIOService.UploadImageAsync(imageFile);
                            newImageUrls.Add(imageUrl);
                        }
                    }
                }

                // Xóa các ảnh không còn được sử dụng khỏi MinIO
                var imagesToDelete = existingImageUrls.Except(newImageUrls).ToList();
                foreach (var imageUrl in imagesToDelete)
                {
                    var segments = imageUrl.Split('/');
                    var bucketName = segments[0];
                    var objectName = segments[1];
                    await _minIOService.DeleteImageAsync(bucketName, objectName);
                }

                // Gán danh sách ảnh mới cho existingCar
                existingCar.hinh = newImageUrls;

                // Cập nhật thông tin Car
                existingCar.ten = carDto.ten;
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
                    // Xóa các ảnh cũ khỏi MinIO
                    foreach (var imageUrl in existingCar.hinh)
                    {
                        var segments = imageUrl.Split('/');
                        var bucketName = segments[0];
                        var objectName = segments[1];
                        await _minIOService.DeleteImageAsync(bucketName, objectName);
                    }
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
