using WebCar.Repository;
using WebCar.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebCar.Models;
namespace WebCar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    { 
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Route For Seeding my roles to DB
        [HttpPost]
        [Route("seed-roles")]
        [Authorize(Roles = Models.Role.ADMIN)]
        public async Task<IActionResult> SeedRoles()
        {
             var seerRoles = await _authService.SeedRolesAsync();

            return Ok(seerRoles);
        }
        // Route -> Register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var registerResult = await _authService.RegisterAsync(registerDto);

            if (registerResult.IsSucceed)
                return Ok(registerResult);

            return BadRequest(registerResult);
        }


        // Route -> Login
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var loginResult = await _authService.LoginAsync(loginDto);

            if(loginResult.IsSucceed)
                return Ok(loginResult);

            return Unauthorized(loginResult);
        }

        // Route -> make user -> admin
        [HttpPost]
        [Route("make-admin")]
        [Authorize(Roles = Models.Role.ADMIN)]
        public async Task<IActionResult> MakeAdmin([FromBody] UpdatePermissionDto updatePermissionDto)
        {
             var operationResult = await _authService.MakeAdminAsync(updatePermissionDto);

            if(operationResult.IsSucceed)
                return Ok(operationResult);

            return BadRequest(operationResult);
        }

        [HttpGet]
        [Route("getAllRole")]
        [Authorize(Roles = Role.ADMIN)]
        public async Task<IActionResult> GetAllRole()
        {
            var operationResult = await _authService.GetAllRolesAsync();

            if (operationResult.IsSucceed)
                return Ok(operationResult);
            else
                return BadRequest(operationResult);
        }
        //// Route -> Logout
        //[HttpPost]
        //[Route("logout")]
        //public async Task<IActionResult> Logout([FromBody] string token)
        //{
        //    var logoutResult = await _authService.LogoutAsync(token);

        //    if (logoutResult.IsSucceed)
        //        return Ok(logoutResult);

        //    return BadRequest(logoutResult);
        //}
        [HttpGet]
        [Route("GetRoleUserByUserNameAsync")]
        [Authorize(Roles = Role.ADMIN)]
        public async Task<IActionResult> GetRoleUserByUserNameAsync(string userName)
        {
            try
            {
                var result = await _authService.GetRoleUserByUserNameAsync(userName);

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
    }
}

