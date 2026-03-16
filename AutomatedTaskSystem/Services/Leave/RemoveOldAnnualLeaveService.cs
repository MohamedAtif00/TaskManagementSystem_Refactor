using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models.Configs;
using AutomatedTaskSystem.Services.Log;
using Microsoft.Extensions.Options;

namespace AutomatedTaskSystem.Services.Leave.BackgroundService
{
    public class RemoveOldAnnualLeaveService : IRemoveOldAnnualLeaveService
    {
        private readonly DataContext _dataContext;
        private readonly IOptionsMonitor<LeaveSettings> _leaveSettingsMonitor;
        private readonly ILogService _logService;

        public RemoveOldAnnualLeaveService(ILogService logService, IOptionsMonitor<LeaveSettings> leaveSettingsMonitor, DataContext dataContext)
        {
            _logService = logService;
            _leaveSettingsMonitor = leaveSettingsMonitor;
            _dataContext = dataContext;
        }

        public async Task RunRemoveOldAnnualLeavesAsync()
        {
            var today = DateTime.Today;
            int year = today.Year;
            var leaveSettings = _leaveSettingsMonitor.CurrentValue ?? new LeaveSettings();

            var removeDate = LeaveSettings.ParseDateForYear(leaveSettings.RemoveOldAnnualLeavesDate, year);
            if (removeDate == null || today.Date != removeDate.Value.Date)
            {
                _logService.LogInformation("Remove old annual leaves skipped: today ({Today:yyyy-MM-dd}) is not the configured RemoveOldAnnualLeavesDate ({RemoveDate}). Configured value: \"{Config}\".", today, removeDate ?? (DateTime?)null, leaveSettings.RemoveOldAnnualLeavesDate ?? "(null)");
                return;
            }

            var users = await _dataContext.Users.Where(u => !u.Archived).ToListAsync();
            foreach (var user in users)
            {
                user.Annual_leave_MAX -= user.OldAnnualBalance;
                user.Annual_leave -= user.OldAnnualBalance;
                user.OldAnnualBalance = 0;

                _dataContext.Users.Update(user);
            }

            await _dataContext.SaveChangesAsync();
            _logService.LogInformation("Old annual leaves removed for year {Year}. {Count} users updated. Old balances no longer available after {RemoveDate}.", year, users.Count, removeDate.Value.ToString("yyyy-MM-dd"));
        }
    }
}
