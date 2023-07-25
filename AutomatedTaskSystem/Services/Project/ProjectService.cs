using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Services.LearningObjectiveService;
using AutomatedTaskSystem.Services.ProjectAssignmentService;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using AutomatedTaskSystem.Services.UnitService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.ProjectService;

public class ProjectService : IProjectService
{
    private readonly DataContext _context;
    private readonly IProjectAssignmentService _projectAssignmentService;
    private readonly IUnitService _unitService;
    private readonly ILearningObjectiveService _learningObjectiveService;
    private readonly ITokenService _tokenService;

    public ProjectService(
        DataContext context,
        IProjectAssignmentService projectAssignmentService,
        IUnitService unitService,
        ILearningObjectiveService learningObjectiveService,
        ITokenService tokenService
    )
    {
        _context = context;
        _projectAssignmentService = projectAssignmentService;
        _unitService = unitService;
        _learningObjectiveService = learningObjectiveService;
        _tokenService = tokenService;
    }

    public async Task<ActionResult<ResponseService<Responses.ProjectUnitDTO>>> AddUnit(
        int Id,
        string Name
    )
    {
        var project = await _context.Projects
            .Where(p => p.Id == Id && !p.Archived)
            .FirstOrDefaultAsync();

        if (project is null)
            return new NotFoundResult();

        var unit = await _unitService.CreateUnit(Name, project);

        return new ResponseService<Responses.ProjectUnitDTO>
        {
            Error = false,
            Message = unit.Message,
            Data = new Responses.ProjectUnitDTO
            {
                Id = unit.Data!.Id,
                Name = unit.Data.Name,
                Lessons = new List<Responses.ProjectLessonDTO> { }
            }
        };
    }

    public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> AssignToProject(
        int Id,
        List<int> UserIds
    )
    {
        var user = await _projectAssignmentService.AssignUsersToProject(Id, UserIds);

        if (user.Error)
            return new NotFoundObjectResult(user);

        return new ResponseService<List<Responses.IDName>>
        {
            Error = false,
            Data = user.Data!
                .Select(u => new Responses.IDName { Id = u.Id, Name = u.Name })
                .ToList(),
            Message = user.Message
        };
    }

    public async Task<ActionResult<ResponseService<Responses.ProjectDTO>>> CreateProject(
        string Name,
        string Description,
        int YearId,
        bool Term
    )
    {
        if (await CheckIfProjectExists(Name))
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Project already exists." }
            );

        var year = await _context.Years.Where(y => y.Id == YearId).FirstOrDefaultAsync();

        if (year is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Year" }
            );

        var newProject = new Project
        {
            Name = Name,
            Description = Description,
            Term = Term,
            Year = year,
            YearId = year.Id,
            Status = ProjectStatus.Active
        };

        _context.Projects.Add(newProject);
        await _context.SaveChangesAsync();

