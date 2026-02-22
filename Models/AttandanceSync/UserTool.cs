using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AttandanceSyncApp.Models.Auth;

namespace AttandanceSyncApp.Models.AttandanceSync
{
    [Table("UserTools")]
    public class UserTool
    {
        [Key]
        public int Id { get; set; }

        public bool IsActive { get; set; }

        // Foreign Keys
        [Required]
        public int UserId { get; set; }

        [Required]
        public int ToolId { get; set; }

        // Nullable (Admin may not exist in Users table)
        public int? AssignedBy { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.Now;

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // =========================
        // Navigation Properties
        // =========================

        public virtual User User { get; set; }

        public virtual Tool Tool { get; set; }

        public virtual User AssignedByUser { get; set; }
    }
}