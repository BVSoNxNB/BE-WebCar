using WebCar.DbContext;
using WebCar.Dtos;
using WebCar.Dtos.Car;
using WebCar.Dtos.Order;
using WebCar.Models;
using WebCar.Repository;

namespace WebCar.Services
{
    public class OrderService : IOrderService
    {
        private readonly myDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(myDbContext myDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = myDbContext;
            _httpContextAccessor = httpContextAccessor;
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
                var order = new Order
                {
                    UserId = userId,
                    NameUser = orderDTO.UserName,
                    PhoneNumber = orderDTO.PhoneNumber,
                    Email = orderDTO.Email,
                    Text = orderDTO.Text,
                    Status = 0,
                };

                // Thêm đối tượng Order vào DbContext
                _dbContext.Orders.Add(order);

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
