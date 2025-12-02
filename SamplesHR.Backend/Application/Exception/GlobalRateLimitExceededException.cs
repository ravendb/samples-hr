namespace SamplesHR.Backend.Application.Exception;

public class GlobalRateLimitExceededException(string message) : System.Exception(message);