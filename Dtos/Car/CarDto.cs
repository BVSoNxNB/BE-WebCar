using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebCar.Models;

namespace WebCar.Dtos.Car
{
    public class CarDto
    {
        [Required(ErrorMessage = "Ten khong duoc bo trong")]
        public string ten { get; set; }
        public List<String> hinh { get; set; }
        public string phienBan { get; set; }
        [Required(ErrorMessage = "NamSX khong duoc bo trong")]
        public int namSanXuat { get; set; }
        public double dungTich { get; set; }
        [Required(ErrorMessage = "Hop so khong duoc bo trong")]
        public string hopSo { get; set; }
        public string kieuDang { get; set; }
        public string tinhTrang { get; set; }
        [Required(ErrorMessage = "Nhien lieu khong duoc bo trong")]
        public string nhienLieu { get; set; }
        public int kichThuoc { get; set; }
        [Required(ErrorMessage = "So ghe khong duoc bo trong")]
        public int soGhe { get; set; }
        [Required(ErrorMessage = "Gia khong duoc bo trong")]
        public decimal gia { get; set; }
        [Required(ErrorMessage = "Hang xe khong duoc bo trong")]
        public int maHangXe { get; set; }
       
    }
}
