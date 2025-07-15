using AutomatedTaskSystem.Data;
using Cronos;
namespace AutomatedTaskSystem.Services
{
    public class MonthlyDatabaseOperationWorker : BackgroundService
    {
        private readonly ILogger<MonthlyDatabaseOperationWorker> _logger;
        private readonly IServiceProvider _serviceProvider; // To create a scope for DbContext
        private readonly string _cronExpression = "0 0 21 * *"; // Runs at 00:00 (midnight) on the 21st day of every month
        private CronExpression _expression;

        public MonthlyDatabaseOperationWorker(ILogger<MonthlyDatabaseOperationWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _expression = CronExpression.Parse(_cronExpression);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Monthly Database Operation Worker running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var nextOccurrence = _expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local);

                if (nextOccurrence.HasValue)
                {
                    var delay = nextOccurrence.Value - DateTimeOffset.Now;

                    if (delay.TotalMilliseconds <= 0)
                    {
                        _logger.LogWarning("Scheduled time is in the past or immediate. Executing operation now.");
                        await PerformDatabaseOperation();
                        // Recalculate next occurrence for the next period
                        nextOccurrence = _expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local);
                        if (!nextOccurrence.HasValue)
                        {
                            _logger.LogError("Could not determine next occurrence after immediate execution. Worker might stop.");
                            break;
                        }
                        delay = nextOccurrence.Value - DateTimeOffset.Now;
                    }

                    _logger.LogInformation($"Next scheduled run for database operation: {nextOccurrence.Value}");
                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await PerformDatabaseOperation();
                    }
                }
                else
                {
                    _logger.LogWarning("No future occurrences found for the cron expression. Worker will stop.");
                    break;
                }
            }

            _logger.LogInformation("Monthly Database Operation Worker stopped.");
        }

        private async Task PerformDatabaseOperation()
        {
            _logger.LogInformation($"Performing database operation: Resetting Permission and WorkFromHome for all users at: {DateTimeOffset.Now}");

            try
            {
                // Create a new scope for the DbContext to ensure it's properly disposed
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Get the DbContext from the service provider
                    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>(); // Replace with your DbContext class name

                    // Fetch all users
                    var users = await dbContext.Users.ToListAsync();

                    if (users != null && users.Any())
                    {
                        foreach (var user in users)
                        {
                            user.Permission = 0;
                            user.WorkFromHome = 0;
                        }

                        // Save changes to the database
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation($"Successfully reset Permission and WorkFromHome for {users.Count} users.");
                    }
                    else
                    {
                        _logger.LogInformation("No users found to reset Permission and WorkFromHome.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while performing the database operation to reset user permissions.");
            }
        }
    }
}
