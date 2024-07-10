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

            var logFiles = Directory.GetFiles(logDirectory, "*.log")
                .Select(Path.GetFileName)
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

            List<LogEntry> logEntries;
            using (var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var streamReader = new StreamReader(fileStream))
            {
                var logContent = streamReader.ReadToEnd();
                logEntries = ExtractLogEntries(logContent);
                
            }
            return Ok(logEntries);
        }

        private List<LogEntry> ExtractLogEntries(string logContent)
        {
            var logEntries = new List<LogEntry>();
            var lines = logContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var entry = ParseLogEntry(line);
                if (entry != null && entry.Type != "INF")
                {
                    logEntries.Add(entry);
                }
            }
            return logEntries;
        }

        private LogEntry ParseLogEntry(string line)
        {
            // IIS log format: yyyy-MM-dd HH:mm:ss.fff +/-zz:zz [LOG_LEVEL] Message
            var match = System.Text.RegularExpressions.Regex.Match(line, @"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2}) \[(.*?)\] (.*)$");
            if (match.Success)
            {
                return new LogEntry
                {
                    Timestamp = match.Groups[1].Value.Trim(),
                    Type = match.Groups[2].Value.Trim(),
                    Message = match.Groups[3].Value.Trim()
                };
            }
            return null;
        }
    }

    public class LogEntry
    {
        public string Timestamp { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }
}