using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace WebCar.Models
{
    public class Car
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Đánh dấu cột Id là Identity
        public int Id { get; set; }
        [Display(Name = "Full name")]
        public string ten { get; set; }
        public List<string> hinh { get; set; }
        public string phienBan { get; set; }
        public int namSanXuat { get; set; }
        public double dungTich { get; set; }
        public string hopSo { get; set; }
        public string kieuDang { get; set; }
        public string tinhTrang { get; set; }
        public string nhienLieu { get; set; }
        public int kichThuoc { get; set; }
        public int soGhe { get; set; }
        public decimal gia { get; set; }
        // Foreign key
        [ForeignKey("CarCompany")]
        public int CarCompanyId { get; set; }

        // Navigation property
        public CarCompany CarCompany { get; set; } // One-to-many relationship

        public ICollection<Order> Orders { get; set; }

    }
}
