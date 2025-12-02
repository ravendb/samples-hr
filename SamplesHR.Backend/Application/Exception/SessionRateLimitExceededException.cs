namespace SamplesHR.Backend.Application.Exception;

public class SessionRateLimitExceededException(string message) : System.Exception(message);