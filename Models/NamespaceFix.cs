// NamespaceFix.cs
// This file bridges AttendanceSyncApp namespace to AttandanceSyncApp namespace
// WITHOUT modifying any existing files
using System.Data.Entity;

namespace AttendanceSyncApp.Models
{
    public class AppDbContext : AttandanceSyncApp.Models.AppDbContext
    {
        public AppDbContext() : base() { }

        public AppDbContext(string connectionString) : base(connectionString) { }
    }
}

namespace AttendanceSyncApp.Models.AttandanceSync
{
    public class Tool : AttandanceSyncApp.Models.AttandanceSync.Tool
    {
    }

    public class Employee : AttandanceSyncApp.Models.AttandanceSync.Employee
    {
    }
}
