using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCar.Models
{
    public class CarCompany
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Đánh dấu cột Id là Identity
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Full name")]
        public string name { get; set; }
        [Required]
        public String logo { get; set; }
        // Navigation property
        public ICollection<Car> Cars { get; set; } // One-to-many relationship
    }
}
