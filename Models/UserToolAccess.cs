using System;

public class UserToolAccess
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ToolId { get; set; }
    public bool IsApproved { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
}
