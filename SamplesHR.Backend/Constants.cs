namespace SamplesHR.Backend;

public static class Constants
{
    // Environment variable names
    public static class EnvVars
    {
        public const string MaxGlobalRequestsPer15Minutes = "SAMPLES_HR_MAX_GLOBAL_REQUESTS_PER_15_MINUTES";
        public const string MaxSessionRequestsPer30Seconds = "SAMPLES_HR_MAX_SESSION_REQUESTS_PER_30_SECONDS";
        public const string OpenAiApiKey = "SAMPLES_HR_OPENAI_API_KEY";
    }

    // RavenDB document IDs
    public static class DocumentIds
    {
        public const string GlobalApiUsage = "GlobalApiUsageLimiter/global";
        public static string SessionApiUsage(string sessionId) => $"ApiUsageSession/{sessionId}";
    }

    // Time series names
    public static class TimeSeries
    {
        public const string Requests = "Requests";
    }

    // Cookie names
    public static class Cookies
    {
        public const string SessionId = "samples-hr-session-id";
    }
}

