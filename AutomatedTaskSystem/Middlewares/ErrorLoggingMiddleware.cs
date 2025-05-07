using System.Text;

namespace AutomatedTaskSystem.Middlewares
{
    public class ErrorLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _logDirectory;

        public ErrorLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
            _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ErrorLogs");

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Proceed to next middleware
            }
            catch (Exception ex)
            {
                await LogErrorToFileAsync(ex, context);
                throw; // Re-throw to preserve error handling
            }
        }

        private async Task LogErrorToFileAsync(Exception ex, HttpContext context)
        {
            var logFilePath = Path.Combine(_logDirectory, $"error_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.txt");

            var logContent = new StringBuilder();
            logContent.AppendLine($"Time: {DateTime.UtcNow:O}");
            logContent.AppendLine($"Request Path: {context.Request.Path}");
            logContent.AppendLine($"Query: {context.Request.QueryString}");
            logContent.AppendLine($"Method: {context.Request.Method}");
            logContent.AppendLine("Exception:");
            logContent.AppendLine(ex.ToString());

            await File.WriteAllTextAsync(logFilePath, logContent.ToString());
        }
    }
}
