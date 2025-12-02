namespace SamplesHR.Backend.Models.RavenDBAiAgent;

public class Reply
{
    public string Answer { get; set; } = string.Empty;
    public string[] Followups { get; set; } = [];
}
