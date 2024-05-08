using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        private readonly KafkaProducerService _kafkaProducerService;
        public OrderController (IOrderService orderService,KafkaProducerService kafkaProducerService )
        {
            _orderService = orderService;
            _kafkaProducerService = kafkaProducerService;
        }
        [HttpPost]
        [Route("Order")]
        [Authorize(Roles = Models.Role.USER)]
        public async Task<IActionResult> Order([FromBody] OrderDto orderDto)
        {
            var orderResuilt = await _orderService.Order(orderDto);

            if (orderResuilt.IsSucceed)
            {
                var message = JsonConvert.SerializeObject(orderDto);
                await _kafkaProducerService.ProduceMessageAsync("WebCar", message);
                return Ok(orderResuilt);
            }

            return BadRequest(orderResuilt);
        }
        [HttpGet]
        [Authorize(Roles = Models.Role.ADMIN)]
        [Route("getAllOrder")]
        public async Task<IActionResult> getAllOrder()
        {
            try
            {
                var result = await _orderService.getAllOrderAsync();

                if (result.IsSucceed)
                {
                    return Ok(result.responseData); // Return the responseData retrieved from the service
                }
                else
                {
                    return NotFound(result.Message); // Return a not found message if the operation fails
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // Return a generic error message for internal server errors
            }
        }
        [HttpGet]
        [Authorize(Roles = Models.Role.ADMIN)]
        [Route("getOrderById/{id}")]
        public async Task<IActionResult> getOrderByIdAsync(int id)
        {
            try
            {
                var result = await _orderService.getOrderByIdAsync(id);

                if (result.IsSucceed)
                {
                    return Ok(result.responseData); // Return the data retrieved from the service
                }
                else
                {
                    return NotFound(result.Message); // Return a not found message if the operation fails
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // Return a generic error message for internal server errors
            }
        }
        [HttpGet]
        [Route("getOrderByIdStatus/{id}")]
        [Authorize(Roles = Models.Role.ADMIN)]

        public async Task<IActionResult> getOrderByStatusAsync(int id)
        {
            try
            {
                var result = await _orderService.getOrderByStatusAsync(id);

                if (result.IsSucceed)
                {
                    return Ok(result.responseData); // Return the data retrieved from the service
                }
                else
                {
                    return NotFound(result.Message); // Return a not found message if the operation fails
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // Return a generic error message for internal server errors
            }
        }
        [HttpPut]
        [Authorize(Roles = Models.Role.ADMIN)]
        [Route("updateStatus/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusDto status)
        {
            var updateResult = await _orderService.UpdateStatus(id, status  );

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
