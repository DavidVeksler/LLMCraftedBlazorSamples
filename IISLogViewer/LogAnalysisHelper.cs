using System.Globalization;
using System.Text.RegularExpressions;
using PayTech.BackOffice.BackOfficeWeb.Controllers;
using LLMCraftedBlazorSamples.Shared.Models;

namespace PayTech.BackOffice.BackOfficeWeb.Services;

public static class LogAnalysisHelper
{
    public static string GetRowClass(string entryType) =>
        LogLevelExtensions.FromShortString(entryType).GetTableClass();

    public static string GetBadgeClass(string entryType) =>
        LogLevelExtensions.FromShortString(entryType).GetBackgroundClass();

    public static IEnumerable<IGrouping<string, string>> GroupLogFilesByMonth(IEnumerable<string> logFiles) =>
        logFiles
            .Select(file => new
            {
                FileName = file,
                Date = ParseDateFromFileName(file)
            })
            .Where(x => x.Date.HasValue)
            .OrderByDescending(x => x.Date)
            .GroupBy(x => x.Date.Value.ToString("MMMM yyyy"),
                x => x.FileName);

    public static DateTime? ParseDateFromFileName(string fileName)
    {
        if (DateTime.TryParseExact(
                fileName.Substring(11, 8),
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime fileDate))
        {
            return fileDate;
        }
        return null;
    }

    public static string FormatLogFileName(string fileName)
    {
        var date = ParseDateFromFileName(fileName);
        return date.HasValue
            ? $"{date.Value:dd MMM yyyy} - {fileName}"
            : fileName;
    }

    public static List<ErrorSummary> GetMostCommonErrors(IEnumerable<LogEntry> logEntries)
    {
        var errorLevels = new[] { LogLevel.Error.ToShortString(), LogLevel.Warning.ToShortString() };

        return logEntries
            .Where(e => errorLevels.Contains(e.Type))
            .Select(e => new
            {
                Entry = e,
                Exception = ExtractExceptionName(e.Message)
            })
            .GroupBy(x => x.Exception)
            .Select(g => new ErrorSummary
            {
                ExceptionName = g.Key,
                Count = g.Count(),
                SampleMessages = g.Select(x => x.Entry.Message).Take(3).ToList()
            })
            .OrderByDescending(e => e.Count)
            .Take(5)
            .ToList();
    }

    public class ErrorSummary
    {
        public string ExceptionName { get; set; }
        public int Count { get; set; }
        public List<string> SampleMessages { get; set; }
    }

    private static string ExtractExceptionName(string message)
    {
        // Try to extract exception name from the message
        var match = System.Text.RegularExpressions.Regex.Match(message, @"(?<=^|\s)([A-Z][a-zA-Z]*Exception)\b");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
    
        // If no exception name found, return a summarized version of the message
        return message.Length > 50 ? message.Substring(0, 47) + "..." : message;
    }

    public static string GetErrorType(string errorMessage)
    {
        var match = Regex.Match(errorMessage, @"^([a-zA-Z.]+Exception)");
        return match.Success ? match.Groups[1].Value : "Unknown Error";
    }

    public static double GetAverageResponseTime(IEnumerable<LogEntry> logEntries)
    {
        var responseTimes = logEntries
            .Where(e => e.Message.Contains("responded"))
            .Select(e =>
            {
                var match = Regex.Match(e.Message, @"responded \d+ in (\d+(\.\d+)?) ms");
                return match.Success && double.TryParse(match.Groups[1].Value, out double time) ? time : 0;
            })
            .Where(t => t > 0);

        return responseTimes.Any() ? Math.Round(responseTimes.Average(), 2) : 0;
    }

    public static int GetErrorsInLastHour(IEnumerable<LogEntry> logEntries)
    {
        var oneHourAgo = DateTime.Now.AddHours(-1);
        var errorType = LogLevel.Error.ToShortString();
        return logEntries.Count(e => e.Type == errorType && DateTime.Parse(e.Timestamp) >= oneHourAgo);
    }
}