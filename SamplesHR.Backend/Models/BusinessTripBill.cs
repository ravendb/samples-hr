namespace SamplesHR.Backend.Models;

public class BusinessTripBill
{
    public string? Id { get; set; }
    public required string EmployeeId { get; set; }
    public required string Vendor { get; set; }
    public required string Category { get; set; } // Transportation, Accommodation, Meals, Flight, Conference, Other
    public required decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public required DateTime ExpenseDate { get; set; }
    public required string Description { get; set; }
    public DateTime ReportedAt { get; set; }
}
