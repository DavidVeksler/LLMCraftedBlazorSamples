namespace LLMCraftedBlazorSamples.Shared.Models;

public enum LogLevel
{
    Info,
    Warning,
    Error
}

public static class LogLevelExtensions
{
    public static string ToShortString(this LogLevel level) => level switch
    {
        LogLevel.Info => "INF",
        LogLevel.Warning => "WRN",
        LogLevel.Error => "ERR",
        _ => string.Empty
    };

    public static LogLevel FromShortString(string shortString) => shortString switch
    {
        "INF" => LogLevel.Info,
        "WRN" => LogLevel.Warning,
        "ERR" => LogLevel.Error,
        _ => LogLevel.Info
    };

    public static string GetTableClass(this LogLevel level) => level switch
    {
        LogLevel.Error => "table-danger",
        LogLevel.Warning => "table-warning",
        _ => string.Empty
    };

    public static string GetBackgroundClass(this LogLevel level) => level switch
    {
        LogLevel.Error => "bg-danger",
        LogLevel.Warning => "bg-warning text-dark",
        _ => "bg-info"
    };
}
