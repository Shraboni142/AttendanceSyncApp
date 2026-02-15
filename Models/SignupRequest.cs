using System;

public class SignupRequest
{
    public int Id { get; set; }

    public string EmployeeName { get; set; }

    public int? EmployeeId { get; set; }

    // ✅ ADD THIS LINE (FIXES YOUR ERROR)
    public int? ToolId { get; set; }

    public string Email { get; set; }

    public string Company { get; set; }

    public string Tool { get; set; }

    public string Status { get; set; }

    public DateTime CreatedDate { get; set; }
}
