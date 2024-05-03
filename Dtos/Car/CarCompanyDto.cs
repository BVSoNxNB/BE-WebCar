using System.ComponentModel.DataAnnotations;

namespace WebCar.Dtos.Car
{
    public class CarCompanyDto
    {
        [Required(ErrorMessage = "Ten hang xe khong duoc bo trong")]
        public string ten { get; set; }
        public string logo { get; set; }
        
    }
}
