using System;

namespace AttandanceSyncApp.Models
{
    public class UserApprovalStatus
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public bool IsEmailVerified { get; set; }

        public bool IsAdminApproved { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
