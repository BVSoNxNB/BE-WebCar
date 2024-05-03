namespace WebCar.Repository
{
    using WebCar.Dtos;
    using WebCar.Dtos.Car;

    public interface ICarCompanyService
    {
        Task<AuthServiceResponseDto> createCarCompanyAsync(CarCompanyDto carCompanyDto);
        Task<AuthServiceResponseDto> getCarCompanyByIdAsync(int carCompanyId);
        Task<AuthServiceResponseDto> getAllCarCompanyAsync();
        Task<AuthServiceResponseDto> updateCarCompanyAsync(int carCompanyId,CarCompanyDto carCompanyDto);
        Task<AuthServiceResponseDto> deleteCarCompanyAsync(int carCompanyId);

    }
}
