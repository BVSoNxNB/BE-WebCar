using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using WebCar.Dtos.Car;
using WebCar.Dtos.Order;
using WebCar.Repository;
using WebCar.Services;

namespace WebCar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController (IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpPost]
        [Route("Order")]
        [Authorize(Roles = Models.Role.USER)]
        public async Task<IActionResult> Order([FromBody] OrderDto orderDto)
        {
            var orderResuilt = await _orderService.Order(orderDto);

            if (orderResuilt.IsSucceed)
                return Ok(orderResuilt);

            return BadRequest(orderResuilt);
        }
        [HttpPut]
        //[Authorize(Roles = Models.Role.ADMIN)]
        [Route("updateCarCompany /{id}")]
        public async Task<IActionResult> UpdateCarCompany(int id, [FromBody] StatusDto status)
        {
            var updateResult = await _orderService.UpdateStatus(id, status);

            if (updateResult.IsSucceed)
            {
                return Ok(updateResult.Message);
            }
            else
            {
                return BadRequest(updateResult); // HTTP 400 Bad Request with error details
            }
        }

    }
}
