namespace WebCar.Repository
{
    using WebCar.Dtos;
    using WebCar.Models;

    public interface IAuthService
    {
        Task<AuthServiceResponseDto> SeedRolesAsync();
        Task<AuthServiceResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthServiceResponseDto> LoginAsync(LoginDto loginDto);
        //Task<AuthServiceResponseDto> LogoutAsync(string token);
        Task<AuthServiceResponseDto> MakeAdminAsync(UpdatePermissionDto updatePermissionDto);
        Task<AuthServiceResponseDto> GetAllRolesAsync();
        Task<AuthServiceResponseDto> GetRoleUserByUserNameAsync(string userName);
    }
}
