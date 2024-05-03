using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using WebCar.DbContext;
using WebCar.Dtos;
using WebCar.Dtos.Order;
using WebCar.Models;
using WebCar.Repository;

namespace WebCar.Services
{
    public class OrderService : IOrderService
    {
        private readonly myDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRedisCache _cache;

        public OrderService(myDbContext myDbContext, IHttpContextAccessor httpContextAccessor, IRedisCache cache)
        {
            _dbContext = myDbContext;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
        }

        public async Task<AuthServiceResponseDto> Order(OrderDto orderDTO)
        {
            try
            {
                // Lấy HttpContext từ dịch vụ
                var httpContext = _httpContextAccessor.HttpContext;

                // Lấy Id của người dùng hiện tại
                var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                // Tạo một đối tượng Order từ dữ liệu đầu vào và Id của người dùng đang đăng nhập
                var order = new Models.Order
                {
                    UserId = userId,
                    NameUser = orderDTO.UserName,
                    PhoneNumber = orderDTO.PhoneNumber,
                    Email = orderDTO.Email,
                    Text = orderDTO.Text,
                    Status = 0,
                    carId = orderDTO.carId,
                    //Cars = _dbContext.Cars.Select(c => new Car
                    //{
                    //    // Ánh xạ các thuộc tính của đối tượng Car từ dữ liệu nhận được
                    //    id = c.id,
                    //    ten = c.ten,
                    //    hinh = c.hinh,
                    //    phienBan = c.phienBan,
                    //    namSanXuat = c.namSanXuat,
                    //    dungTich = c.dungTich,
                    //    hopSo = c.hopSo,
                    //    kieuDang = c.kieuDang,
                    //    tinhTrang = c.tinhTrang,
                    //    nhienLieu = c.nhienLieu,
                    //    kichThuoc = c.kichThuoc,
                    //    soGhe = c.soGhe,
                    //    gia = c.gia,
                    //    CarCompany = c.CarCompany,
                    //    CarCompanyId = c.CarCompanyId,
                    //}).ToList()
                };

                // Thêm đối tượng Order vào DbContext
                _dbContext.Orders.Add(order);
                await _cache.Delete("allOrders");
                // Lưu thay đổi vào cơ sở dữ liệu
                await _dbContext.SaveChangesAsync();

                return new AuthServiceResponseDto { IsSucceed = true, Message = "Gửi thành công" };
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return new AuthServiceResponseDto { IsSucceed = false, Message = "Đã xảy ra lỗi khi gửi", responseData = new List<string> { ex.Message } };
            }
        }
        public async Task<AuthServiceResponseDto> getOrderByIdAsync(int orderId)
        {
            try
            {
                //kiem tra da co du lieu dc luu trong cache chua
                var cachedData = await _cache.Get($"Order_{orderId}");
                if (cachedData != null)
                {
                    //da ton tai. thi convert du lieu tu byte qua json
                    var order = JsonSerializer.Deserialize<Models.Order>(cachedData);
                    //tra du lieu ra 
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin order từ cache thành công",
                        responseData = order
                    };
                }
                //chua co du lieu trong cache thi lay tu database
                var orders = await _dbContext.Orders.FirstOrDefaultAsync(c => c.Id == orderId);

                if (orders != null)
                {
                    // add du lieu tu data vao cache 
                    await _cache.Add($"CarCompany_{orderId}", JsonSerializer.Serialize(orders));
                    //tra du lieu 
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin orders từ database thành công",
                        responseData = orders
                    };
                }
                else
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy orders với ID {orderId}"
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
        public async Task<AuthServiceResponseDto> getOrderByStatusAsync(int statusId)
        {
            try
            {
                // Kiểm tra xem dữ liệu đã được lưu trong cache chưa
                var cachedData = await _cache.Get($"OrderStatus_{statusId}");
                if (cachedData != null)
                {
                    // Nếu đã tồn tại, chuyển đổi dữ liệu từ byte thành đối tượng Order
                    var orders = JsonSerializer.Deserialize<List<Models.Order>>(cachedData);

                    // Trả về dữ liệu từ cache
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin order từ cache thành công",
                        responseData = orders
                    };
                }

                // Nếu chưa có trong cache, lấy dữ liệu từ cơ sở dữ liệu
                var order = await _dbContext.Orders.Where(c => c.Status == statusId).ToListAsync();

                if (order != null)
                {
                    // Thêm dữ liệu từ database vào cache
                    await _cache.Add($"OrderStatus_{statusId}", JsonSerializer.Serialize(order));

                    // Trả về dữ liệu từ cơ sở dữ liệu
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin orders từ database thành công",
                        responseData = order
                    };
                }
                else
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy orders với ID {statusId}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthServiceResponseDto
                {
                    IsSucceed = false,
                    Message = $"Đã xảy ra lỗi khi lấy thông tin: {ex.Message}"
                };
            }
        }

        public async Task<AuthServiceResponseDto> getAllOrderAsync()
        {
            try
            {
                //kiem tra ton tai
                var cachedData = await _cache.Get("allOrders");
                if (cachedData != null)
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin tất cả don hang từ cache thành công",
                        responseData = JsonSerializer.Deserialize<List<Models.Order>>(cachedData)
                    };
                }
                //chua co cache thi lay tu data
                var order = await _dbContext.Orders.ToListAsync();

                if (order != null)
                {
                    //luu cache
                    await _cache.Add("allOrders", JsonSerializer.Serialize(order));
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = true,
                        Message = "Lấy thông tin tất cả orders từ database thành công",
                        responseData = order
                    };
                }
                else
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy bất kỳ don hang nào"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthServiceResponseDto
                {
                    IsSucceed = false,
                    Message = $"Đã xảy ra lỗi khi lấy thông tin don hang: {ex.Message}"
                };
            }
        }

        public async Task<AuthServiceResponseDto> UpdateStatus(int OrderId,StatusDto status)
        {
            try
            {
                //lay du lieu tu data qua id
                var existingOrder = await _dbContext.Orders.FindAsync(OrderId);

                if (existingOrder == null)
                {
                    return new AuthServiceResponseDto
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy đơn với ID {OrderId}"
                    };
                }
                //gan du lieu moi 
                existingOrder.Status = status.status;
                //luu du lieu vao data
                await _dbContext.SaveChangesAsync();
                await _cache.Delete("allOrders");
                //tra du lieu
                return new AuthServiceResponseDto
                {
                    IsSucceed = true,
                    Message = "Cập nhật thông tin đơn thành công"
                };
            }
            catch (Exception ex)
            {
                return new AuthServiceResponseDto
                {
                    IsSucceed = false,
                    Message = $"Đã xảy ra lỗi khi cập nhật thông tin đơn: {ex.Message}"
                };
            }
        }
    }
}
