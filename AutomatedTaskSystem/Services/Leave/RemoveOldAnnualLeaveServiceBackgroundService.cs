using AutomatedTaskSystem.Models.Configs;
using AutomatedTaskSystem.Services.Leave.BackgroundService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AutomatedTaskSystem.Services.Leave
{
    public class RemoveOldAnnualLeaveServiceBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly LeaveSettings _leaveSettings;
        private readonly ILogger<RemoveOldAnnualLeaveServiceBackgroundService> _logger;

        public RemoveOldAnnualLeaveServiceBackgroundService(IServiceScopeFactory scopeFactory, IOptions<LeaveSettings> leaveSettings, ILogger<RemoveOldAnnualLeaveServiceBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _leaveSettings = leaveSettings?.Value ?? new LeaveSettings();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Remove Old Annual Leaves background service started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var resetService = scope.ServiceProvider.GetRequiredService<IRemoveOldAnnualLeaveService>();
                    await resetService.RunRemoveOldAnnualLeavesAsync();
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
