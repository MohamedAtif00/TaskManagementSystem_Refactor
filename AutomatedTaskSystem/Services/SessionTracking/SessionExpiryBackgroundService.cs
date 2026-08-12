namespace AutomatedTaskSystem.Services.SessionTracking;

/// <summary>
/// Periodically closes open sessions whose refresh tokens have expired.
/// </summary>
public class SessionExpiryBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SessionExpiryBackgroundService> _logger;

    public SessionExpiryBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SessionExpiryBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session expiry background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sessionService = scope.ServiceProvider.GetRequiredService<IUserSessionService>();
                await sessionService.CloseExpiredSessionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing expired user sessions.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
