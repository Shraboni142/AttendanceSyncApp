using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Models.Auth;
using AttendanceSyncApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AttandanceSyncApp.Models
{
    public class AppDbContext : DbContext
    {
        // Default constructor (main database)
        public AppDbContext() : base("name=AttandanceSyncConnection")
        {
        }

        /// <summary>
        /// Constructor for dynamic connection strings (external databases)
        /// </summary>
        public AppDbContext(string connectionString) : base(connectionString)
        {
        }

        // =============================
        // EXISTING TABLES
        // =============================

        public DbSet<Company> Companies { get; set; }

        public DbSet<SyncCompany> SyncCompanies { get; set; }

        public DbSet<AttandanceSynchronization> AttandanceSynchronizations { get; set; }

        public DbSet<UserApproval> UserApprovals { get; set; }
        public DbSet<UserTool> UserTools { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Tool> Tools { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }




        // =============================
        // ✅ ADD THIS (FOR ADMIN REQUEST SYSTEM)
        // =============================

        public DbSet<SyncRequest> SyncRequests { get; set; }


        // =============================
        // MODEL CONFIGURATION
        // =============================

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
