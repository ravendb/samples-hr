namespace SamplesHR.Backend.Models;

public class SampleBill
{
    public int Id { get; set; }
    public string Vendor { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageResourceName { get; set; } = string.Empty;
}

public static class SampleBillCatalog
{
    public static readonly SampleBill[] Bills =
    [
        new()
        {
            Id = 1,
            Vendor = "City Taxi Co.",
            Category = "Transportation",
            Amount = 45.00m,
            Date = "2026-03-03",
            Description = "Airport to hotel transfer in Seattle",
            ImageResourceName = "bill-1.png"
        },
        new()
        {
            Id = 2,
            Vendor = "Grand Plaza Hotel",
            Category = "Accommodation",
            Amount = 320.00m,
            Date = "2026-03-04",
            Description = "2-night stay (Mar 4–6) for TechConf 2026",
            ImageResourceName = "bill-2.png"
        },
        new()
        {
            Id = 3,
            Vendor = "La Bella Cucina",
            Category = "Meals",
            Amount = 78.50m,
            Date = "2026-03-04",
            Description = "Business dinner with client",
            ImageResourceName = "bill-3.png"
        },
        new()
        {
            Id = 4,
            Vendor = "SkyWay Airlines",
            Category = "Flight",
            Amount = 450.00m,
            Date = "2026-03-02",
            Description = "Economy class JFK → SEA round trip",
            ImageResourceName = "bill-4.png"
        },
        new()
        {
            Id = 5,
            Vendor = "TechConf 2026",
            Category = "Conference",
            Amount = 199.00m,
            Date = "2026-03-05",
            Description = "Standard pass + Cloud Architecture workshop",
            ImageResourceName = "bill-5.png"
        }
    ];
}
