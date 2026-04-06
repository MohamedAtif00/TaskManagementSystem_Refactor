	using AutomatedTaskSystem.Data;
	using AutomatedTaskSystem.Models;
	using AutomatedTaskSystem.Models.Enums.ProjectStatus;
	using AutomatedTaskSystem.Models.Enums.UserRole;
	using AutomatedTaskSystem.Services.Notification;
	using AutomatedTaskSystem.Services.ResponseService;
	using AutomatedTaskSystem.Services.TokenService;

namespace AutomatedTaskSystem.Services.ProjectAssignmentService;

public class ProjectAssignmentService : IProjectAssignmentService
{
    private readonly DataContext _context;
	    private readonly INotificationService _notificationService;
	    private readonly ITokenService _tokenService;

	    public ProjectAssignmentService(DataContext context, INotificationService notificationService, ITokenService tokenService)
        {
            _context = context;
	        _notificationService = notificationService;
	        _tokenService = tokenService;
        }

		    public async Task<ResponseService<List<User>>> AssignUsersToProject(int Pid, List<int> userIds)
	    {
	        // Load project with current user assignments so we can avoid creating duplicate relationships
	        var project = await _context.Projects
	            .Where(p => !p.Archived && p.Id == Pid)
	            .Include(p => p.Users)
	            .FirstOrDefaultAsync();

	        if (project is null)
	            return new ResponseService<List<User>>
	            {
	                Error = true,
	                Message = "Project is not found"
	            };

	        // Fetch only existing, non-archived users from the requested ids
	        var users = await _context.Users
	            .Where(u => userIds.Contains(u.Id) && !u.Archived)
	            .ToListAsync();

	        // Only add users that are not already assigned to this project
	        var addedUsers = new List<User>();
	        foreach (var user in users)
	        {
	            var alreadyAssigned = project.Users.Any(u => u.Id == user.Id);
	            if (!alreadyAssigned)
	            {
	                project.Users.Add(user);
	                addedUsers.Add(user);
	            }
	        }

		        await _context.SaveChangesAsync();

		        // After successfully saving the new assignments, send notifications only to newly added users
		        if (addedUsers.Any())
		        {
		            int? assignedByUserId = null;
		            try
		            {
		                var authRes = _tokenService.GetUserIdFromToken();
		                if (!authRes.Error && int.TryParse(authRes.Data, out var parsedUserId))
		                {
		                    assignedByUserId = parsedUserId;
		                }
		            }
		            catch
		            {
		                // If we can't resolve the assigning user, fall back to "System" in the notification service
		            }

		            foreach (var user in addedUsers)
		            {
		                await _notificationService.NotifyUserOfProjectAssignment(user.Id, project.Id, assignedByUserId);
		            }
		        }

		        return new ResponseService<List<User>>
	        {
	            // Return only the users that were newly assigned, similar to UnassignUsersToProject
	            Data = addedUsers,
	            Error = false,
	            Message = addedUsers.Count == 0
	                ? "All provided users are already assigned to this project"
	                : "List of users added to project"
	        };
	    }

    public async Task<ResponseService<List<User>>> GetAssignedUsersForProject(int Pid)
    {
        var project = await _context.Projects
            .Where(p => p.Id == Pid && !p.Archived)
            .Include(p => p.Users)
            .ThenInclude(u => u.Group)
            .Include(p => p.Users)
            .FirstOrDefaultAsync();

        if (project is null)
            return new ResponseService<List<User>>
            {
                Error = true,
                Message = "Project is not found"
            };

        return new ResponseService<List<User>>
        {
            Data = project.Users,
            Error = false,
            Message = "List of users assigned to project"
        };
    }

    public async Task<ResponseService<List<User>>> GetUnassignedUsersForProject(int Pid)
    {
        var project = await _context.Projects
            .Where(p => !p.Archived && p.Id == Pid)
            .FirstOrDefaultAsync();

        if (project is null)
            return new ResponseService<List<User>>
            {
                Error = true,
                Message = "Project is not found"
            };

        var users = await _context.Users
            .Where(u => !u.Archived && !u.Projects.Contains(project))
            .Include(u => u.Group)
            .ToListAsync();

        return new ResponseService<List<User>>
        {
            Data = users,
            Error = false,
            Message = $"List of unassigned Users from Project of id:{Pid}"
        };
    }

    public async Task<ResponseService<List<Models.Project>>> ProjectsAssignedToUser(int Uid)
    {
        var user = await _context.Users
            .Where(u => !u.Archived && u.Id == Uid)
            .Include(u => u.Projects)
            .FirstOrDefaultAsync();

        if (user is null)
            return new ResponseService<List<Models.Project>>
            {
                Error = true,
                Message = $"User of id:{Uid} is not found"
            };

        return new ResponseService<List<Models.Project>>
        {
            Data =
                user.Role == UserRoleEnum.ProjectManger
                    ? await _context.Projects.Where(p => !p.Archived).ToListAsync()
                    : user.Projects
                        .Where(
                            p =>
                                !p.Archived
                                && p.Status != ProjectStatusEnum.Closed
                                && p.Status != ProjectStatusEnum.Hold
                        )
                        .ToList(),
            Error = false,
            Message = $"List of projects assigned to User of id:{user.Id}"
        };
    }

    public async Task<ResponseService<List<User>>> UnassignUsersToProject(
        int Pid,
        List<int> userIds
    )
    {
        var project = await _context.Projects
            .Where(p => !p.Archived && p.Id == Pid)
            .Include(u => u.Users)
            .FirstOrDefaultAsync();

        if (project is null)
            return new ResponseService<List<User>>
            {
                Error = true,
                Message = "Project is not found"
            };

        var users = new List<User> { };

        foreach (var uid in userIds)
        {
            var user = project.Users.Find(u => u.Id == uid);

            if (user is not null)
            {
                users.Add(user);
                project.Users.Remove(user);
            }
        }

		await _context.SaveChangesAsync();

        return new ResponseService<List<User>>
        {
            Data = users,
            Error = false,
            Message = $"List of users unassigned from project of id:{Pid}"
        };
    }
}
