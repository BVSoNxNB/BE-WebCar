using Microsoft.AspNetCore.Identity;

namespace WebCar.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Car> Cars { get; set; }

    }
}
