
namespace AutomatedTaskSystem.Services.Notification
{
    public interface INotificationService
    {
        Task<bool> ErrorNotification(string userId, string errorMessage, string? details = null);
        Task<bool> NotifyOwnerOfNewPendingLeaveRequest(int newLeaveRequestId);
    }
}