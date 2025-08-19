using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting; // For IWebHostEnvironment
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedTaskSystem.Services.Log
{
    public class LogService : ILogService
    {
        private readonly ILogger<LogService> _logger;
        private readonly string _logDirectory;
        private readonly string _logFileNamePrefix = "application_log"; // Prefix for all log files

        public LogService(ILogger<LogService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            // Define a specific directory for custom file logs
            // It's usually a good practice to place logs outside of the web-accessible folders (like wwwroot)
            _logDirectory = Path.Combine(webHostEnvironment.ContentRootPath, "Logs");

            if (!Directory.Exists(_logDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_logDirectory);
                    // Log to the internal ILogger that the directory was created
                    _logger.LogInformation($"Successfully created log directory at: {_logDirectory}");
                }
                catch (Exception ex)
                {
                    // If directory creation fails, log this critical error
                    _logger.LogCritical(ex, $"Failed to create log directory at: {_logDirectory}. Check file system permissions.");
                    // You might want to throw here or handle more gracefully if logging is absolutely essential
                }
            }
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args); // Log to standard providers
            _ = WriteLogToFileAsync(LogLevel.Information, null, message, args); // Log to file
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args); // Log to standard providers
            _ = WriteLogToFileAsync(LogLevel.Warning, null, message, args); // Log to file
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            _logger.LogError(exception, message, args); // Log to standard providers
            _ = WriteLogToFileAsync(LogLevel.Error, exception, message, args); // Log to file
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug(message, args); // Log to standard providers
            _ = WriteLogToFileAsync(LogLevel.Debug, null, message, args); // Log to file
        }

        public void LogCritical(Exception exception, string message, params object[] args)
        {
            _logger.LogCritical(exception, message, args); // Log to standard providers
            _ = WriteLogToFileAsync(LogLevel.Critical, exception, message, args); // Log to file
        }

        /// <summary>
        /// Writes a log entry to a daily rotating file.
        /// </summary>
        /// <param name="logLevel">The log level of the entry.</param>
        /// <param name="ex">The exception associated with the log entry, if any.</param>
        /// <param name="messageTemplate">The message template string.</param>
        /// <param name="args">The arguments for the message template.</param>
        private async Task WriteLogToFileAsync(
            LogLevel logLevel,
            Exception ex,
            string messageTemplate,
            params object[] args)
        {
            // Use a daily log file name
            var logFileName = $"{_logFileNamePrefix}_{DateTime.UtcNow:yyyyMMdd}.txt";
            var logFilePath = Path.Combine(_logDirectory, logFileName);

            // Format the message using the template and arguments
            string formattedMessage = string.Format(messageTemplate, args);

            var logContent = new StringBuilder();
            logContent.AppendLine($"Time: {DateTime.UtcNow:O}"); // ISO 8601 format
            logContent.AppendLine($"Level: {logLevel}");
            logContent.AppendLine($"Message: {formattedMessage}");

            if (ex != null)
            {
                logContent.AppendLine("Exception Details:");
                logContent.AppendLine(ex.ToString()); // Includes stack trace and inner exceptions
            }
            logContent.AppendLine("---"); // Separator for multiple entries

            try
            {
                // Append to the file. If the file doesn't exist, it will be created.
                await File.AppendAllTextAsync(logFilePath, logContent.ToString());
            }
            catch (Exception fileEx)
            {
                // Fallback: If writing to the file fails, log this specific failure
                // using the primary _logger, so you're aware of the file logging issue.
                _logger.LogError(fileEx,
                                 "Failed to write log to file: {LogFilePath}. Original log level: {LogLevel}, Message: {OriginalMessage}",
                                 logFilePath, logLevel, formattedMessage);
            }
        }
    }
}