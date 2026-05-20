using AutomatedTaskSystem.Models.Configs;
using AutomatedTaskSystem.Services.Leave.BackgroundService;
using Microsoft.Extensions.Options;

namespace AutomatedTaskSystem.Services.Leave
{
    /// <summary>
    /// Runs daily and triggers the annual leave reset when the configured reset date is reached.
    /// Uses IOptionsMonitor so appsettings changes to LeaveSettings (e.g. ResetDate) are picked up on the next run without restart.
    /// </summary>
    public class LeaveResetBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LeaveResetBackgroundService> _logger;

        public LeaveResetBackgroundService(IServiceScopeFactory scopeFactory, IOptionsMonitor<LeaveSettings> leaveSettingsMonitor, ILogger<LeaveResetBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Leave reset background service started. Runs every 24 hours; reset runs only on the configured ResetDate when not already run for that year.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var resetService = scope.ServiceProvider.GetRequiredService<ILeaveResetService>();
                    await resetService.RunAnnualResetAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in leave reset background run.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
