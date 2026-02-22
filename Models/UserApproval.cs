using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSyncApp.Models
{
    [Table("UserApproval")]
    public class UserApproval
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public string EmployeeEmail { get; set; }

        public int CompanyId { get; set; }

        public int ToolId { get; set; }

        [NotMapped]
        public string PasswordHash { get; set; }



        public string ApprovalStatus { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
