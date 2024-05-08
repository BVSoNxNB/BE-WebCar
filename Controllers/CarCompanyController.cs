using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Text;
using WebCar.Dtos;
using WebCar.Dtos.Car;
using WebCar.Models;
using WebCar.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebCar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarCompanyController : ControllerBase
    {
        
        private readonly ICarCompanyService _carCompanyService;
        private readonly KafkaProducerService _kafkaProducerService;
        

        public CarCompanyController(ICarCompanyService carCompanyService, MinIOService minIOService, KafkaProducerService kafkaProducerService, IConfiguration configuration)
        {
            _carCompanyService = carCompanyService;
            _kafkaProducerService = kafkaProducerService;

        }
        // GET api/<ValuesController>/5
        [HttpGet]
        [Route("getCarCompanyById/{id}")]
        public async Task<IActionResult> getCarCompanyById(int id)
        {
            try
            {
                var result = await _carCompanyService.getCarCompanyByIdAsync(id);

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
        [Route("getAllCarCompany")]
        public async Task<IActionResult> getAllCarCompany()
        {
            try
            {
                var result = await _carCompanyService.getAllCarCompanyAsync();

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
        // POST api/<ValuesController>
        [HttpPost]
        [Authorize(Roles = Models.Role.ADMIN)]
        [Route("create-CarCompany")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> createCarCompany([FromForm] CarCompanyDto carCompanyDto)
        {
            var registerResult = await _carCompanyService.createCarCompanyAsync(carCompanyDto);

            if (registerResult.IsSucceed)
            {
                var message = JsonConvert.SerializeObject(carCompanyDto);
                //await _kafkaProducerService.ProduceMessageAsync("WebCar", message);
                return Ok(registerResult);
            }

            return BadRequest(registerResult);
        }
        // PUT api/<ValuesController>/5
        [HttpPut]
        [Authorize(Roles = Models.Role.ADMIN)]
        [Route("updateCarCompany/{id}")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UpdateCarCompany(int id, [FromForm] CarCompanyDto carCompanyDto)
        {
            var updateResult = await _carCompanyService.updateCarCompanyAsync(id, carCompanyDto);

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
        [Route("deleteCarCompany/{id}")]
        public async Task<IActionResult> deleteCarCompany(int id)
        {
            var deleteResult = await _carCompanyService.deleteCarCompanyAsync(id);

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
