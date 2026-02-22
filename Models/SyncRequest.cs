using System;

namespace AttendanceSyncApp.Models
{
    public class SyncRequest
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public string Email { get; set; }

        public int CompanyId { get; set; }

        public int ToolId { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
