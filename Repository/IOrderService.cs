using WebCar.Dtos;
using WebCar.Dtos.Order;

namespace WebCar.Repository
{
    public interface IOrderService
    {
        Task<AuthServiceResponseDto> Order(OrderDto orderDTO);
        Task<AuthServiceResponseDto> UpdateStatus(int statusId,StatusDto status);
    }
}
