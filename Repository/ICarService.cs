namespace WebCar.Repository
{
    using WebCar.Dtos;
    using WebCar.Dtos.Car;

    public interface ICarService
    {
        Task<AuthServiceResponseDto> createCarAsync(CarDto carDto);
        Task<AuthServiceResponseDto> getCarByIdAsync(int carId);
        Task<AuthServiceResponseDto> getAllCarAsync();
        Task<AuthServiceResponseDto> updateCarAsync(int carId, CarDto carDto);
        Task<AuthServiceResponseDto> deleteCarAsync(int carId);
    }
}
