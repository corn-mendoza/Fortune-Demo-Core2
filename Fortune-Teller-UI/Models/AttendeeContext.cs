using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Workshop_UI.Models
{
    public class AttendeeContext : DbContext
    {
        public AttendeeContext(DbContextOptions<AttendeeContext> options)
            : base(options)
        {
        }

        public DbSet<Workshop_UI.Models.AttendeeModel> AttendeeModel { get; set; }
    }
}