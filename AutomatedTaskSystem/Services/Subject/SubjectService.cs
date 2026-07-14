using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.LearningObjectiveService;
using AutomatedTaskSystem.Services.Notification;
using AutomatedTaskSystem.Services.ProjectAssignmentService;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using AutomatedTaskSystem.Services.UnitService;
using AutomatedTaskSystem.Helper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Data.Common;
using System.Text.Json;
using static AutomatedTaskSystem.DTO.Responses;

namespace AutomatedTaskSystem.Services.SubjectService;

public class SubjectService(
    DataContext context,
    IMemoryCache cache,
    IProjectAssignmentService projectAssignmentService,
    IUnitService unitService,
    ILearningObjectiveService learningObjectiveService,
    ITokenService tokenService,
    INotificationService notificationService) : ISubjectService
{
    private async Task<DbConnection> GetOpenConnectionAsync()
    {
        var connection = context.Database.GetDbConnection();
        await connection.EnsureOpenAsync();
        return connection;
    }

    private static int CalculateProgressPercent(int completed, int expected)
    {
        if (expected <= 0) return 0;
        return Math.Min(100, (int)Math.Round((double)completed / expected * 100));
    }

    private static bool CanViewAllSubjects(UserRoleEnum role) =>
        role is UserRoleEnum.ProjectManger or UserRoleEnum.Owner;

    private static bool PassesAssignmentStatusFilter(Subject s, bool includeInactiveStatuses) =>
        includeInactiveStatuses
        || (s.Status != ProjectStatusEnum.Hold && s.Status != ProjectStatusEnum.Closed);

    private static List<Subject> FilterSubjectsByUserRole(
        Models.User user,
        IEnumerable<Subject> candidates,
        bool includeInactiveStatuses
    )
    {
        var list = candidates
            .Where(s => !s.Archived && PassesAssignmentStatusFilter(s, includeInactiveStatuses))
            .ToList();

        if (CanViewAllSubjects(user.Role))
            return list;

        var assignedIds = user.Subjects
            .Where(s => !s.Archived)
            .Select(s => s.Id)
            .ToHashSet();

        return list.Where(s => assignedIds.Contains(s.Id)).ToList();
    }

    private static bool UserCanAccessSubject(Models.User user, Subject subject)
    {
        if (CanViewAllSubjects(user.Role))
            return true;

        return user.Subjects.Any(s => !s.Archived && s.Id == subject.Id);
    }

    private async Task<(Models.User? User, ActionResult? Error)> ResolveCurrentUserAsync()
    {
        var authRes = tokenService.GetUserIdFromToken();
        if (authRes.Error)
            return (
                null,
                new UnauthorizedObjectResult(
                    new BaseResponseService { Error = true, Message = authRes.Message }
                )
            );

        if (!int.TryParse(authRes.Data, out var uid))
            return (
                null,
                new UnauthorizedObjectResult(
                    new BaseResponseService { Error = true, Message = "Invalid token" }
                )
            );

        var user = await context.Users
            .Where(u => u.Id == uid && !u.Archived)
            .Include(u => u.Subjects)
            .FirstOrDefaultAsync();

        if (user is null)
            return (
                null,
                new NotFoundObjectResult(
                    new BaseResponseService { Error = true, Message = $"User of id:{uid} is not found" }
                )
            );

        return (user, null);
    }

    private static SubjectDTO MapSubjectDto(
        Subject s,
        string folderPath,
        int? progressPercent = null,
        int? count = null
    )
    {
        return new SubjectDTO
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            FolderId = s.FolderId,
            FolderPath = folderPath,
            Status = s.Status,
            ProgressPercent = progressPercent,
            Count = count
        };
    }

    private async Task<string> BuildFolderPathAsync(int folderId)
    {
        var parts = new List<string>();
        var current = await context.Folders.FirstOrDefaultAsync(f => f.Id == folderId);
        while (current is not null)
        {
            parts.Add(current.Name);
            current = current.ParentFolderId.HasValue
                ? await context.Folders.FirstOrDefaultAsync(f => f.Id == current.ParentFolderId.Value)
                : null;
        }

        parts.Reverse();
        return string.Join(" > ", parts);
    }

    private async Task<Dictionary<int, string>> BuildFolderPathsAsync(IEnumerable<int> folderIds)
    {
        var result = new Dictionary<int, string>();
        foreach (var folderId in folderIds.Distinct())
        {
            result[folderId] = await BuildFolderPathAsync(folderId);
        }

        return result;
    }

    private static List<string> DeserializeLevelNames(string? levelNamesJson)
    {
        if (string.IsNullOrWhiteSpace(levelNamesJson))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(levelNamesJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private async Task<List<string>> BuildFolderLevelNamesAsync(int folderId)
    {
        var current = await context.Folders
            .Include(f => f.Project)
            .FirstOrDefaultAsync(f => f.Id == folderId);

        while (current?.ParentFolderId is not null)
        {
            current = await context.Folders
                .Include(f => f.Project)
                .FirstOrDefaultAsync(f => f.Id == current.ParentFolderId.Value);
        }

        return DeserializeLevelNames(current?.Project?.LevelNamesJson);
    }

    private async Task<Dictionary<int, List<string>>> BuildFolderLevelNamesMapAsync(IEnumerable<int> folderIds)
    {
        var result = new Dictionary<int, List<string>>();
        foreach (var folderId in folderIds.Distinct())
        {
            result[folderId] = await BuildFolderLevelNamesAsync(folderId);
        }

        return result;
    }

    private async Task<Dictionary<int, int>> GetProgressBySubjectIdAsync(IEnumerable<int> subjectIds)
    {
        var ids = subjectIds.Distinct().ToList();
        if (ids.Count == 0 || !context.Database.IsRelational())
        {
            return ids.ToDictionary(id => id, _ => 0);
        }

        var connection = await GetOpenConnectionAsync();

        // Pass IDs as JSON once instead of expanding IN (@p0, @p1, ...) which is slow for large lists.
        // Drive both aggregates from the ID set so SQL Server starts at Units by SubjectId.
        const string progressSql = """
WITH SubjectIds AS (
    SELECT DISTINCT CAST([value] AS INT) AS Id
    FROM OPENJSON(@subjectIdsJson)
),
Expected AS (
    SELECT
        u.SubjectId,
        ExpectedTasks = COUNT(1)
    FROM SubjectIds sid
    INNER JOIN Units u ON u.SubjectId = sid.Id AND u.Archived = 0
    INNER JOIN Lessons l ON l.UnitId = u.Id AND l.Archived = 0
    INNER JOIN LearningObjectives lo ON lo.LessonId = l.Id AND lo.Archived = 0
    INNER JOIN Nodes n ON n.SchemaId = lo.SchemaId AND n.Archived = 0
    INNER JOIN Steps s ON s.NodeId = n.Id AND s.Archived = 0
    GROUP BY u.SubjectId
),
Completed AS (
    SELECT
        u.SubjectId,
        CompletedTasks = COUNT(1)
    FROM SubjectIds sid
    INNER JOIN Units u ON u.SubjectId = sid.Id AND u.Archived = 0
    INNER JOIN Lessons l ON l.UnitId = u.Id AND l.Archived = 0
    INNER JOIN LearningObjectives lo ON lo.LessonId = l.Id AND lo.Archived = 0
    INNER JOIN Tasks t ON t.LearningObjectiveId = lo.Id AND t.Archived = 0 AND t.Status = @done
    GROUP BY u.SubjectId
)
SELECT
    SubjectId = COALESCE(e.SubjectId, c.SubjectId),
    ExpectedTasks = ISNULL(e.ExpectedTasks, 0),
    CompletedTasks = ISNULL(c.CompletedTasks, 0)
FROM Expected e
FULL OUTER JOIN Completed c ON c.SubjectId = e.SubjectId;
""";

        var rows = await connection.QueryAsync<SubjectProgressRow>(
            progressSql,
            new
            {
                subjectIdsJson = JsonSerializer.Serialize(ids),
                done = (int)TaskStatusEnum.Done
            }
        );

        var map = ids.ToDictionary(id => id, _ => 0);
        foreach (var r in rows)
        {
            map[r.SubjectId] = CalculateProgressPercent(r.CompletedTasks, r.ExpectedTasks);
        }
        return map;
    }

    private sealed record SubjectProgressRow(int SubjectId, int ExpectedTasks, int CompletedTasks);

    public async Task<ActionResult<ResponseService<ProjectUnitDTO>>> AddUnit(int Id, string Name)
    {
        var subject = await context.Subjects
            .Where(p => p.Id == Id && !p.Archived)
            .FirstOrDefaultAsync();

        if (subject is null)
            return new NotFoundResult();

        var unit = await unitService.CreateUnit(Name, subject);

        return new ResponseService<ProjectUnitDTO>
        {
            Error = false,
            Message = unit.Message,
            Data = new ProjectUnitDTO
            {
                Id = unit.Data!.Id,
                Name = unit.Data.Name,
                Lessons = new List<ProjectLessonDTO>()
            }
        };
    }

    public async Task<ActionResult<ResponseService<List<IDName>>>> AssignToProject(int Id, List<int> UserIds)
    {
        var user = await projectAssignmentService.AssignUsersToProject(Id, UserIds);

        if (user.Error)
            return new NotFoundObjectResult(user);

        return new ResponseService<List<IDName>>
        {
            Error = false,
            Data = user.Data!
                .Select(u => new IDName { Id = u.Id, Name = u.Name })
                .ToList(),
            Message = user.Message
        };
    }

    public async Task<ActionResult<ResponseService<SubjectDTO>>> CreateProject(
        string Name,
        string Description,
        int folderId)
    {
        var folder = await context.Folders.FirstOrDefaultAsync(f => f.Id == folderId);
        if (folder is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid folder" }
            );

        var subject = new Subject
        {
            Name = Name,
            Description = Description,
            FolderId = folderId,
            Folder = folder,
            Status = ProjectStatusEnum.Active
        };

        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        InvalidateAllSubjectsCache();
        var folderPath = await BuildFolderPathAsync(subject.FolderId);

        return new ResponseService<SubjectDTO>
        {
            Data = MapSubjectDto(subject, folderPath),
            Error = false,
            Message = $"Subject {Name} is created.",
        };
    }

    public async Task<ActionResult<BaseResponseService>> DeleteProject(int Id)
    {
        var subject = await context.Subjects
            .Where(p => p.Id == Id && !p.Archived)
            .Include(p => p.Units)
            .ThenInclude(u => u.Lessons)
            .ThenInclude(l => l.LearningObjectives)
            .ThenInclude(lo => lo.Tasks)
            .FirstOrDefaultAsync();

        if (subject is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Subject is not found" }
            );

        foreach (var unit in subject.Units)
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
        subject.Archived = true;

        await context.SaveChangesAsync();

        return new BaseResponseService { Error = false, Message = "Subject is now deleted" };
    }

    public async Task<ActionResult<ResponseService<SubjectDTO>>> EditProject(
        int id,
        string Name,
        string Description,
        int folderId)
    {
        var subject = await context.Subjects
            .Where(p => p.Id == id && !p.Archived)
            .Include(p => p.Folder)
            .FirstOrDefaultAsync();

        if (subject is null)
            return new NotFoundResult();

        if (folderId != subject.FolderId)
        {
            var folder = await context.Folders.FirstOrDefaultAsync(f => f.Id == folderId);

            if (folder is null)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Invalid folder" }
                );

            subject.Folder = folder;
            subject.FolderId = folderId;
        }

        subject.Name = Name;
        subject.Description = Description;

        await context.SaveChangesAsync();
        InvalidateAllSubjectsCache();
        var folderPath = await BuildFolderPathAsync(subject.FolderId);

        return new ResponseService<SubjectDTO>
        {
            Data = MapSubjectDto(subject, folderPath),
            Error = false,
            Message = $"Subject of id:{id} edited.",
        };
    }

    public async Task<ActionResult<ResponseService<List<SubjectDTO>>>> GetSubjectsByFolder(
        int folderId,
        bool includeInactiveStatuses = false
    )
    {
        var (user, userError) = await ResolveCurrentUserAsync();
        if (userError is not null)
            return userError;

        var folder = await context.Folders.FirstOrDefaultAsync(f => f.Id == folderId);
        if (folder is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = $"Folder {folderId} not found" }
            );

        var subjects = await context.Subjects
            .Where(s => !s.Archived && s.FolderId == folderId)
            .Include(s => s.Folder)
            .ToListAsync();

        var visible = FilterSubjectsByUserRole(user!, subjects, includeInactiveStatuses);

        var progressById = await GetProgressBySubjectIdAsync(visible.Select(s => s.Id));
        var paths = await BuildFolderPathsAsync(visible.Select(s => s.FolderId));

        return new ResponseService<List<SubjectDTO>>
        {
            Error = false,
            Data = visible
                .Select(
                    s =>
                        MapSubjectDto(
                            s,
                            paths.GetValueOrDefault(s.FolderId, s.Folder?.Name ?? string.Empty),
                            progressById.GetValueOrDefault(s.Id)
                        )
                )
                .ToList(),
            Message = "Subjects for folder",
        };
    }

    public async Task<ActionResult<ResponseService<List<SubjectDTO>>>> GetAllProjects()
    {
        if (cache.TryGetValue("AllSubjects", out List<SubjectDTO>? cachedData) && cachedData != null)
        {
            return new ResponseService<List<SubjectDTO>> { Data = cachedData };
        }

        var subjects = await context.Subjects
            .Where(p => !p.Archived)
            .Include(p => p.Folder)
            .ToListAsync();

        var progressById = await GetProgressBySubjectIdAsync(subjects.Select(p => p.Id));
        var paths = await BuildFolderPathsAsync(subjects.Select(s => s.FolderId));

        var list = subjects
            .Select(
                s =>
                    MapSubjectDto(
                        s,
                        paths.GetValueOrDefault(s.FolderId, s.Folder?.Name ?? string.Empty),
                        progressById.GetValueOrDefault(s.Id)
                    )
            )
            .ToList();

        return new ResponseService<List<SubjectDTO>>
        {
            Error = false,
            Data = list,
            Message = "List of all subjects"
        };
    }

    public async Task<ActionResult<ResponseService<List<SubjectDTO>>>> GetAllProjectsForSprint()
    {
        var subjects = await context.Subjects
            .Where(p => !p.Archived && p.Status != ProjectStatusEnum.Hold && p.Status != ProjectStatusEnum.Closed)
            .Include(p => p.Folder)
            .ToListAsync();

        var progressById = await GetProgressBySubjectIdAsync(subjects.Select(p => p.Id));
        var paths = await BuildFolderPathsAsync(subjects.Select(s => s.FolderId));

        return new ResponseService<List<SubjectDTO>>
        {
            Error = false,
            Data = subjects
                .Select(
                    s =>
                        MapSubjectDto(
                            s,
                            paths.GetValueOrDefault(s.FolderId, s.Folder?.Name ?? string.Empty),
                            progressById.GetValueOrDefault(s.Id)
                        )
                )
                .ToList(),
            Message = "List of all subjects"
        };
    }

    public async Task<ActionResult<ResponseService<List<UserDTO>>>> GetAssignedUsers(int Id)
    {
        var subject = await context.Subjects
            .Where(p => !p.Archived && p.Id == Id)
            .Include(p => p.Users)
            .ThenInclude(u => u.Group)
            .Include(p => p.Users)
            .FirstOrDefaultAsync();

        if (subject is null)
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"Subject of id:{Id} is not found"
                }
            );

        var res = new List<UserDTO>();

        foreach (var user in subject.Users)
        {
            if (!user.Archived)
            {
                res.Add(
                    new UserDTO
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Group = user.Group == null
                            ? null
                            : new IDName
                            {
                                Id = user.GroupId ?? 0,
                                Name = user.Group.Name
                            },
                        Role = user.Role
                    }
                );
            }
        }

        return new ResponseService<List<UserDTO>>
        {
            Data = res,
            Error = false,
            Message = $"List of assigned users for subject of id:{subject.Id}"
        };
    }

    public async Task<ActionResult<ResponseService<SubjectDTO>>> GetProject(int Id)
    {
        var (user, userError) = await ResolveCurrentUserAsync();
        if (userError is not null)
            return userError;

        var subject = await context.Subjects
            .Where(p => p.Id == Id && !p.Archived)
            .Include(p => p.Folder)
            .FirstOrDefaultAsync();

        if (subject is null || !UserCanAccessSubject(user!, subject))
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Subject is not found" }
            );

        return new ResponseService<SubjectDTO>
        {
            Data = MapSubjectDto(subject, await BuildFolderPathAsync(subject.FolderId)),
            Error = false,
            Message = "Subject found"
        };
    }

    public async Task<ActionResult<ResponseService<DetailedProjectDTO>>> GetProjectDetails(int Id)
    {
        var (user, userError) = await ResolveCurrentUserAsync();
        if (userError is not null)
            return userError;

        var subject = await context.Subjects
            .Where(p => p.Id == Id && !p.Archived)
            .Include(p => p.Units)
            .ThenInclude(u => u.Lessons)
            .ThenInclude(l => l.LearningObjectives)
            .ThenInclude(lo => lo.Schema)
            .FirstOrDefaultAsync();

        if (subject is null || !UserCanAccessSubject(user!, subject))
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Subject is not found" }
            );

        return new ResponseService<DetailedProjectDTO>
        {
            Data = new DetailedProjectDTO
            {
                Id = subject.Id,
                Name = subject.Name,
                Description = subject.Description,
                Status = subject.Status,
                Units = subject.Units
                    .Where(u => !u.Archived)
                    .Select(
                        u =>
                            new ProjectUnitDTO
                            {
                                Id = u.Id,
                                Name = u.Name,
                                Lessons = u.Lessons
                                    .Where(l => !l.Archived)
                                    .Select(
                                        l =>
                                            new ProjectLessonDTO
                                            {
                                                Id = l.Id,
                                                Name = l.Name,
                                                LearningObjectives = l.LearningObjectives
                                                    .Where(lo =>
                                                        !lo.Archived
                                                        && (lo.Name == null
                                                            || !lo.Name.Contains(
                                                                "old",
                                                                StringComparison.OrdinalIgnoreCase)))
                                                    .Select(
                                                        lo =>
                                                            new LearningObjectiveDTO
                                                            {
                                                                Id = lo.Id,
                                                                Environment = lo.Environment,
                                                                Name = lo.Name,
                                                                Schema = new IDName
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

    public async Task<ActionResult<ResponseService<List<IDName>>>> GetProjectLearningObjectives(int Id)
    {
        var res = await learningObjectiveService.GetLearningObjectivesBySubjectId(Id);

        if (res.Error)
            return new NotFoundObjectResult(res);

        return new ResponseService<List<IDName>>
        {
            Data = res.Data!
                .Select(lo => new IDName { Id = lo.Id, Name = lo.Name })
                .ToList(),
            Error = false,
            Message = $"List of learning objective for subject of id:{Id}"
        };
    }

    public async Task<ActionResult<ResponseService<List<UserDTO>>>> GetUnassignedUsers(int Id)
    {
        var subject = await context.Subjects
            .Where(p => !p.Archived && p.Id == Id)
            .FirstOrDefaultAsync();

        if (subject is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Subject is not found" }
            );

        var users = await context.Users
            .Where(u => !u.Archived && !u.Subjects.Any(p => p.Id == subject.Id))
            .Include(u => u.Subjects)
            .Include(u => u.Group)
            .ToListAsync();

        return new ResponseService<List<UserDTO>>
        {
            Data = users
                .Select(
                    u =>
                        new UserDTO
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Role = u.Role,
                            Group = u.Group != null
                                ? new IDName
                                {
                                    Id = u.GroupId ?? 0,
                                    Name = u.Group.Name
                                }
                                : null
                        }
                )
                .ToList(),
            Error = false,
            Message = $"List of unassigned users for subject of id:{subject.Id}"
        };
    }

    public async Task<ActionResult<ResponseService<List<SubjectDTO>>>> GetUserSpecificProjects()
    {
        var (user, userError) = await ResolveCurrentUserAsync();
        if (userError is not null)
            return userError;

        user = await context.Users
            .Where(u => u.Id == user!.Id)
            .Include(u => u.Subjects)
            .ThenInclude(p => p.Folder)
            .FirstOrDefaultAsync();

        if (user is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "User is not found" }
            );

        if (CanViewAllSubjects(user.Role))
        {
            var allSubjects = await context.Subjects
                .Where(
                    p =>
                        !p.Archived
                        && p.Status != ProjectStatusEnum.Hold
                        && p.Status != ProjectStatusEnum.Closed
                )
                .Include(p => p.Folder)
                .ToListAsync();

            var subjectIds = allSubjects.Select(p => p.Id).ToList();

            var taskCounts = await context.Tasks
                .Where(t =>
                    !t.Archived &&
                    t.LearningObjective != null &&
                    t.LearningObjective.Lesson != null &&
                    t.LearningObjective.Lesson.Unit != null &&
                    subjectIds.Contains(t.LearningObjective.Lesson.Unit.SubjectId) &&
                    (t.Status == TaskStatusEnum.Backlog || t.Status == TaskStatusEnum.Doing
                     || t.Status == TaskStatusEnum.ToDo)
                )
                .GroupBy(t => t.LearningObjective.Lesson.Unit.SubjectId)
                .Select(g => new { SubjectId = g.Key, Count = g.Count() })
                .ToListAsync();

            var data = allSubjects
                .Select(p =>
                {
                    var dto = MapSubjectDto(p, p.Folder.Name);
                    dto.Count = taskCounts.FirstOrDefault(tc => tc.SubjectId == p.Id)?.Count ?? 0;
                    return dto;
                })
                .ToList();

            var progressById = await GetProgressBySubjectIdAsync(allSubjects.Select(p => p.Id));
            var paths = await BuildFolderPathsAsync(allSubjects.Select(p => p.FolderId));
            var levelNamesByFolderId = await BuildFolderLevelNamesMapAsync(allSubjects.Select(p => p.FolderId));
            foreach (var dto in data)
            {
                dto.ProgressPercent = progressById.GetValueOrDefault(dto.Id);
                dto.FolderPath = paths.GetValueOrDefault(dto.FolderId, dto.FolderPath);
                dto.LevelNames = levelNamesByFolderId.GetValueOrDefault(dto.FolderId, new List<string>());
            }

            return new ResponseService<List<SubjectDTO>>
            {
                Error = false,
                Message = "List of all subjects",
                Data = data
            };
        }

        var groups = new List<Group>();

        if (user.Role == UserRoleEnum.TeamLeader || user.Role == UserRoleEnum.SectionHead)
        {
            var userGroup = await context.Groups
                .Where(g => g.Id == user.GroupId)
                .FirstOrDefaultAsync();

            if (userGroup is null)
                return new NotFoundObjectResult(
                    new BaseResponseService { Error = true, Message = "User's group is not found" }
                );

            groups.Add(userGroup);

            if (user.Role == UserRoleEnum.SectionHead)
            {
                var section = await context.Sections
                    .Where(s => s.HeadId == user.Id && !s.Archived)
                    .FirstOrDefaultAsync();

                if (section is not null)
                {
                    var sectionGroupIds = await context.SectionGroups
                        .Where(sg => sg.SectionId == section.Id)
                        .Select(sg => sg.GroupId)
                        .ToListAsync();

                    var sectionGroups = await context.Groups
                        .Where(g => sectionGroupIds.Contains(g.Id))
                        .ToListAsync();

                    groups.AddRange(sectionGroups);
                }
            }
        }

        var userSubjects = user.Subjects
            .Where(p =>
                !p.Archived
                && p.Status != ProjectStatusEnum.Hold
                && p.Status != ProjectStatusEnum.Closed
            )
            .ToList();

        var userSubjectIds = userSubjects.Select(p => p.Id).ToList();

        IQueryable<Models.Task> baseTaskQuery = context.Tasks
            .Where(t =>
                !t.Archived &&
                t.GroupId == user.GroupId &&
                t.LearningObjective != null &&
                t.LearningObjective.Lesson != null &&
                t.LearningObjective.Lesson.Unit != null &&
                userSubjectIds.Contains(t.LearningObjective.Lesson.Unit.SubjectId) &&
                t.Status != TaskStatusEnum.Done
            );

        if (user.Role == UserRoleEnum.TeamLeader || user.Role == UserRoleEnum.SectionHead)
        {
            baseTaskQuery = baseTaskQuery.Where(t =>
                groups.Select(g => g.Id).Contains(t.GroupId)
            );
        }
        else
        {
            baseTaskQuery = baseTaskQuery.Where(t =>
                t.GroupId == user.GroupId &&
                (t.UserId == user.Id || t.Status == TaskStatusEnum.Backlog) &&
                (!t.TL || t.UserId == user.Id)
            );
        }

        var userTaskCounts = await baseTaskQuery
            .GroupBy(t => t.LearningObjective.Lesson.Unit.SubjectId)
            .Select(g => new { SubjectId = g.Key, Count = g.Count() })
            .ToListAsync();

        var listOfSubjects = userSubjects
            .Select(s =>
            {
                var dto = MapSubjectDto(s, s.Folder?.Name ?? string.Empty);
                dto.Count = userTaskCounts.FirstOrDefault(tc => tc.SubjectId == s.Id)?.Count ?? 0;
                return dto;
            })
            .ToList();

        var progressByIdForUser = await GetProgressBySubjectIdAsync(userSubjects.Select(p => p.Id));
        var pathsForUser = await BuildFolderPathsAsync(userSubjects.Select(p => p.FolderId));
        var levelNamesByFolderIdForUser = await BuildFolderLevelNamesMapAsync(userSubjects.Select(p => p.FolderId));
        foreach (var dto in listOfSubjects)
        {
            dto.ProgressPercent = progressByIdForUser.GetValueOrDefault(dto.Id);
            dto.FolderPath = pathsForUser.GetValueOrDefault(dto.FolderId, dto.FolderPath);
            dto.LevelNames = levelNamesByFolderIdForUser.GetValueOrDefault(dto.FolderId, new List<string>());
        }

        return new ResponseService<List<SubjectDTO>>
        {
            Error = false,
            Message = $"Subjects assigned to user of id:{user.Id}",
            Data = listOfSubjects
        };
    }

    public async Task<ActionResult<ResponseService<List<IDName>>>> UnassignToProject(int Id, List<int> UserIds)
    {
        var res = await projectAssignmentService.UnassignUsersToProject(Id, UserIds);

        if (res.Error)
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"Subject of id:{Id} is not found"
                }
            );

        return new ResponseService<List<IDName>>
        {
            Data = res.Data!
                .Select(u => new IDName { Id = u.Id, Name = u.Name })
                .ToList(),
            Error = false,
            Message = res.Message
        };
    }

    public async Task<ActionResult<ResponseService<SubjectDTO>>> UpdateProjectStatus(int id, ProjectStatusEnum status)
    {
        var subject = await context.Subjects
            .Where(p => !p.Archived && p.Id == id)
            .Include(p => p.Folder)
            .FirstOrDefaultAsync();

        if (subject is null)
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"Subject of id:{id} is not found"
                }
            );

        if (status == ProjectStatusEnum.Active)
        {
            if (
                subject.Status == ProjectStatusEnum.Active
                || subject.Status == ProjectStatusEnum.Reopened
            )
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Subject is already active" }
                );

            if (subject.Status == ProjectStatusEnum.Hold)
                subject.Status = ProjectStatusEnum.Active;
            else if (subject.Status == ProjectStatusEnum.Closed)
                subject.Status = ProjectStatusEnum.Reopened;
        }
        else if (status == ProjectStatusEnum.Hold)
        {
            if (subject.Status == ProjectStatusEnum.Hold)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Subject is already on Hold" }
                );

            if (subject.Status == ProjectStatusEnum.Closed)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Subject is Closed" }
                );

            subject.Status = ProjectStatusEnum.Hold;
        }
        else if (status == ProjectStatusEnum.Closed)
        {
            if (subject.Status == ProjectStatusEnum.Closed)
                return new BadRequestObjectResult(
                    new BaseResponseService { Error = true, Message = "Subject is already closed" }
                );

            subject.Status = ProjectStatusEnum.Closed;

            _ = await notificationService.NotifyOwnerOfProjectClosed(subject.Id, true);
        }

        await context.SaveChangesAsync();
        InvalidateAllSubjectsCache();

        return new ResponseService<SubjectDTO>
        {
            Message = "Subject status is updated",
            Error = false,
            Data = MapSubjectDto(subject, await BuildFolderPathAsync(subject.FolderId))
        };
    }

    private void InvalidateAllSubjectsCache() => cache.Remove("AllSubjects");
}
