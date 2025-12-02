namespace SamplesHR.Backend.Application.Usage;

public class SessionApiUsage
{
    public string Id { get; set; } = default!;

    public string SessionId { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    public string? UserAgent { get; set; }
    public string? ClientIP { get; set; }
    public string? AcceptLanguage { get; set; }
    public string? Referer { get; set; }

    public UtmInfo UTM { get; set; } = new();
}

public class UtmInfo
{
    public string? Source { get; set; }
    public string? Medium { get; set; }
    public string? Campaign { get; set; }
    public string? Term { get; set; }
    public string? Content { get; set; }
}