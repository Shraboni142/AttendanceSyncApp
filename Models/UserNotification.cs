using System;

namespace AttendanceSyncApp.Models
{
    public class UserNotification
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
