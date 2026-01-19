	using AutomatedTaskSystem.Models.Enums.UserRole;
	using AutomatedTaskSystem.Models;
	using Microsoft.AspNetCore.SignalR;
	using AutomatedTaskSystem.Hub;
	using AutomatedTaskSystem.Data;
	using AutomatedTaskSystem.Models.Enums.ProjectStatus;
	using AutomatedTaskSystem.Models.Enums.TaskStatus;
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
	            try
	            {
	                var ownerClient = _hubContext.Clients.User(userId);
	
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
	
	        public async Task<bool> NotifyUserOfTaskAssignment(int assignedUserId, int taskId, int? assignedByUserId = null)
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
	
	                return true;
	            }
	            catch (Exception ex)
	            {
	                Console.WriteLine($"Error sending TaskAssigned notification for task {taskId} to user {assignedUserId}: {ex.Message}");
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
	
	                return true;
	            }
	            catch (Exception ex)
	            {
	                Console.WriteLine($"Error sending ProjectCompleted notification for project {projectId}: {ex.Message}");
	                return false;
	            }
	        }
	    }
	}
