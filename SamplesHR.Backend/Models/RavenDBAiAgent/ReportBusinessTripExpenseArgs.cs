namespace SamplesHR.Backend.Models.RavenDBAiAgent;

public class ReportBusinessTripExpenseArgs
{
    public required string Vendor { get; set; }
    public required string Category { get; set; }
    public required decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public required string ExpenseDate { get; set; }
    public required string Description { get; set; }
}
