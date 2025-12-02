namespace SamplesHR.Backend.Models;

public class HrIssue
{
    public string? Id { get; set; }
    public required string EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Category { get; set; } // Benefits, Payroll, IT Request, Policy Question, etc.
    public required string Priority { get; set; } // Low, Medium, High, Urgent
    public required string Status { get; set; } // Open, In Progress, Resolved, Closed
    public DateTime SubmittedDate { get; set; }
    public DateTime? AssignedDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? AssignedTo { get; set; }
    public string? Resolution { get; set; }
    public List<HrIssueComment> Comments { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

public class HrIssueComment
{
    public string Id { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsInternal { get; set; } = false; // Internal HR notes vs employee communication
}
