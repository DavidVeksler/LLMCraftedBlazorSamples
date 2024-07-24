using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;

namespace PayTech.BackOffice.BackOfficeWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly string _logDirectory;

        public LogsController(IConfiguration configuration)
        {
            _logDirectory = configuration["Logging:Directory"];
        }

        [HttpGet]
        public IActionResult GetLogFiles(string webApp)
        {
            var logDirectory = Path.Combine(_logDirectory, webApp, "logs");
            if (!Directory.Exists(logDirectory))
            {
                return NotFound("Log directory not found.");
            }

            List<string> logFiles = Directory.GetFiles(logDirectory, "*.log")
                .Select(Path.GetFileName)
                .Where(fileName => fileName != null)
                .OrderByDescending(fileName => 
                {
                    // Extract date from filename (assuming format: applicationYYYYMMDD.log)
                    if (DateTime.TryParseExact(
                            fileName.Substring(11, 8), 
                            "yyyyMMdd", 
                            CultureInfo.InvariantCulture, 
                            DateTimeStyles.None, 
                            out DateTime fileDate))
                    {
                        return fileDate;
                    }
                    return DateTime.MinValue; // For files that don't match the expected format
                })
                .ToList();

            return Ok(logFiles);
        }

        [HttpGet("{webApp}/{fileName}")]
        public IActionResult GetLogContent(string webApp, string fileName)
        {
            var logFilePath = Path.Combine(_logDirectory, webApp, "logs", fileName);
            if (!System.IO.File.Exists(logFilePath))
            {
                return NotFound("Log file not found.");
            }

            var logEntries = new List<LogEntry>();
            const int bufferSize = 4096;
            var buffer = new char[bufferSize];
            var leftOver = string.Empty;

            using (var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var streamReader = new StreamReader(fileStream))
            {
                fileStream.Seek(0, SeekOrigin.End);
                long position = fileStream.Position;

                while (position > 0)
                {
                    int charsToRead = (int)Math.Min(bufferSize, position);
                    position -= charsToRead;
                    fileStream.Seek(position, SeekOrigin.Begin);

                    int charsRead = streamReader.ReadBlock(buffer, 0, charsToRead);

                    var text = new string(buffer, 0, charsRead);
                    var lines = (text + leftOver).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = lines.Length - 1; i >= 0; i--)
                    {
                        var entry = ParseLogEntry(lines[i]);
                        if (entry != null)
                        {
                            logEntries.Add(entry);
                        }
                    }

                    leftOver = lines[0];
                }
            }

            logEntries.Reverse(); // Reverse the list to get chronological order

            return Ok(logEntries);
        }

        private List<LogEntry> ExtractLogEntries(string logContent)
        {
            var logEntries = new List<LogEntry>();
            var lines = logContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            LogEntry currentEntry = null;

            foreach (var line in lines)
            {
                var newEntry = ParseLogEntry(line);
                if (newEntry != null)
                {
                    if (currentEntry != null)
                    {
                        logEntries.Add(currentEntry);
                    }
                    currentEntry = newEntry;
                }
                else if (currentEntry != null)
                {
                    currentEntry.Message += Environment.NewLine + line;
                }
            }

            if (currentEntry != null)
            {
                logEntries.Add(currentEntry);
            }

            return logEntries;
        }

        private LogEntry ParseLogEntry(string line)
        {
            // Updated regex to handle both formats
            var match = System.Text.RegularExpressions.Regex.Match(line, @"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2}) (?:\[(\d+)\] )?\[(.*?)\] (.*)$");
            if (match.Success)
            {
                return new LogEntry
                {
                    Timestamp = match.Groups[1].Value.Trim(),
                    ProcessId = match.Groups[2].Success ? match.Groups[2].Value : null,
                    Type = match.Groups[3].Value.Trim(),
                    Message = match.Groups[4].Value.Trim()
                };
            }
            return null;
        }
    }

    public class LogEntry
    {
        public string Timestamp { get; set; }
        public string? ProcessId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }
}