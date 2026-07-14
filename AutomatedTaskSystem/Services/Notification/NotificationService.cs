using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Helper;
	using AutomatedTaskSystem.Hub;
	using AutomatedTaskSystem.Models;
	using AutomatedTaskSystem.Models.Enums.NotificationCategory;
	using AutomatedTaskSystem.Models.Enums.NotificationStatus;
	using AutomatedTaskSystem.Models.Enums.NotificationType;
	using AutomatedTaskSystem.Models.Enums.ProjectStatus;
	using AutomatedTaskSystem.Models.Enums.TaskStatus;
	using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.Log;
using Microsoft.AspNetCore.SignalR;
	using NotificationModel = AutomatedTaskSystem.Models.Notification;

	namespace AutomatedTaskSystem.Services.Notification
	{
	    public class NotificationService : INotificationService
	    {
	        private readonly IHubContext<UserHub> _hubContext;
	        private readonly DataContext _dataContext;
		private readonly ILogService _logService;

        public NotificationService(IHubContext<UserHub> hubContext, DataContext dataContext, ILogService logService)
        {
            _hubContext = hubContext;
            _dataContext = dataContext;
            _logService = logService;
        }

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

	                    var leaveRequest = await _dataContext.LeaveRequests
	                        .Include(lr => lr.User)
	                        .FirstOrDefaultAsync(lr => lr.Id == newLeaveRequestId);

	                    var title = "New Leave Request";
	                    var message = leaveRequest is not null
	                        ? $"{leaveRequest.User.Name} submitted a {leaveRequest.Type} leave request."
	                        : $"A new leave request (ID: {newLeaveRequestId}) is pending approval.";

	                    await CreateNotification(
	                        owner.Id,
	                        title,
	                        message,
	                        NotificationCategoryEnum.Leaves,
	                        NotificationTypeEnum.Leave,
	                        relatedEntityId: newLeaveRequestId,
	                        hasActions: true,
	                        status: NotificationStatusEnum.Pending);

	                    return true;
	                }
	                catch (Exception ex)
	                {
	                    Console.WriteLine($"Error sending SignalR notification to owner for LeaveRequest {newLeaveRequestId}: {ex.Message}");
	                    return false;
	                }
	            }

	            Console.WriteLine("Warning: No user with Role.Owner found to send SignalR update for new pending request.");
	            return false;
	        }
	
			public async Task<bool> ErrorNotification(string userId, string errorMessage, string? details = null)
			{
				// Always attempt to send the real-time SignalR error notification first
				var signalRSuccess = false;
				try
				{
					var ownerClient = _hubContext.Clients.User(userId);
					await ownerClient.SendAsync("ReceiveError", new
					{
						message = errorMessage,
					});
					signalRSuccess = true;
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine($"CRITICAL ERROR: Failed to send error notification. Original error: '{errorMessage}'. Notification error: {ex.Message}");
					return false;
				}

				// Best-effort persistence: do not let DB issues affect the real-time notification
				if (int.TryParse(userId, out var parsedUserId))
				{
					try
					{
						var composedMessage = string.IsNullOrWhiteSpace(details)
							? errorMessage
							: $"{errorMessage} Details: {details}";

						await CreateNotification(
							parsedUserId,
							"System Error",
							composedMessage,
							NotificationCategoryEnum.General,
							NotificationTypeEnum.System,
							relatedEntityId: null,
							hasActions: false,
							status: null);
					}
					catch (Exception ex)
					{
						Console.Error.WriteLine($"ERROR: Failed to persist error notification for user '{userId}'. Original error: '{errorMessage}'. Persistence error: {ex.Message}");
						// Swallow to avoid impacting the already-sent SignalR message
					}
				}
				else
				{
					Console.Error.WriteLine($"Warning: Could not parse userId '{userId}' when trying to persist error notification.");
				}

				return signalRSuccess;
			}
	
	        public async Task<bool> NotifyUserOfTaskAssignment(int assignedUserId, int taskId, int? assignedByUserId = null)
	        {
	            try
	            {
	                var task = await _dataContext.Tasks
	                    .Where(t => !t.Archived && t.Id == taskId)
	                    .Include(t => t.LearningObjective)
	                        .ThenInclude(lo => lo.Lesson)
	                            .ThenInclude(l => l.Unit)
	                                .ThenInclude(u => u.Subject)
	                    .FirstOrDefaultAsync();

	                if (task is null)
	                {
	                    Console.WriteLine($"Warning: Task with id {taskId} not found when trying to notify user {assignedUserId} about assignment.");
	                    return false;
	                }

	                User? assignedByUser = null;
	                if (assignedByUserId.HasValue)
	                {
	                    assignedByUser = await _dataContext.Users
	                        .FirstOrDefaultAsync(u => u.Id == assignedByUserId.Value);
	                }

	                var client = _hubContext.Clients.User(assignedUserId.ToString());

	                await client.SendAsync("TaskAssigned", new
	                {
	                    taskId = task.Id,
	                    taskName = task.Name,
	                    subjectId = task.LearningObjective.Lesson.Unit.Subject.Id,
	                    projectName = task.LearningObjective.Lesson.Unit.Subject.Name,
	                    learningObjectiveId = task.LearningObjective.Id,
	                    learningObjectiveName = task.LearningObjective.Name,
	                    assignedByUserId = assignedByUser?.Id,
	                    assignedByUserName = assignedByUser?.Name
	                });

	                var subject = task.LearningObjective.Lesson.Unit.Subject;
	                var assignedByName = assignedByUser?.Name ?? "System";
	                var title = "New Task Assigned";
	                var message = $"Task '{task.Name}' in subject '{subject.Name}' has been assigned to you by {assignedByName}.";

	                // Create additional data JSON with subjectId for proper routing
	                var additionalData = System.Text.Json.JsonSerializer.Serialize(new { subjectId = subject.Id });

	                await CreateNotification(
	                    assignedUserId,
	                    title,
	                    message,
	                    NotificationCategoryEnum.WorkUpdates,
	                    NotificationTypeEnum.Task,
	                    relatedEntityId: task.Id,
	                    hasActions: false,
	                    status: null,
	                    additionalData: additionalData);

	                return true;
	            }
	            catch (Exception ex)
	            {
	                Console.WriteLine($"Error sending TaskAssigned notification for task {taskId} to user {assignedUserId}: {ex.Message}");
	                return false;
	            }
	        }

	        public async Task<bool> NotifyTeamLeaderOfFlaggedTask(int teamLeaderId, int taskId, int flaggedByUserId, string comment)
	        {
	            try
	            {
	                var task = await _dataContext.Tasks
	                    .Where(t => !t.Archived && t.Id == taskId)
	                    .Include(t => t.LearningObjective)
	                        .ThenInclude(lo => lo.Lesson)
	                            .ThenInclude(l => l.Unit)
	                                .ThenInclude(u => u.Subject)
	                    .FirstOrDefaultAsync();

	                if (task is null)
	                {
	                    Console.WriteLine($"Warning: Task with id {taskId} not found when trying to notify team leader {teamLeaderId} about flagged task.");
	                    return false;
	                }

	                var flaggedByUser = await _dataContext.Users
	                    .FirstOrDefaultAsync(u => u.Id == flaggedByUserId);

	                var teamLeader = await _dataContext.Users
	                    .FirstOrDefaultAsync(u => u.Id == teamLeaderId && !u.Archived);

	                var canView = teamLeader is not null && await CanUserViewTask(teamLeader, task);

	                var client = _hubContext.Clients.User(teamLeaderId.ToString());
	                var subject = task.LearningObjective.Lesson.Unit.Subject;
	                var learningObjectiveName = task.LearningObjective.Name;
	                var flaggedByName = flaggedByUser?.Name ?? "A user";

	                await client.SendAsync("TaskFlagged", new
	                {
	                    taskId = task.Id,
	                    taskName = task.Name,
	                    learningObjectiveName,
	                    projectId = subject.Id,
	                    projectName = subject.Name,
	                    flaggedByUserId = flaggedByUser?.Id,
	                    flaggedByUserName = flaggedByName,
	                    comment,
	                    canView
	                });

	                var title = "Task Flagged";
	                var message = $"{flaggedByName} flagged task '{task.Name}' in LO '{learningObjectiveName}' (project '{subject.Name}'). Comment: {comment}";
	                var additionalData = System.Text.Json.JsonSerializer.Serialize(new
	                {
	                    projectId = subject.Id,
	                    subjectId = subject.Id,
	                    kind = "flagged",
	                    taskName = task.Name,
	                    learningObjectiveName,
	                    projectName = subject.Name,
	                    flaggedByUserName = flaggedByName,
	                    comment,
	                    taskGroupId = task.GroupId,
	                    canView
	                });

	                await CreateNotification(
	                    teamLeaderId,
	                    title,
	                    message,
	                    NotificationCategoryEnum.WorkUpdates,
	                    NotificationTypeEnum.Task,
	                    relatedEntityId: task.Id,
	                    hasActions: false,
	                    status: null,
	                    additionalData: additionalData);

	                return true;
	            }
	            catch (Exception ex)
	            {
	                Console.WriteLine($"Error sending TaskFlagged notification for task {taskId} to team leader {teamLeaderId}: {ex.Message}");
	                return false;
	            }
	        }

		    public async Task<bool> NotifyUserOfProjectAssignment(int assignedUserId, int subjectId, int? assignedByUserId = null)
		    {
		        try
		        {
		            var project = await _dataContext.Subjects
		                .Where(p => !p.Archived && p.Id == subjectId)
		                .FirstOrDefaultAsync();

		            if (project is null)
		            {
		                Console.WriteLine($"Warning: Project with id {subjectId} not found when trying to notify user {assignedUserId} about project assignment.");
		                return false;
		            }

		            User? assignedByUser = null;
		            if (assignedByUserId.HasValue)
		            {
		                assignedByUser = await _dataContext.Users
		                    .FirstOrDefaultAsync(u => u.Id == assignedByUserId.Value);
		            }

		            var client = _hubContext.Clients.User(assignedUserId.ToString());

		            await client.SendAsync("ProjectAssigned", new
		            {
		                subjectId = project.Id,
		                projectName = project.Name,
		                description = project.Description,
		                assignedByUserId = assignedByUser?.Id,
		                assignedByUserName = assignedByUser?.Name
		            });

		            var assignedByName = assignedByUser?.Name ?? "System";
		            var title = "Assigned to Project";
		            var message = $"You have been assigned to project '{project.Name}' (ID: {project.Id}) by {assignedByName}.";

		            await CreateNotification(
		                assignedUserId,
		                title,
		                message,
		                NotificationCategoryEnum.WorkUpdates,
		                NotificationTypeEnum.Task,
		                relatedEntityId: project.Id,
		                hasActions: false,
		                status: null);

		            return true;
		        }
		        catch (Exception ex)
		        {
				_logService.LogError(ex,message:$"this error happen in NotifyUserOfProjectAssignment");
		            Console.WriteLine($"Error sending ProjectAssigned notification for project {subjectId} to user {assignedUserId}: {ex.Message}");
		            return false;
		        }
		    }
	
	        public async Task<bool> NotifyOwnerOfProjectClosed(int subjectId, bool closedManually)
	        {
	            var owner = await _dataContext.Users.FirstOrDefaultAsync(x => x.Role == UserRoleEnum.Owner);
	            if (owner is null)
	            {
	                Console.WriteLine("Warning: No user with Role.Owner found to send ProjectClosed notification.");
	                return false;
	            }

	            var project = await _dataContext.Subjects
	                .Where(p => !p.Archived && p.Id == subjectId)
	                .FirstOrDefaultAsync();

	            if (project is null)
	            {
	                Console.WriteLine($"Warning: Project with id {subjectId} not found when trying to send ProjectClosed notification.");
	                return false;
	            }

	            try
	            {
	                var ownerClient = _hubContext.Clients.User(owner.Id.ToString());

	                await ownerClient.SendAsync("ProjectClosed", new
	                {
	                    subjectId = project.Id,
	                    projectName = project.Name,
	                    description = project.Description,
	                    folderId = project.FolderId,
	                    status = project.Status.ToString(),
	                    closedManually
	                });

	                var title = "Project Closed";
	                var message = closedManually
	                    ? $"Project '{project.Name}' has been manually closed."
	                    : $"Project '{project.Name}' has been closed automatically.";

	                await CreateNotification(
	                    owner.Id,
	                    title,
	                    message,
	                    NotificationCategoryEnum.WorkUpdates,
	                    NotificationTypeEnum.Project,
	                    relatedEntityId: project.Id,
	                    hasActions: false,
	                    status: null);

	                return true;
	            }
	            catch (Exception ex)
	            {
	                Console.WriteLine($"Error sending ProjectClosed notification for project {subjectId}: {ex.Message}");
	                return false;
	            }
	        }
	
	        public async Task<bool> NotifyOwnerOfProjectCompleted(int subjectId)
	        {
	            var owner = await _dataContext.Users.FirstOrDefaultAsync(x => x.Role == UserRoleEnum.Owner);
	            if (owner is null)
	            {
	                Console.WriteLine("Warning: No user with Role.Owner found to send ProjectCompleted notification.");
	                return false;
	            }

	            var project = await _dataContext.Subjects
	                .Where(p => !p.Archived && p.Id == subjectId)
	                .Include(p => p.Units)
	                    .ThenInclude(u => u.Lessons)
	                        .ThenInclude(l => l.LearningObjectives)
	                            .ThenInclude(lo => lo.Tasks)
	                .FirstOrDefaultAsync();

	            if (project is null)
	            {
	                Console.WriteLine($"Warning: Project with id {subjectId} not found when trying to send ProjectCompleted notification.");
	                return false;
	            }

	            try
	            {
	                var allTasks = project.Units
	                    .SelectMany(u => u.Lessons)
	                    .SelectMany(l => l.LearningObjectives)
	                    .SelectMany(lo => lo.Tasks)
	                    .Where(t => !t.Archived)
	                    .ToList();

	                var totalTasks = allTasks.Count;
	                var completedTasks = allTasks.Count(t => t.Status == TaskStatusEnum.Done);
	                var remainingTasks = totalTasks - completedTasks;
	
	                var ownerClient = _hubContext.Clients.User(owner.Id.ToString());

	                await ownerClient.SendAsync("ProjectCompleted", new
	                {
	                    subjectId = project.Id,
	                    projectName = project.Name,
	                    description = project.Description,
	                    folderId = project.FolderId,
	                    status = project.Status.ToString(),
	                    totalTasks,
	                    completedTasks,
	                    remainingTasks
	                });

	                var title = "Project Completed";
	                var message = $"Project '{project.Name}' has been completed. Total tasks: {totalTasks}, completed: {completedTasks}, remaining: {remainingTasks}.";

	                await CreateNotification(
	                    owner.Id,
	                    title,
	                    message,
	                    NotificationCategoryEnum.WorkUpdates,
	                    NotificationTypeEnum.Project,
	                    relatedEntityId: project.Id,
	                    hasActions: false,
	                    status: null);

	                return true;
	            }
	            catch (Exception ex)
	            {
	                Console.WriteLine($"Error sending ProjectCompleted notification for project {subjectId}: {ex.Message}");
	                return false;
	            }
	        }

	        public async Task<NotificationModel?> CreateNotification(int userId, string title, string message, NotificationCategoryEnum category, NotificationTypeEnum type, int? relatedEntityId = null, bool hasActions = false, NotificationStatusEnum? status = null, string? additionalData = null)
	        {
	            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
	            if (user is null)
	            {
	                Console.WriteLine($"Warning: Cannot create notification. User with id {userId} not found.");
	                return null;
	            }

	            var notification = new NotificationModel
	            {
	                UserId = user.Id,
	                User = user,
	                Title = title,
	                Message = message,
	                Category = category,
	                Type = type,
	                RelatedEntityId = relatedEntityId,
	                HasActions = hasActions,
	                Status = status,
	                AdditionalData = additionalData,
	                CreatedAt = DateTime.Now,
	                IsRead = false
	            };

	            _dataContext.Notifications.Add(notification);
	            await _dataContext.SaveChangesAsync();

	            return notification;
	        }

	        private IQueryable<NotificationModel> BuildUserNotificationsQuery(
	            int userId,
	            NotificationCategoryEnum? category = null,
	            NotificationTimeRange? timeFilter = null,
	            bool? isRead = null,
	            NotificationTypeEnum? type = null,
	            bool? flaggedOnly = null)
	        {
	            var query = _dataContext.Notifications
	                .Where(n => n.UserId == userId);

	            if (category.HasValue)
	            {
	                query = query.Where(n => n.Category == category.Value);
	            }

	            if (type.HasValue)
	            {
	                query = query.Where(n => n.Type == type.Value);
	            }

	            if (flaggedOnly == true)
	            {
	                query = query.Where(n =>
	                    n.Title == "Task Flagged" ||
	                    (n.AdditionalData != null && n.AdditionalData.Contains("\"kind\":\"flagged\"")));
	            }

	            if (isRead.HasValue)
	            {
	                query = query.Where(n => n.IsRead == isRead.Value);
	            }

	            if (timeFilter.HasValue && timeFilter.Value != NotificationTimeRange.AllTime)
	            {
	                var now = DateTime.Now;
	                DateTime from = timeFilter.Value switch
	                {
	                    NotificationTimeRange.Last7Days => now.AddDays(-7),
	                    NotificationTimeRange.Last30Days => now.AddDays(-30),
	                    NotificationTimeRange.Last90Days => now.AddDays(-90),
	                    _ => now.AddYears(-10)
	                };

	                query = query.Where(n => n.CreatedAt >= from);
	            }

	            return query.OrderByDescending(n => n.CreatedAt);
	        }

	        public Task<List<NotificationModel>> GetUserNotifications(
	            int userId,
	            NotificationCategoryEnum? category = null,
	            NotificationTimeRange? timeFilter = null,
	            bool? isRead = null,
	            NotificationTypeEnum? type = null,
	            bool? flaggedOnly = null)
	        {
	            return BuildUserNotificationsQuery(userId, category, timeFilter, isRead, type, flaggedOnly).ToListAsync();
	        }

	        public Task<PageList<NotificationModel>> GetUserNotificationsPaged(
	            int userId,
	            int page,
	            int pageSize,
	            NotificationCategoryEnum? category = null,
	            NotificationTimeRange? timeFilter = null,
	            bool? isRead = null,
	            NotificationTypeEnum? type = null,
	            bool? flaggedOnly = null)
	        {
	            return PageList<NotificationModel>.CreateAsync(
	                BuildUserNotificationsQuery(userId, category, timeFilter, isRead, type, flaggedOnly),
	                page,
	                pageSize);
	        }

	        public Task<int> GetUnreadNotificationCount(int userId)
	        {
	            return _dataContext.Notifications
	                .CountAsync(n => n.UserId == userId && !n.IsRead);
	        }

	        public async Task<bool> MarkAsRead(int notificationId, int userId,bool? accepted = null)
	        {
	            var notification = await _dataContext.Notifications
	                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

	            if (notification is null)
	            {
	                Console.WriteLine($"Warning: Notification with id {notificationId} not found for user {userId}.");
	                return false;
	            }

	            if (notification.IsRead)
	            {
	                return true;
	            }

            // 1. Handle Decision Logic
            // If the notification expects a response (Status is currently 'Pending')
            if (notification.Status != null && accepted.HasValue)
            {
                notification.Status = accepted.Value ? NotificationStatusEnum.Accepted:	NotificationStatusEnum.Declined;
                //notification.ActionDate = DateTime.UtcNow;
            }

            notification.IsRead = true;
	            await _dataContext.SaveChangesAsync();
	            return true;
	        }

	        public async Task<int> MarkAllAsRead(int userId)
	        {
	            var unreadNotifications = await _dataContext.Notifications
	                .Where(n => n.UserId == userId && !n.IsRead)
	                .ToListAsync();

	            if (unreadNotifications.Count == 0)
	                return 0;

	            foreach (var notification in unreadNotifications)
	                notification.IsRead = true;

	            await _dataContext.SaveChangesAsync();
	            return unreadNotifications.Count;
	        }

	        public async Task<bool> UpdateNotificationStatus(int notificationId, NotificationStatusEnum status)
	        {
	            var notification = await _dataContext.Notifications
	                .FirstOrDefaultAsync(n => n.Id == notificationId);

	            if (notification is null)
	            {
	                Console.WriteLine($"Warning: Notification with id {notificationId} not found when updating status.");
	                return false;
	            }

		            notification.Status = status;
		            if (status != NotificationStatusEnum.Pending)
		            {
		                notification.HasActions = false;
		            }
		            await _dataContext.SaveChangesAsync();
	            return true;
	        }

	        private async Task<bool> CanUserViewTask(User user, Models.Task task)
	        {
	            if (task.Status == TaskStatusEnum.Done || task.Status == TaskStatusEnum.Rollback)
	                return false;

	            if (user.Role == UserRoleEnum.ProjectManger || user.Role == UserRoleEnum.Owner)
	                return true;

	            if (user.Role == UserRoleEnum.TeamLeader)
	                return user.GroupId == task.GroupId;

	            if (user.Role == UserRoleEnum.SectionHead)
	            {
	                if (user.GroupId == task.GroupId)
	                    return true;

	                return await _dataContext.SectionGroups.AnyAsync(sg =>
	                    sg.Section.HeadId == user.Id && sg.GroupId == task.GroupId);
	            }

	            return false;
	        }
	    }
	}
