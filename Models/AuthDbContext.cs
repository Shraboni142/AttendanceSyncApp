using System.Data.Entity;
using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Models.SalaryGarbge;

namespace AttandanceSyncApp.Models
{
    /// <summary>
    /// DbContext for AttandanceSync database (authentication and sync requests)
    /// </summary>
    public class AuthDbContext : DbContext
    {
        public AuthDbContext() : base("name=AttendanceSyncConnection")
        {
            // ===== CRITICAL FIX ADDED =====
            Database.SetInitializer<AuthDbContext>(null);
            // ==============================
        }

        public DbSet<UserApprovalStatus> UserApprovalStatuses { get; set; }
 
        public DbSet<UserApprovalStatus> UserApprovalStatus { get; set; }

        // Auth entities
        public DbSet<User> Users { get; set; }
        public DbSet<LoginSession> LoginSessions { get; set; }

        // Sync entities
        public DbSet<SyncCompany> Companies { get; set; }
        public DbSet<Tool> Tools { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<AttandanceSyncRequest> AttandanceSyncRequests { get; set; }
        public DbSet<CompanyRequest> CompanyRequests { get; set; }
        public DbSet<DatabaseConfiguration> DatabaseConfigurations { get; set; }
        public DbSet<DatabaseAssign> DatabaseAssignments { get; set; }
        public DbSet<UserTool> UserTools { get; set; }

        // SalaryGarbge entities
        public DbSet<ServerIp> ServerIps { get; set; }
        public DbSet<DatabaseAccess> DatabaseAccess { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoginSession>()
                .HasRequired(ls => ls.User)
                .WithMany(u => u.LoginSessions)
                .HasForeignKey(ls => ls.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<AttandanceSyncRequest>()
                .HasRequired(sr => sr.User)
                .WithMany(u => u.SyncRequests)
                .HasForeignKey(sr => sr.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AttandanceSyncRequest>()
                .HasRequired(sr => sr.Employee)
                .WithMany(e => e.SyncRequests)
                .HasForeignKey(sr => sr.EmployeeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AttandanceSyncRequest>()
                .HasRequired(sr => sr.Company)
                .WithMany(c => c.SyncRequests)
                .HasForeignKey(sr => sr.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AttandanceSyncRequest>()
                .HasRequired(sr => sr.Tool)
                .WithMany(t => t.SyncRequests)
                .HasForeignKey(sr => sr.ToolId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AttandanceSyncRequest>()
                .HasRequired(sr => sr.Session)
                .WithMany()
                .HasForeignKey(sr => sr.SessionId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseConfiguration>()
                .HasRequired(dc => dc.Company)
                .WithMany()
                .HasForeignKey(dc => dc.CompanyId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<LoginSession>()
                .HasIndex(ls => ls.SessionToken)
                .IsUnique();

            modelBuilder.Entity<DatabaseConfiguration>()
                .HasIndex(dc => dc.CompanyId)
                .IsUnique();

            modelBuilder.Entity<CompanyRequest>()
                .HasRequired(cr => cr.User)
                .WithMany(u => u.CompanyRequests)
                .HasForeignKey(cr => cr.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CompanyRequest>()
                .HasRequired(cr => cr.Employee)
                .WithMany(e => e.CompanyRequests)
                .HasForeignKey(cr => cr.EmployeeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CompanyRequest>()
                .HasRequired(cr => cr.Company)
                .WithMany(c => c.CompanyRequests)
                .HasForeignKey(cr => cr.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CompanyRequest>()
                .HasRequired(cr => cr.Tool)
                .WithMany(t => t.CompanyRequests)
                .HasForeignKey(cr => cr.ToolId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CompanyRequest>()
                .HasRequired(cr => cr.Session)
                .WithMany()
                .HasForeignKey(cr => cr.SessionId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseAssign>()
                .HasRequired(da => da.CompanyRequest)
                .WithMany()
                .HasForeignKey(da => da.CompanyRequestId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseAssign>()
                .HasRequired(da => da.AssignedByUser)
                .WithMany()
                .HasForeignKey(da => da.AssignedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseAssign>()
                .HasRequired(da => da.DatabaseConfiguration)
                .WithMany()
                .HasForeignKey(da => da.DatabaseConfigurationId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseAssign>()
                .HasIndex(da => da.CompanyRequestId)
                .IsUnique();

            modelBuilder.Entity<UserTool>()
                .HasRequired(ut => ut.User)
                .WithMany(u => u.UserTools)
                .HasForeignKey(ut => ut.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserTool>()
                .HasRequired(ut => ut.Tool)
                .WithMany(t => t.UserTools)
                .HasForeignKey(ut => ut.ToolId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserTool>()
                .HasRequired(ut => ut.AssignedByUser)
                .WithMany()
                .HasForeignKey(ut => ut.AssignedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserTool>()
                .HasIndex(ut => new { ut.UserId, ut.ToolId });

            modelBuilder.Entity<ServerIp>()
                .HasIndex(s => s.IpAddress)
                .IsUnique();

            modelBuilder.Entity<DatabaseAccess>()
                .HasRequired(da => da.ServerIp)
                .WithMany()
                .HasForeignKey(da => da.ServerIpId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<DatabaseAccess>()
                .HasIndex(da => new { da.ServerIpId, da.DatabaseName })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
