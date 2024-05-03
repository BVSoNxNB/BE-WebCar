using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebCar.Models;

namespace WebCar.DbContext
{
    public class myDbContext : IdentityDbContext<ApplicationUser>
    {
        public myDbContext(DbContextOptions<myDbContext> options) : base(options)
        {
        }
        public DbSet<Car> Cars { get; set; } // DbSet for the Car entity
        public DbSet<CarCompany> CarCompanies { get; set; } // DbSet for the CarCompany entity

        public DbSet<Order> Orders { get; set; }    

    }
}

