using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Models;
using Microsoft.AspNetCore.SignalR;
using AutomatedTaskSystem.Hub;
using AutomatedTaskSystem.Data;
using static AutomatedTaskSystem.Services.Notification.NotificationService;

namespace AutomatedTaskSystem.Services.Notification
{


    public class NotificationService : INotificationService
    {
        private readonly IHubContext<UserHub> _hubContext;
        private readonly DataContext _dataContext;

        public NotificationService(IHubContext<UserHub> hubContext, DataContext dataContext)
        {
            _hubContext = hubContext;
            _dataContext = dataContext;
        }

        // Changed return type from Task to Task<bool>
        public async Task<bool> NotifyOwnerOfNewPendingLeaveRequest(int newLeaveRequestId)
        {
            var owner = await _dataContext.Users.FirstOrDefaultAsync(x => x.Role == UserRoleEnum.Owner);

            if (owner != null)
            {
                try
                {
                    var ownerClient = _hubContext.Clients.User(owner.Id.ToString());
                    var pendingCount = await _dataContext.LeaveRequests
                                            .Where(x => x.Status == LeaveRequestStatusEnum.Pending)
                                            .CountAsync();

                    await ownerClient.SendAsync("UpdatePendings", new
                    {
                        pendings = pendingCount,
                        isNewRequest = true,
                        newLeaveRequestId = newLeaveRequestId
                    });
                    return true; // Notification successfully attempted
                }
                catch (Exception ex)
                {
                    // Log the exception if SignalR sending fails
                    Console.WriteLine($"Error sending SignalR notification to owner for LeaveRequest {newLeaveRequestId}: {ex.Message}");
                    // In a real application, use a proper logging framework (e.g., ILogger)
                    return false; // Notification failed due to an exception
                }
            }
            else
            {
                Console.WriteLine("Warning: No user with Role.Owner found to send SignalR update for new pending request.");
                return false; // No owner found, so notification couldn't be sent
            }
        }
        public async Task<bool> ErrorNotification(string userId,string errorMessage, string? details = null)
        {

            try
            {
                var ownerClient = _hubContext.Clients.User(userId);

                // You can define a specific SignalR method for errors, e.g., "ReceiveError"
                await ownerClient.SendAsync("ReceiveError", new
                {
                    message = errorMessage,
                });
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"CRITICAL ERROR: Failed to send error notification. Original error: '{errorMessage}'. Notification error: {ex.Message}");
                return false;
            }
            
        }
    }

}
