namespace WebCar.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string NameUser { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Text { get; set; }
        public int Status { get; set; }
        public string UserId { get; set; } // Foreign key to ApplicationUser
        public ApplicationUser User { get; set; }
        public ICollection<Car> Cars { get; set; }
    }
}
