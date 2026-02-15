using AttandanceSyncApp.Models.AttandanceSync;
using AttendanceSyncApp.Models;
using System.Data.Entity;

namespace AttandanceSyncApp.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
            : base("name=AttendanceSyncConnection")
        {
        }

        // Dynamic connection er jonno
        public AppDbContext(string connectionString)
            : base(connectionString)
        {
        }

        public DbSet<Company> Companies { get; set; }

        public DbSet<AttandanceSynchronization> AttandanceSynchronizations { get; set; }

        public DbSet<SignupRequest> SignupRequests { get; set; }

        // ✅ ONLY ADDED THESE TWO LINES (NO CHANGE to existing code)
        public DbSet<Tool> Tools { get; set; }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
