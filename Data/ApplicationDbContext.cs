using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Auth;
using System.Data.Entity;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("name=AttendanceSyncConnection")
        {
        }

        public DbSet<Tool> Tools { get; set; }

        public DbSet<Employee> Employees { get; set; }

        // ✅ ADD THIS LINE (FIXES ERROR)
        public DbSet<SignupRequest> SignupRequests { get; set; }
    }
}
