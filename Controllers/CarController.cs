using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCar.Dtos.Car;
using WebCar.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebCar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly ICarService _carService;
        public CarController(ICarService carService)
        {
            _carService = carService;
        }
        // GET api/<ValuesController>/5
        [HttpGet]
        [Route("getCarById/{id}")]
        public async Task<IActionResult> getCarById(int id)
        {
            try
            {
                var result = await _carService.getCarByIdAsync(id);

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
        [Route("getAllCar")]
        public async Task<IActionResult> getAllCar()
        {
            try
            {
                var result = await _carService.getAllCarAsync();

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
        // POST api/<ValuesController>
        [HttpPost]
        [Route("create-Car")]
        [Authorize(Roles = Models.Role.ADMIN)]
        public async Task<IActionResult> createCar([FromBody] CarDto carDto)
        {
            var registerResult = await _carService.createCarAsync(carDto);

            if (registerResult.IsSucceed)
                return Ok(registerResult);

            return BadRequest(registerResult);
        }

        // PUT api/<ValuesController>/5
        [HttpPut]
        [Route("updateCar /{id}")]
        [Authorize(Roles = Models.Role.ADMIN)]
        public async Task<IActionResult> updateCar(int id, [FromBody] CarDto carDto)
        {
            var updateResult = await _carService.updateCarAsync(id, carDto);

            if (updateResult.IsSucceed)
            {
                return Ok(updateResult.Message);
            }
            else
            {
                return BadRequest(updateResult); // HTTP 400 Bad Request with error details
            }
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete()]
        [Authorize(Roles = Models.Role.ADMIN)]
        [Route("deleteCar/{id}")]
        public async Task<IActionResult> deleteCar(int id)
        {
            var deleteResult = await _carService.deleteCarAsync(id);

            if (deleteResult.IsSucceed)
            {
                return Ok(deleteResult.Message);
            }
            else
            {
                return BadRequest(deleteResult); // HTTP 400 Bad Request with error details
            }
        }


    }
}
