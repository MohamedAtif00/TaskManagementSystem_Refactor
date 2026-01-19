
namespace AutomatedTaskSystem.Services.Notification
{
    public interface INotificationService
    {
	        Task<bool> ErrorNotification(string userId, string errorMessage, string? details = null);
	        Task<bool> NotifyOwnerOfNewPendingLeaveRequest(int newLeaveRequestId);
	        Task<bool> NotifyUserOfTaskAssignment(int assignedUserId, int taskId, int? assignedByUserId = null);
	        Task<bool> NotifyOwnerOfProjectClosed(int projectId, bool closedManually);
	        Task<bool> NotifyOwnerOfProjectCompleted(int projectId);
    }
}