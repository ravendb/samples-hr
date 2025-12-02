namespace SamplesHR.Backend.Hubs;

public record UsageUpdate(
    int CurrentUsage,
    int MaxRequests,
    int RequestsLeft,
    DateTime Timestamp,
    DateTime WindowStart,
    List<DataPoint> RecentPoints
);

public record DataPoint(DateTime Timestamp, int Count, string? SessionId = null);

