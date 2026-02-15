using System;

namespace AttandanceSyncApp.Models.DTOs.Admin
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // ✅ MUST HAVE
        public string Email { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class EmployeeCreateDto
    {
        public string Name { get; set; }

        // ✅ NEW ADDED
        public string Email { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class EmployeeUpdateDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        // ✅ NEW ADDED
        public string Email { get; set; }

        public bool IsActive { get; set; }
    }
}
