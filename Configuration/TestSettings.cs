namespace TfLJourneyPlanner.Tests.Configuration;

public static class TestSettings
{
    public const string BaseUrl = "https://tfl.gov.uk/plan-a-journey";
    public static bool Headless => !string.Equals(Environment.GetEnvironmentVariable("HEADLESS"), "false", StringComparison.OrdinalIgnoreCase);
}
