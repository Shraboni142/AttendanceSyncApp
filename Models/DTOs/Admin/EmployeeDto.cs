using System;

public class EmployeeDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }   // ✅ MUST EXIST

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}


public class EmployeeCreateDto
{
    public string Name { get; set; }

    public string Email { get; set; }   // ✅ ADD

    public bool IsActive { get; set; } = true;
}

public class EmployeeUpdateDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }   // ✅ MUST EXIST

    public bool IsActive { get; set; }
}

