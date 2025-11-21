namespace LLMCraftedBlazorSamples.Shared;

public static class AppConstants
{
    public static class Paths
    {
        public const string WebAppsDirectory = "C:\\web";
        public const int FileReadBufferSize = 4096;
    }

    public static class Email
    {
        public const string SystemsEmail = "systems@paytech.systems";
    }

    public static class Api
    {
        // Set this flag appropriately in your application startup/configuration
        public static bool IsProduction = false;

        public static string BitPayWebhookUrl =>
            IsProduction
                ? "https://api.paytech.systems/api/BitPayWebhook"
                : "https://api-test.test.paytech.systems/api/BitPayWebhook";
    }

    public static class Timeouts
    {
        public const int LogRefreshMinutes = 5;
        public const int RecentErrorsHours = 1;
    }
}