        return new ResponseService<Responses.ProjectDTO>
        {
            Data = new Responses.ProjectDTO
            {
                Id = newProject.Id,
                Description = newProject.Description,
                Name = newProject.Name,
                Year = new Responses.IDName
                {
                    Id = newProject.YearId,
                    Name = newProject.Year.Number
                },
                Term = newProject.Term,
                Status = newProject.Status
            },
            Error = false,
            Message = $"Project {Name} is created.",
        };
    }

    public async Task<ActionResult<BaseResponseService>> DeleteProject(int Id)
    {
        var project = await _context.Projects
            .Where(p => p.Id == Id && !p.Archived)
            .Include(p => p.Units)
            .ThenInclude(u => u.Lessons)
            .ThenInclude(l => l.LearningObjectives)
            .ThenInclude(lo => lo.Tasks)
            .FirstOrDefaultAsync();

        if (project is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Project is not found" }
            );

        foreach (var unit in project.Units)
        {
            if (unit.Archived)
                continue;
            foreach (var lesson in unit.Lessons)
            {
                if (lesson.Archived)
                    continue;
                foreach (var lo in lesson.LearningObjectives)
                {
                    if (lo.Archived)
                        continue;
                    foreach (var task in lo.Tasks)
                    {
                        if (task.Archived)
                            continue;
                        task.Archived = true;
                    }
                    lo.Archived = true;
                }
                lesson.Archived = true;
            }
            unit.Archived = true;
        }
        project.Archived = true;

        await _context.SaveChangesAsync();

        return new BaseResponseService { Error = false, Message = "Project is now Deleted" };
    }

    public async Task<ActionResult<ResponseService<Responses.ProjectDTO>>> EditProject(
        int id,
        string Name,
        string Description,
        int YearId,
        bool term
    )
    {
        var project = await _context.Projects
            .Where(p => p.Id == id && !p.Archived)
            .Include(p => p.Year)
            .FirstOrDefaultAsync();

        if (project is null)
            return new NotFoundResult();

        if (YearId != project.YearId)
        {
            var year = await _context.Years.Where(y => y.Id == YearId).FirstOrDefaultAsync();

            if (year is null)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Invalid year" }
                );

            project.Year = year;
        }

        project.Name = Name;
        project.Description = Description;
        project.Term = term;

        await _context.SaveChangesAsync();

        return new ResponseService<Responses.ProjectDTO>
        {
            Data = new Responses.ProjectDTO
            {
                Id = project.Id,
                Description = project.Description,
                Name = project.Name,
                Term = project.Term,
                Year = new Responses.IDName { Name = project.Year.Number, Id = project.Year.Id, },
                Status = project.Status
            },
            Error = false,
            Message = $"Project of id:{id} edited.",
        };
    }

    public async Task<ActionResult<ResponseService<List<Responses.ProjectDTO>>>> GetAllProjects()
    {
        var projects = await _context.Projects
            .Where(p => !p.Archived)
            .Include(p => p.Year)
            .ToListAsync();

        return new ResponseService<List<Responses.ProjectDTO>>
        {
            Error = false,
            Data = projects
                .Select(
                    p =>
                        new Responses.ProjectDTO
                        {
                            Description = p.Description,
                            Id = p.Id,
                            Name = p.Name,
                            Term = p.Term,
                            Year = new Responses.IDName { Id = p.YearId, Name = p.Year.Number },
                            Status = p.Status
                        }
                )
                .ToList(),
            Message = "List of all projects"
        };
    }

    public async Task<ActionResult<ResponseService<List<Responses.UserDTO>>>> GetAssignedUsers(
        int Id
    )
    {
        var project = await _context.Projects
            .Where(p => !p.Archived && p.Id == Id)
            .Include(p => p.Users)
            .ThenInclude(u => u.Group)
            .Include(p => p.Users)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync();

        if (project is null)
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"Project of id:{Id} is not found"
                }
            );

        var res = new List<Responses.UserDTO> { };

        foreach (var user in project.Users)
            if (!user.Archived)
                res.Add(
                    new Responses.UserDTO
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Group = new Responses.IDName { Id = user.GroupId, Name = user.Group.Name },
                        Role = new Responses.IDName { Id = user.Role.Id, Name = user.Role.Name }
                    }
                );

        return new ResponseService<List<Responses.UserDTO>>
        {
            Data = res,
            Error = false,
            Message = $"List of assigned users for project of id:{project.Id}"
        };
    }

    public async Task<ActionResult<ResponseService<Responses.ProjectDTO>>> GetProject(int Id)
    {
        var project = await _context.Projects
            .Where(p => p.Id == Id && !p.Archived)
            .Include(p => p.Year)
            .FirstOrDefaultAsync();

        if (project is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Project is not found" }
            );

        return new ResponseService<Responses.ProjectDTO>
        {
            Data = new Responses.ProjectDTO
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Term = project.Term,
                Year = new Responses.IDName { Id = project.YearId, Name = project.Year.Number },
                Status = project.Status
            },
            Error = false,
            Message = "Project found"
        };
    }

    public async Task<
        ActionResult<ResponseService<Responses.DetailedProjectDTO>>
    > GetProjectDetails(int Id)
    {
        var project = await _context.Projects
            .Where(p => p.Id == Id && !p.Archived)
            .Include(p => p.Units)
            .ThenInclude(u => u.Lessons)
            .ThenInclude(l => l.LearningObjectives)
            .ThenInclude(lo => lo.Schema)
            .FirstOrDefaultAsync();

        if (project is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Project is not found" }
            );

        var status = project.Status;
        return new ResponseService<Responses.DetailedProjectDTO>
        {
            Data = new Responses.DetailedProjectDTO
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status =
                    status == ProjectStatus.Closed
                        ? "Close"
                        : status == ProjectStatus.Hold
                            ? "On Hold"
                            : status == ProjectStatus.Active
                                ? "Active"
                                : "Reopened",
                Units = project.Units
                    .Where(u => !u.Archived)
                    .Select(
                        u =>
                            new Responses.ProjectUnitDTO
                            {
                                Id = u.Id,
                                Name = u.Name,
                                Lessons = u.Lessons
                                    .Where(l => !l.Archived)
                                    .Select(
                                        l =>
                                            new Responses.ProjectLessonDTO
                                            {
                                                Id = l.Id,
                                                Name = l.Name,
                                                LearningObjectives = l.LearningObjectives
                                                    .Where(lo => !lo.Archived)
                                                    .Select(
                                                        lo =>
                                                            new Responses.LearningObjectiveDTO
                                                            {
                                                                Id = lo.Id,
                                                                Environment = lo.Environment,
                                                                Name = lo.Name,
                                                                Schema = new Responses.IDName
                                                                {
                                                                    Id = lo.SchemaId,
                                                                    Name = lo.Schema.Name
                                                                },
                                                                Tag = lo.Tag,
                                                                Template = lo.Template
                                                            }
                                                    )
                                                    .ToList()
                                            }
                                    )
                                    .ToList()
                            }
                    )
                    .ToList()
            }
        };
    }

    public async Task<
        ActionResult<ResponseService<List<Responses.IDName>>>
    > GetProjectLearningObjectives(int Id)
    {
        var res = await _learningObjectiveService.GetLearningObjectivesByProjectId(Id);

        if (res.Error)
            return new NotFoundObjectResult(res);

        return new ResponseService<List<Responses.IDName>>
        {
            Data = res.Data!
                .Select(lo => new Responses.IDName { Id = lo.Id, Name = lo.Name })
                .ToList(),
            Error = false,
            Message = $"List of learning objective for project of id:{Id}"
        };
    }

    public async Task<ActionResult<ResponseService<List<Responses.UserDTO>>>> GetUnassignedUsers(
        int Id
    )
    {
        var project = await _context.Projects
            .Where(p => !p.Archived && p.Id == Id)
            .FirstOrDefaultAsync();

        if (project is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Project is not found" }
            );

        var users = await _context.Users
            .Where(u => !u.Archived && !u.Projects.Any(p => p.Id == project.Id))
            .Include(u => u.Projects)
            .Include(u => u.Group)
            .Include(u => u.Role)
            .ToListAsync();

        return new ResponseService<List<Responses.UserDTO>>
        {
            Data = users
                .Select(
                    u =>
                        new Responses.UserDTO
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Group = { Id = u.GroupId, Name = u.Group.Name },
                            Role = { Id = u.RoleId, Name = u.Role.Name },
                        }
                )
                .ToList(),
            Error = false,
            Message = $"List of unassigned users for project of id:{project.Id}"
        };
    }

    public async Task<
        ActionResult<ResponseService<List<Responses.ProjectDTO>>>
    > GetUserSpecificProjects()
    {
        var authRes = _tokenService.GetUserIdFromToken();
        if (authRes.Error)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = authRes.Message }
            );

        var convertable = Int32.TryParse(authRes.Data!, out int uid);

        if (!convertable)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid token" }
            );

        var user = await _context.Users
            .Where(u => u.Id == uid && !u.Archived)
            .Include(u => u.Projects)
            .ThenInclude(p => p.Year)
            .FirstOrDefaultAsync();

        if (user is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = $"User of id:{uid} is not found" }
            );

        if (user.RoleId == 1)
            return new ResponseService<List<Responses.ProjectDTO>>
            {
                Error = false,
                Message = "List of all projects",
                Data = await _context.Projects
                    .Where(p => !p.Archived)
                    .Include(p => p.Year)
                    .Select(
                        p =>
                            new Responses.ProjectDTO
                            {
                                Id = p.Id,
                                Name = p.Name,
                                Description = p.Description,
                                Term = p.Term,
                                Year = new Responses.IDName { Id = p.YearId, Name = p.Year.Number },
                                Status = p.Status
                            }
                    )
                    .ToListAsync()
            };

        var listOfProjects = new List<Responses.ProjectDTO> { };

        foreach (var project in user.Projects)
            if (
                !project.Archived
                && project.Status != ProjectStatus.Hold
                && project.Status != ProjectStatus.Closed
            )
                listOfProjects.Add(
                    new Responses.ProjectDTO
                    {
                        Id = project.Id,
                        Name = project.Name,
                        Description = project.Description,
                        Term = project.Term,
                        Year = new Responses.IDName
                        {
                            Id = project.YearId,
                            Name = project.Year.Number
                        },
                        Status = project.Status
                    }
                );

        return new ResponseService<List<Responses.ProjectDTO>>
        {
            Error = false,
            Message = $"Projects assigned to users of id:{user.Id}",
            Data = listOfProjects
        };
    }

    public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> UnassignToProject(
        int Id,
        List<int> UserIds
    )
    {
        var res = await _projectAssignmentService.UnassignUsersToProject(Id, UserIds);

        if (res.Error)
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"Project of id:{Id} is not found"
                }
            );

        return new ResponseService<List<Responses.IDName>>
        {
            Data = res.Data!
                .Select(u => new Responses.IDName { Id = u.Id, Name = u.Name })
                .ToList(),
            Error = false,
            Message = res.Message
        };
    }

    public async Task<ActionResult<ResponseService<Responses.ProjectDTO>>> UpdateProjectStatus(
        int id,
        ProjectStatus status
    )
    {
        var project = await _context.Projects
            .Where(p => !p.Archived && p.Id == id)
            .Include(p => p.Year)
            .FirstOrDefaultAsync();

        if (project is null)
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"Project of id:{id} is not found"
                }
            );

        if (status == ProjectStatus.Active)
        {
            if (project.Status == ProjectStatus.Active || project.Status == ProjectStatus.Reopened)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Project is already active" }
                );

            if (project.Status == ProjectStatus.Hold)
                project.Status = ProjectStatus.Active;
            else if (project.Status == ProjectStatus.Closed)
                project.Status = ProjectStatus.Reopened;
        }
        else if (status == ProjectStatus.Hold)
        {
            if (project.Status == ProjectStatus.Hold)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Project is already on Hold" }
                );

            if (project.Status == ProjectStatus.Closed)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Project is Closed" }
                );

            project.Status = ProjectStatus.Hold;
        }
        else if (status == ProjectStatus.Closed)
        {
            if (project.Status == ProjectStatus.Closed)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Project is already closed" }
                );

            project.Status = ProjectStatus.Closed;
        }

        await _context.SaveChangesAsync();

        return new ResponseService<Responses.ProjectDTO>
        {
            Message = "Project Status is updated",
            Error = false,
            Data = new Responses.ProjectDTO
            {
                Status = project.Status,
                Id = project.Id,
                Name = project.Name,
                Term = project.Term,
                Year = new Responses.IDName { Name = project.Year.Number, Id = project.YearId },
                Description = project.Description
            }
        };
    }

    private async Task<bool> CheckIfProjectExists(string Name) =>
        await _context.Projects.AnyAsync(p => p.Name.ToLower() == Name.ToLower() && !p.Archived);
}
