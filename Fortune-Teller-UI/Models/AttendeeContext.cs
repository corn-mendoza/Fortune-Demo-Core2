using Microsoft.EntityFrameworkCore;

namespace FortuneTeller.Models
{
    public class AttendeeContext : DbContext
    {
        public AttendeeContext(DbContextOptions<AttendeeContext> options)
            : base(options)
        {
        }

        public DbSet<FortuneTeller.Models.AttendeeModel> AttendeeModel { get; set; }
    }
}