using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Services.ProjectAssignmentService;

public class ProjectAssignmentService : IProjectAssignmentService
{
    private readonly DataContext _context;

    public ProjectAssignmentService(DataContext context)
    {
        _context = context;
    }

    public async Task<ResponseService<List<User>>> AssignUsersToProject(int Pid, List<int> userIds)
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
            .Where(u => userIds.Contains(u.Id) && !u.Archived)
            .ToListAsync();

        foreach (var user in users)
        {
            user.Projects.Add(project);
            project.Users.Add(user);
        }

        await _context.SaveChangesAsync();

        return new ResponseService<List<User>>
        {
            Data = users,
            Error = false,
            Message = "List of users added to project"
        };
    }

    public async Task<ResponseService<List<User>>> GetAssignedUsersForProject(int Pid)
    {
        var project = await _context.Projects
            .Where(p => p.Id == Pid && !p.Archived)
            .Include(p => p.Users)
            .ThenInclude(u => u.Group)
            .Include(p => p.Users)
            .ThenInclude(u => u.Role)
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
            .Include(u => u.Role)
            .ToListAsync();

        return new ResponseService<List<User>>
        {
            Data = users,
            Error = false,
            Message = $"List of unassigned Users from Project of id:{Pid}"
        };
    }

    public async Task<ResponseService<List<Project>>> ProjectsAssignedToUser(int Uid)
    {
        var user = await _context.Users
            .Where(u => !u.Archived && u.Id == Uid)
            .Include(u => u.Projects)
            .FirstOrDefaultAsync();

        if (user is null)
            return new ResponseService<List<Project>>
            {
                Error = true,
                Message = $"User of id:{Uid} is not found"
            };

        return new ResponseService<List<Project>>
        {
            Data =
                user.RoleId == 1
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
