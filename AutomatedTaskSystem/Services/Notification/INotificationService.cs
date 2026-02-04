using AutomatedTaskSystem.Dtos.NotificationDtos;
using AutomatedTaskSystem.Models.Enums.NotificationCategory;
using AutomatedTaskSystem.Models.Enums.NotificationStatus;
using AutomatedTaskSystem.Models.Enums.NotificationType;
using NotificationModel = AutomatedTaskSystem.Models.Notification;

namespace AutomatedTaskSystem.Services.Notification
{
	public enum NotificationTimeRange
	{
		Last7Days,
		Last30Days,
		Last90Days,
		AllTime
	}

	public interface INotificationService
	{
		Task<bool> ErrorNotification(string userId, string errorMessage, string? details = null);
		Task<bool> NotifyOwnerOfNewPendingLeaveRequest(int newLeaveRequestId);
		Task<bool> NotifyUserOfTaskAssignment(int assignedUserId, int taskId, int? assignedByUserId = null);
			Task<bool> NotifyUserOfProjectAssignment(int assignedUserId, int projectId, int? assignedByUserId = null);
		Task<bool> NotifyOwnerOfProjectClosed(int projectId, bool closedManually);
		Task<bool> NotifyOwnerOfProjectCompleted(int projectId);

		Task<NotificationModel?> CreateNotification(
			int userId,
			string title,
			string message,
			NotificationCategoryEnum category,
			NotificationTypeEnum type,
			int? relatedEntityId = null,
			bool hasActions = false,
			NotificationStatusEnum? status = null,
			string? additionalData = null);

		Task<List<NotificationModel>> GetUserNotifications(
			int userId,
			NotificationCategoryEnum? category = null,
			NotificationTimeRange? timeFilter = null,
			bool? isRead = null);

		Task<bool> MarkAsRead(int notificationId, int userId, bool? accepted = null);
		Task<bool> UpdateNotificationStatus(int notificationId, NotificationStatusEnum status);
        Task<bool> NotifyMemberOfRollBack(RollBackNotificationDto rollBackNotificationDto, bool critical = false);

        /// <summary>
        /// Notifies team leader(s) of a group that a task has been moved to backlog and needs assignment.
        /// Used when a task enters backlog status without an assigned user.
        /// </summary>
        /// <param name="taskId">The ID of the task that moved to backlog</param>
        /// <param name="groupId">The group ID to find team leaders for</param>
        /// <param name="reason">The reason/context for the backlog status (e.g., "Task unassigned", "New workflow task created")</param>
        /// <returns>True if notification was sent successfully</returns>
        Task<bool> NotifyTeamLeaderOfBacklogTask(int taskId, int groupId, string reason);

        /// <summary>
        /// Notifies an assigned user that their task has been moved to backlog status.
        /// Used when a task with an assigned user is moved back to backlog.
        /// </summary>
        /// <param name="userId">The ID of the assigned user to notify</param>
        /// <param name="taskId">The ID of the task that moved to backlog</param>
        /// <param name="reason">The reason/context for the backlog status</param>
        /// <returns>True if notification was sent successfully</returns>
        Task<bool> NotifyUserOfBacklogTask(int userId, int taskId, string reason);
    }
}