namespace SamplesHR.Backend.Models.RavenDBAiAgent;

public class RaiseIssueArgs
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Category { get; set; }
    public required string Priority { get; set; }
}