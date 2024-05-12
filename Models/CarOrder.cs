namespace WebCar.Models
{
   public class CarOrder
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int CarId { get; set; }
        public Car Car { get; set; }
    }
}
