using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Configs;
using AutomatedTaskSystem.Services.Log;
using Microsoft.Extensions.Options;

namespace AutomatedTaskSystem.Services.Leave.BackgroundService
{
    public class LeaveResetService : ILeaveResetService
    {
        private readonly DataContext _dataContext;
        private readonly IOptionsMonitor<LeaveSettings> _leaveSettingsMonitor;
        private readonly ILogService _logService;

        public LeaveResetService(DataContext dataContext, IOptionsMonitor<LeaveSettings> leaveSettingsMonitor, ILogService logService)
        {
            _dataContext = dataContext;
            _leaveSettingsMonitor = leaveSettingsMonitor;
            _logService = logService;
        }

        public async System.Threading.Tasks.Task RunAnnualResetAsync()
        {
            var today = DateTime.Today;
            int year = today.Year;
            // Read current config each run so appsettings changes (e.g. ResetDate) are picked up without restart
            var leaveSettings = _leaveSettingsMonitor.CurrentValue ?? new LeaveSettings();

            var alreadyRun = await _dataContext.LeaveResetLogs.AnyAsync(x => x.Year == year);
            if (alreadyRun)
            {
                _logService.LogInformation("Annual leave reset skipped: already run for year {Year}. To run again, remove the LeaveResetLog entry for that year.", year);
                return;
            }

            var resetDate = LeaveSettings.ParseDateForYear(leaveSettings.ResetDate, year);
            if (resetDate == null || today.Date != resetDate.Value.Date)
            {
                _logService.LogInformation("Annual leave reset skipped: today ({Today:yyyy-MM-dd}) is not the configured reset date ({ResetDate}). Configured ResetDate in appsettings: \"{ResetDateConfig}\".", today, resetDate ?? (DateTime?)null, leaveSettings.ResetDate ?? "(null)");
                return;
            }


            var users = await _dataContext.Users.Where(u => !u.Archived).ToListAsync();
            foreach (var user in users)
            {
                user.OldAnnualBalance = user.Annual_leave_MAX - user.Annual_leave;
                user.Annual_leave_MAX += user.OldAnnualBalance;
                user.Annual_leave = 0;
                user.Emergency_leave = 0;
                user.Sick_leave = 0;
                user.Permission = 0;
                _dataContext.Users.Update(user);
            }

            _dataContext.LeaveResetLogs.Add(new LeaveResetLog { Year = year, ExecutedAt = DateTime.UtcNow });
            await _dataContext.SaveChangesAsync();
            _logService.LogInformation("Annual leave reset completed for year {Year}. {Count} users updated.", year, users.Count);
        }
    }
}
