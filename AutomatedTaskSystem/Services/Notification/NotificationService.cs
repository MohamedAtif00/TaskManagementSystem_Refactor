	using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.NotificationDtos;
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
				if (assignedUserId == assignedByUserId) return true;

					var task = await _dataContext.Tasks
						.Where(t => !t.Archived && t.Id == taskId)
						.Include(t => t.LearningObjective)
							.ThenInclude(lo => lo.Lesson)
								.ThenInclude(l => l.Unit)
									.ThenInclude(u => u.Project)
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
						projectId = task.LearningObjective.Lesson.Unit.Project.Id,
						projectName = task.LearningObjective.Lesson.Unit.Project.Name,
						learningObjectiveId = task.LearningObjective.Id,
						learningObjectiveName = task.LearningObjective.Name,
						assignedByUserId = assignedByUser?.Id,
						assignedByUserName = assignedByUser?.Name
					});

					var project = task.LearningObjective.Lesson.Unit.Project;
					var assignedByName = assignedByUser?.Name ?? "System";
					var title = "New Task Assigned";
					var message = $"Task '{task.Name}' in project '{project.Name}' has been assigned to you by {assignedByName}.";

					// Create additional data JSON with projectId for proper routing
					var additionalData = System.Text.Json.JsonSerializer.Serialize(new { projectId = project.Id });

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

			public async Task<bool> NotifyUserOfProjectAssignment(int assignedUserId, int projectId, int? assignedByUserId = null)
			{
				try
				{
					var project = await _dataContext.Projects
						.Where(p => !p.Archived && p.Id == projectId)
						.FirstOrDefaultAsync();

					if (project is null)
					{
						Console.WriteLine($"Warning: Project with id {projectId} not found when trying to notify user {assignedUserId} about project assignment.");
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
						projectId = project.Id,
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
					Console.WriteLine($"Error sending ProjectAssigned notification for project {projectId} to user {assignedUserId}: {ex.Message}");
					return false;
				}
			}
	
			public async Task<bool> NotifyOwnerOfProjectClosed(int projectId, bool closedManually)
			{
				var owner = await _dataContext.Users.FirstOrDefaultAsync(x => x.Role == UserRoleEnum.Owner);
				if (owner is null)
				{
					Console.WriteLine("Warning: No user with Role.Owner found to send ProjectClosed notification.");
					return false;
				}

				var project = await _dataContext.Projects
					.Where(p => !p.Archived && p.Id == projectId)
					.Include(p => p.Year)
					.FirstOrDefaultAsync();

				if (project is null)
				{
					Console.WriteLine($"Warning: Project with id {projectId} not found when trying to send ProjectClosed notification.");
					return false;
				}

				try
				{
					var ownerClient = _hubContext.Clients.User(owner.Id.ToString());

					await ownerClient.SendAsync("ProjectClosed", new
					{
						projectId = project.Id,
						projectName = project.Name,
						description = project.Description,
						yearId = project.YearId,
						yearName = project.Year?.Number,
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
					Console.WriteLine($"Error sending ProjectClosed notification for project {projectId}: {ex.Message}");
					return false;
				}
			}
	
			public async Task<bool> NotifyOwnerOfProjectCompleted(int projectId)
			{
				var owner = await _dataContext.Users.FirstOrDefaultAsync(x => x.Role == UserRoleEnum.Owner);
				if (owner is null)
				{
					Console.WriteLine("Warning: No user with Role.Owner found to send ProjectCompleted notification.");
					return false;
				}

				var project = await _dataContext.Projects
					.Where(p => !p.Archived && p.Id == projectId)
					.Include(p => p.Year)
					.Include(p => p.Units)
						.ThenInclude(u => u.Lessons)
							.ThenInclude(l => l.LearningObjectives)
								.ThenInclude(lo => lo.Tasks)
					.FirstOrDefaultAsync();

				if (project is null)
				{
					Console.WriteLine($"Warning: Project with id {projectId} not found when trying to send ProjectCompleted notification.");
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
						projectId = project.Id,
						projectName = project.Name,
						description = project.Description,
						yearId = project.YearId,
						yearName = project.Year?.Number,
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
					Console.WriteLine($"Error sending ProjectCompleted notification for project {projectId}: {ex.Message}");
					return false;
				}
			}

			public async Task<bool> NotifyMemberOfRollBack(RollBackNotificationDto rollBackNotificationDto, bool critical = false)
			{
				try
				{
					// Fetch the system owner
					var owner = await _dataContext.Users.FirstOrDefaultAsync(x => x.Role == UserRoleEnum.Owner);
					if (owner is null)
					{
						Console.WriteLine("Warning: No user with Role.Owner found to send Rollback notification.");
						return false;
					}

					// Build the list of unique user IDs to notify: ToUser, TeamLeader (if exists), and Owner
					var userIdsToNotify = new HashSet<int> { rollBackNotificationDto.ToUserId, owner.Id };
					if (rollBackNotificationDto.TeamLeaderId > 0)
					{
						userIdsToNotify.Add(rollBackNotificationDto.TeamLeaderId);
					}

					// Remove any invalid user IDs (0 or negative)
					userIdsToNotify.RemoveWhere(id => id <= 0);

					var userIdStrings = userIdsToNotify.Select(id => id.ToString()).ToList();
					var clients = _hubContext.Clients.Users(userIdStrings);

					// Send SignalR notification to all recipients with critical flag and rollbackCount
					await clients.SendAsync("NormalRollback", new
					{
						fromUserId = rollBackNotificationDto.FromUserId,
						fromUserName = rollBackNotificationDto.FromUserName,
						toUserId = rollBackNotificationDto.ToUserId,
						toUserName = rollBackNotificationDto.ToUserName,
						fromTaskId = rollBackNotificationDto.FromTaskId,
						fromTaskName = rollBackNotificationDto.FromTaskName,
						toTaskId = rollBackNotificationDto.ToTaskId,
						toTaskName = rollBackNotificationDto.ToTaskName,
						loName = rollBackNotificationDto.LoName,
						projectId = rollBackNotificationDto.ProjectId,
						projectName = rollBackNotificationDto.ProjectName,
						sprintName = rollBackNotificationDto.SprintName,
						critical = critical,
						rollbackCount = rollBackNotificationDto.RollbackCount
					});

					// Create title and message based on whether this is a critical rollback
					var title = critical ? "Task Rolled Back Critically" : "Task Rolled Back";
					var message = critical
						? $"{rollBackNotificationDto.FromUserName} rolled back {rollBackNotificationDto.FromTaskName} for Learning Objective {rollBackNotificationDto.LoName} to {rollBackNotificationDto.ToUserName} ({rollBackNotificationDto.ToTaskName}) from Sprint {rollBackNotificationDto.SprintName} after multiple rollbacks."
						: $"{rollBackNotificationDto.FromUserName} rolled back ({rollBackNotificationDto.FromTaskName}) for Learning Objective {rollBackNotificationDto.LoName} to {rollBackNotificationDto.ToUserName} ({rollBackNotificationDto.ToTaskName}) from Sprint {rollBackNotificationDto.SprintName}.";

					// Create comprehensive additional data JSON with all rollback metadata
					// Include icon SVG for critical rollbacks
					var criticalIcon = @"<svg width=""13"" height=""12"" viewBox=""0 0 13 12"" fill=""none"" xmlns=""http://www.w3.org/2000/svg""><path d=""M6.30407 8.22394H6.30941M6.30407 4.22394V6.22394M5.35407 1.03527L0.648739 8.8906C0.551792 9.05863 0.500514 9.2491 0.500004 9.44309C0.499494 9.63707 0.54977 9.82781 0.645832 9.99635C0.741895 10.1649 0.880398 10.3053 1.04757 10.4037C1.21475 10.5021 1.40477 10.5551 1.59874 10.5573H11.0094C11.2035 10.5553 11.3936 10.5025 11.5609 10.4041C11.7282 10.3057 11.8668 10.1653 11.9629 9.99667C12.059 9.82807 12.1092 9.63724 12.1086 9.44317C12.108 9.24911 12.0566 9.05859 11.9594 8.8906L7.25474 1.03527C7.15578 0.87189 7.01636 0.736792 6.84995 0.643027C6.68353 0.549262 6.49575 0.5 6.30474 0.5C6.11373 0.5 5.92594 0.549262 5.75953 0.643027C5.59312 0.736792 5.4537 0.87189 5.35474 1.03527"" stroke=""#DC2626"" stroke-miterlimit=""10"" stroke-linecap=""round"" stroke-linejoin=""round""/></svg>";

					var additionalData = System.Text.Json.JsonSerializer.Serialize(new
					{
						projectId = rollBackNotificationDto.ProjectId,
						taskId = rollBackNotificationDto.FromTaskId,
						toTaskId = rollBackNotificationDto.ToTaskId,
						critical = critical,
						rollbackCount = rollBackNotificationDto.RollbackCount,
						sprintName = rollBackNotificationDto.SprintName,
						icon = critical ? criticalIcon : null
					});

					// Create persistent notifications for all recipients
					foreach (var userId in userIdsToNotify)
					{
						await CreateNotification(
							userId,
							title,
							message,
							NotificationCategoryEnum.WorkUpdates,
							NotificationTypeEnum.Task,
							relatedEntityId: rollBackNotificationDto.ToTaskId,
							hasActions: false,
							status: null,
							additionalData: additionalData);
					}

					return true;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error sending Rollback notification: {ex.Message}");
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

	        public async Task<List<NotificationModel>> GetUserNotifications(int userId, NotificationCategoryEnum? category = null, NotificationTimeRange? timeFilter = null, bool? isRead = null)
	        {
	            var query = _dataContext.Notifications
	                .Where(n => n.UserId == userId);

	            if (category.HasValue)
	            {
	                query = query.Where(n => n.Category == category.Value);
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

	            return await query
	                .OrderByDescending(n => n.CreatedAt)
	                .ToListAsync();
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

        public async Task<bool> NotifyTeamLeaderOfBacklogTask(int taskId, int groupId, string reason)
        {
            try
            {
                var task = await _dataContext.Tasks
                    .Where(t => !t.Archived && t.Id == taskId)
                    .Include(t => t.Group)
                    .Include(t => t.LearningObjective)
                        .ThenInclude(lo => lo.Lesson)
                            .ThenInclude(l => l.Unit)
                                .ThenInclude(u => u.Project)
                    .FirstOrDefaultAsync();

                if (task is null)
                {
                    Console.WriteLine($"Warning: Task with id {taskId} not found when trying to notify team leaders about backlog status.");
                    return false;
                }

                // Find all team leaders for the task's group
                var teamLeaders = await _dataContext.Users
                    .Where(u => u.Role == UserRoleEnum.TeamLeader && !u.Archived && u.GroupId == groupId)
                    .ToListAsync();

                if (!teamLeaders.Any())
                {
                    Console.WriteLine($"Warning: No team leaders found for group {groupId} to notify about backlog task {taskId}.");
                    return false;
                }

                var project = task.LearningObjective.Lesson.Unit.Project;
                var title = "Task Added to Backlog";
                var message = $"Task '{task.Name}' in project '{project.Name}' has been added to backlog.";

                // Create additional data JSON with projectId for proper routing
                var additionalData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    projectId = project.Id,
                    groupId = groupId,
                    reason = reason
                });

                // Send SignalR notification to all team leaders
                var teamLeaderIds = teamLeaders.Select(tl => tl.Id.ToString()).ToList();
                var clients = _hubContext.Clients.Users(teamLeaderIds);

                await clients.SendAsync("TaskAddedToBacklog", new
                {
                    taskId = task.Id,
                    taskName = task.Name,
                    projectId = project.Id,
                    projectName = project.Name,
                    groupId = groupId,
                    groupName = task.Group?.Name ?? "Unknown",
                    learningObjectiveId = task.LearningObjective.Id,
                    learningObjectiveName = task.LearningObjective.Name,
                    reason = reason
                });

                // Create persistent notifications for all team leaders
                foreach (var teamLeader in teamLeaders)
                {
                    await CreateNotification(
                        teamLeader.Id,
                        title,
                        message,
                        NotificationCategoryEnum.WorkUpdates,
                        NotificationTypeEnum.Task,
                        relatedEntityId: task.Id,
                        hasActions: false,
                        status: null,
                        additionalData: additionalData);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending backlog notification for task {taskId} to team leaders of group {groupId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> NotifyUserOfBacklogTask(int userId, int taskId, string reason)
        {
            try
            {
                var task = await _dataContext.Tasks
                    .Where(t => !t.Archived && t.Id == taskId)
                    .Include(t => t.LearningObjective)
                        .ThenInclude(lo => lo.Lesson)
                            .ThenInclude(l => l.Unit)
                                .ThenInclude(u => u.Project)
                    .FirstOrDefaultAsync();

                if (task is null)
                {
                    Console.WriteLine($"Warning: Task with id {taskId} not found when trying to notify user {userId} about backlog status.");
                    return false;
                }

                var project = task.LearningObjective.Lesson.Unit.Project;
                var title = "Task Moved to Backlog";
                var message = $"Task '{task.Name}' in project '{project.Name}' has been moved to backlog.";

                // Create additional data JSON with projectId for proper routing
                var additionalData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    projectId = project.Id,
                    reason = reason
                });

                // Send SignalR notification to the user
                var client = _hubContext.Clients.User(userId.ToString());

                await client.SendAsync("TaskMovedToBacklog", new
                {
                    taskId = task.Id,
                    taskName = task.Name,
                    projectId = project.Id,
                    projectName = project.Name,
                    learningObjectiveId = task.LearningObjective.Id,
                    learningObjectiveName = task.LearningObjective.Name,
                    reason = reason
                });

                // Create persistent notification
                await CreateNotification(
                    userId,
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
                Console.WriteLine($"Error sending backlog notification for task {taskId} to user {userId}: {ex.Message}");
                return false;
            }
        }
	    }
	}
