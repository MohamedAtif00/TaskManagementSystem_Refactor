using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Models.Enums.UserRole;
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
            Status = ProjectStatusEnum.Active
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
            foreach (var lesson in unit.Lessons)
            {
                foreach (var lo in lesson.LearningObjectives)
                {
                    foreach (var task in lo.Tasks)
                    {
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
                        Group = new Responses.IDName { Id = user.GroupId ?? 0, Name = user.Group.Name },
                        Role = user.Role
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

        return new ResponseService<Responses.DetailedProjectDTO>
        {
            Data = new Responses.DetailedProjectDTO
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
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
            .Include(u => u.Group) // Ensure Group is loaded
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
                            Role = u.Role,
                            // --- FIX APPLIED HERE ---
                            Group = u.Group != null // Check if u.Group is not null before accessing its properties
                                ? new Responses.IDName // Assuming you have a GroupDTO in Responses namespace
                                {
                                    Id = u.GroupId ?? 0, // u.GroupId could be null, use null-coalescing
                                    Name = u.Group.Name // Now it's safe to access u.Group.Name
                                }
                                : null // Or a default GroupDTO if Group can be null
                        }
                )
                .ToList(),
            Error = false,
            Message = $"List of unassigned users for project of id:{project.Id}"
        };
    }
    public async Task<ActionResult<ResponseService<List<Responses.ProjectDTO>>>> GetUserSpecificProjects()
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

        // Common logic to determine the relevant groups for SectionHead
        var userGroup = await _context.Groups
            .Where(g => g.Id == user.GroupId)
            .FirstOrDefaultAsync();

        if (userGroup is null)
            throw new Exception("User has a not found group");

        var groups = new List<Group> { userGroup };

        if (user.Role == UserRoleEnum.SectionHead)
        {
            var section = await _context.Sections
                .Where(s => s.HeadId == user.Id && !s.Archived)
                .FirstOrDefaultAsync();

            if (section is not null)
            {
                var sectionGroupIds = await _context.SectionGroups
                    .Where(sg => sg.SectionId == section.Id)
                    .Select(sg => sg.GroupId)
                    .ToListAsync();

                var sectionGroups = await _context.Groups
                    .Where(g => sectionGroupIds.Contains(g.Id))
                    .ToListAsync();

                groups.AddRange(sectionGroups);
            }
        }

        if (user.Role == UserRoleEnum.ProjectManger || user.Role == UserRoleEnum.Owner)
        {
            var allProjects = await _context.Projects
                .Where(
                    p =>
                        !p.Archived
                        && p.Status != ProjectStatusEnum.Hold
                        && p.Status != ProjectStatusEnum.Closed
                )
                .Include(p => p.Year)
                .ToListAsync();

            var projectIds = allProjects.Select(p => p.Id).ToList();

            // Task counting logic for ProjectManager/Owner roles
            var taskCounts = await _context.Tasks
                .Where(t =>
                    !t.Archived &&
                    t.LearningObjective != null &&
                    t.LearningObjective.Lesson != null &&
                    t.LearningObjective.Lesson.Unit != null &&
                    projectIds.Contains(t.LearningObjective.Lesson.Unit.ProjectId) &&
                    t.Status != TaskStatusEnum.Done // Filter out 'Done' tasks
                )
                .GroupBy(t => t.LearningObjective.Lesson.Unit.ProjectId)
                .Select(g => new { ProjectId = g.Key, Count = g.Count() })
                .ToListAsync();

            var data = allProjects.Select(p => new Responses.ProjectDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Term = p.Term,
                Year = new Responses.IDName { Id = p.YearId, Name = p.Year.Number },
                Status = p.Status,
                Count = taskCounts.FirstOrDefault(tc => tc.ProjectId == p.Id)?.Count ?? 0
            }).ToList();

            return new ResponseService<List<Responses.ProjectDTO>>
            {
                Error = false,
                Message = "List of all projects",
                Data = data
            };
        }

        var userProjects = user.Projects
            .Where(p =>
                !p.Archived &&
                p.Status != ProjectStatusEnum.Hold &&
                p.Status != ProjectStatusEnum.Closed
            )
            .ToList();

        var userProjectIds = userProjects.Select(p => p.Id).ToList();

        // Task counting logic for other roles (Team Leader, Section Head, Member)
        IQueryable<Models.Task> baseTaskQuery = _context.Tasks
            .Where(t =>
                !t.Archived &&
                t.LearningObjective != null &&
                t.LearningObjective.Lesson != null &&
                t.LearningObjective.Lesson.Unit != null &&
                userProjectIds.Contains(t.LearningObjective.Lesson.Unit.ProjectId) &&
                t.Status != TaskStatusEnum.Done // Filter out 'Done' tasks
            );

        if (user.Role == UserRoleEnum.TeamLeader || user.Role == UserRoleEnum.SectionHead)
        {
            baseTaskQuery = baseTaskQuery.Where(t =>
                groups.Select(g => g.Id).Contains(t.GroupId)
            );
        }
        else // UserRoleEnum.Member
        {
            baseTaskQuery = baseTaskQuery.Where(t =>
                t.GroupId == user.GroupId &&
                (t.UserId == user.Id || t.Status == TaskStatusEnum.Backlog) &&
                (!t.TL || t.UserId == user.Id)
            );
        }

        var userTaskCounts = await baseTaskQuery
            .GroupBy(t => t.LearningObjective.Lesson.Unit.ProjectId)
            .Select(g => new { ProjectId = g.Key, Count = g.Count() })
            .ToListAsync();

        var listOfProjects = userProjects.Select(project => new Responses.ProjectDTO
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
            Status = project.Status,
            Count = userTaskCounts.FirstOrDefault(tc => tc.ProjectId == project.Id)?.Count ?? 0
        }).ToList();

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
        ProjectStatusEnum status
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

        if (status == ProjectStatusEnum.Active)
        {
            if (
                project.Status == ProjectStatusEnum.Active
                || project.Status == ProjectStatusEnum.Reopened
            )
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Project is already active" }
                );

            if (project.Status == ProjectStatusEnum.Hold)
                project.Status = ProjectStatusEnum.Active;
            else if (project.Status == ProjectStatusEnum.Closed)
                project.Status = ProjectStatusEnum.Reopened;
        }
        else if (status == ProjectStatusEnum.Hold)
        {
            if (project.Status == ProjectStatusEnum.Hold)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Project is already on Hold" }
                );

            if (project.Status == ProjectStatusEnum.Closed)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Project is Closed" }
                );

            project.Status = ProjectStatusEnum.Hold;
        }
        else if (status == ProjectStatusEnum.Closed)
        {
            if (project.Status == ProjectStatusEnum.Closed)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Project is already closed" }
                );

            project.Status = ProjectStatusEnum.Closed;
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
}
