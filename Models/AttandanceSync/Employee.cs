using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttandanceSyncApp.Models.AttandanceSync
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        // ✅ Email column
        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        // ✅ Active / Inactive status (use this only)
        public bool IsActive { get; set; } = true;

        // ❌ REMOVE Status column (CAUSE OF ERROR)

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<AttandanceSyncRequest> SyncRequests { get; set; }

        public virtual ICollection<CompanyRequest> CompanyRequests { get; set; }

        public Employee()
        {
            SyncRequests = new HashSet<AttandanceSyncRequest>();
            CompanyRequests = new HashSet<CompanyRequest>();
        }
    }
}
