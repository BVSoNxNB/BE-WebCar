using WebCar.Dtos;
using WebCar.Dtos.Order;

namespace WebCar.Repository
{
    public interface IOrderService
    {
        Task<AuthServiceResponseDto> Order(OrderDto orderDTO);
        Task<AuthServiceResponseDto> UpdateStatus(int statusId,StatusDto status);
        Task<AuthServiceResponseDto> getAllOrderAsync();
        Task<AuthServiceResponseDto> getOrderByIdAsync(int orderId);
        Task<AuthServiceResponseDto> getOrderByStatusAsync(int statusId);

    }
}
