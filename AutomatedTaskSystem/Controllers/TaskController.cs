using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Static;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutomatedTaskSystem.Services.PathService;
using AutomatedTaskSystem.Services.TaskService;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;

namespace AutomatedTaskSystem.Controllers;

[Route("tasks")]
[ApiController]
public class TaskController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPathService _pathService;
    private readonly ITaskService _taskService;

    public TaskController(
        DataContext context,
        ITokenService authService,
        IPathService pathService,
        ITaskService taskService
    )
    {
        _context = context;
        _tokenService = authService;
        _pathService = pathService;
        _taskService = taskService;
    }

    [HttpPatch("{id}/priority")]
    public async Task<ActionResult<ResponseService<Responses.ITaskDTO>>> EditTask(
        int id,
        Requests.PriorityUpdateDto req
    ) => await _taskService.UpdateTaskPriority(id, req.Priority);

    // POST:
    // Add Comment to Task
    [Authorize, HttpPost]
    public async Task<ActionResult<Responses.ITaskDTO>> AddTask(Requests.NewTaskDTO req)
    {
        var TaskBankItem = await _context.TaskBank
            .Where(g => g.Id == req.TaskBankItemId)
            .Include(tb => tb.Group)
            .FirstOrDefaultAsync();
        if (TaskBankItem == null)
            return BadRequest(new Responses.BadRequestsDTO("Group not found"));

        var lo = await _context.LearningObjectives
            .Where(lo => !lo.Archived && lo.Id == req.LearningObjectiveId)
            .Include(lo => lo.Lesson)
            .ThenInclude(l => l.Unit)
            .ThenInclude(u => u.Project)
            .FirstOrDefaultAsync();
        if (lo == null)
            return BadRequest(new Responses.BadRequestsDTO("Group not found"));

        var newTask = await _taskService.CreateTask(TaskBankItem, lo);

        if (req.UserId != 0)
        {
            var user = await _context.Users
                .Where(u => !u.Archived && u.Id == req.UserId)
                .FirstOrDefaultAsync();
            if (user == null)
                return BadRequest(new Responses.BadRequestsDTO("Group not found"));

            var status = await _context.Statuses
                .Where(s => s.Id == Statuses.ToDo)
                .FirstOrDefaultAsync();
            if (status == null)
                return BadRequest(
                    new Responses.BadRequestsDTO(
                        "I somehow failed to find ToDo Status in a lookup table"
                    )
                );

            newTask.UserId = user.Id;
            newTask.User = user;
            newTask.Status = status;
            newTask.StatusId = status.Id;
        }
        else
        {
            var status = await _context.Statuses
                .Where(s => s.Id == Statuses.Backlog)
                .FirstOrDefaultAsync();
            if (status == null)
                return BadRequest(
                    new Responses.BadRequestsDTO(
                        "I somehow failed to find Backlog Status in a lookup table"
                    )
                );
            newTask.Status = status;
            newTask.StatusId = status.Id;
        }

        _context.Tasks.Add(newTask);
        lo.Tasks.Add(newTask);

        await _context.SaveChangesAsync();

        return Ok(
            new Responses.ITaskDTO
            {
                Pause = newTask.Pause,
                LearningObjective = new Responses.IDName { Id = lo.Id, Name = lo.Name },
                Name = newTask.Name,
                IsReview = newTask.IsReview,
                Id = newTask.Id,
                Tag = lo.Tag,
                Schema = new Responses.IDName { Name = lo.Schema.Name, Id = lo.Schema.Id },
                Flagged = false,
                Status = newTask.Status.Name,
                Comments = new List<Responses.CommentDTO> { },
                Template = lo.Template,
                Environment = lo.Environment,
            }
        );
    }

    // POST:
    // Add comment to task
    [Authorize, HttpPost("{id}/comment")]
    public async Task<ActionResult<Responses.CommentDTO>> AddComment(
        int id,
        Requests.CommentDTO req
    )
    {
        try
        {
            var authRes = _tokenService.GetUserIdFromToken();
            if (authRes.Error)
                return BadRequest(
                    new BaseResponseService { Error = true, Message = authRes.Message }
                );
            var uid = Int64.Parse(authRes.Data!);
            var user = await _context.Users
                .Where(u => u.Id == uid && !u.Archived)
                .Include(u => u.Group)
                .FirstOrDefaultAsync();
            if (user == null)
                return Unauthorized(new Responses.BadRequestsDTO("Please login"));

            var task = await _context.Tasks
                .Where(t => t.Id == id && !t.Archived)
                .Include(t => t.Comments)
                .FirstOrDefaultAsync();

            if (task == null)
                return NotFound(new Responses.BadRequestsDTO("Task not found"));

            var newComment = new Comment
            {
                Task = task,
                TaskId = task.Id,
                Timestamp = DateTime.Now,
                Content = req.Comment,
                UserId = user.Id,
                User = user,
                Archived = false
            };

            _context.Comments.Add(newComment);
            task.Comments.Add(newComment);

            return Ok(
                new Responses.CommentDTO
                {
                    Id = newComment.Id,
                    User = new Responses.IDName { Id = user.Id, Name = user.Name },
                    Content = newComment.Content,
                    Timestamp = newComment.Timestamp
                }
            );
        }
        catch (System.Exception err)
        {
            Console.WriteLine(err.Message);
            return BadRequest(new Responses.BadRequestsDTO("Error occuried"));
        }
    }

    // GET:
    // Fetch currently assigned user to task (if any) and users that can work on the task
    [HttpGet("{id}/assigned")]
    public async Task<ActionResult<Responses.TaskAssignmentDTO>> GetAssignment(int id)
    {
        var task = await _context.Tasks
            .Where(t => t.Id == id && !t.Archived)
            .Include(t => t.User)
            .Include(t => t.LearningObjective)
            .ThenInclude(lo => lo.Lesson)
            .ThenInclude(l => l.Unit)
            .ThenInclude(u => u.Project)
            .ThenInclude(u => u.Users)
            .FirstOrDefaultAsync();

        if (task == null)
            return NotFound(new Responses.BadRequestsDTO("Task is not found"));

        var res = new Responses.TaskAssignmentDTO { };

        if (task.User != null)
            res.AssignedUser = new Responses.IDName { Id = task.User.Id, Name = task.User.Name };

        task.LearningObjective.Lesson.Unit.Project.Users.ForEach(u =>
        {
            if (u.GroupId == task.GroupId)
                res.AssignableUsers.Add(new Responses.IDName { Id = u.Id, Name = u.Name });
        });

        return res;
    }

    // GET:
    // Get one Task
    [HttpGet("{id}")]
    public async Task<ActionResult<Responses.ITaskDTO>> GetTask(int id)
    {
        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == id)
            .Include(t => t.Group)
            .Include(t => t.From)
            .Include(t => t.Status)
            .Include(t => t.LearningObjective)
            .ThenInclude(lo => lo.Schema)
            .Include(t => t.User)
            .Include(t => t.Comments)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync();

        if (task == null)
            return NotFound(new Responses.BadRequestsDTO("Task not found"));

        var comments = new List<Responses.CommentDTO> { };

        foreach (var item in task.Comments)
            comments.Add(
                new Responses.CommentDTO
                {
                    User = new Responses.IDName { Name = item.User.Name, Id = item.UserId },
                    Id = item.Id,
                    Content = item.Content,
                    Timestamp = item.Timestamp
                }
            );

        var started = await _context.Activities
            .Where(a => a.TaskId == task.Id && a.ActivityTypeId == 1)
            .OrderBy(a => a.TimeStamp)
            .LastOrDefaultAsync();

        var done = await _context.Activities
            .Where(a => a.TaskId == task.Id && a.ActivityTypeId == 2)
            .OrderBy(a => a.TimeStamp)
            .LastOrDefaultAsync();

        var res = new Responses.ITaskDTO
        {
            Pause = task.Pause,
            StartedAt = started != null ? started.TimeStamp : null,
            DoneAt = done != null ? done.TimeStamp : null,
            Id = task.Id,
            IsReview = task.IsReview,
            LearningObjective = new Responses.IDName
            {
                Name = task.LearningObjective.Name,
                Id = task.LearningObjective.Id
            },
            Schema = new Responses.IDName
            {
                Name = task.LearningObjective.Schema.Name,
                Id = task.LearningObjectiveId
            },
            Name = task.Name,
            Status = task.Status.Name,
            Flagged = task.Flagged,
            Tag = task.LearningObjective.Tag,
            Environment = task.LearningObjective.Environment,
            Template = task.LearningObjective.Template,
            Comments = comments,
            Priority = task.Priority
        };
        return res;
    }

    // GET:
    // Get Project Tasks
    [Authorize]
    [HttpGet("projects/{id}")]
    public async Task<ActionResult<List<Responses.TaskDTO>>> GetTasks(int id)
    {
        try
        {
            var authRes = _tokenService.GetUserIdFromToken();
            if (authRes.Error)
                return BadRequest(
                    new BaseResponseService { Error = true, Message = authRes.Message }
                );
            var uid = Int64.Parse(authRes.Data!);
            var user = await _context.Users
                .Where(u => u.Id == uid && !u.Archived)
                .Include(u => u.Group)
                .FirstOrDefaultAsync();
            if (user == null)
                return Unauthorized(new Responses.BadRequestsDTO("Please login"));

            var project = await _context.Projects
                .Where(p => p.Id == id && !p.Archived)
                .Include(p => p.Units)
                .ThenInclude(u => u.Lessons)
                .ThenInclude(l => l.LearningObjectives)
                .ThenInclude(lo => lo.Tasks)
                .ThenInclude(t => t.Group)
                .Include(p => p.Units)
                .ThenInclude(u => u.Lessons)
                .ThenInclude(l => l.LearningObjectives)
                .ThenInclude(lo => lo.Tasks)
                .ThenInclude(t => t.Status)
                .Include(p => p.Units)
                .ThenInclude(u => u.Lessons)
                .ThenInclude(l => l.LearningObjectives)
                .ThenInclude(lo => lo.Tasks)
                .Include(p => p.Units)
                .ThenInclude(u => u.Lessons)
                .ThenInclude(l => l.LearningObjectives)
                .ThenInclude(lo => lo.Tasks)
                .ThenInclude(t => t.User)
                .Include(p => p.Units)
                .ThenInclude(u => u.Lessons)
                .ThenInclude(l => l.LearningObjectives)
                .ThenInclude(lo => lo.Tasks)
                .ThenInclude(t => t.Comments)
                .ThenInclude(c => c.User)
                .Include(p => p.Units)
                .ThenInclude(u => u.Lessons)
                .ThenInclude(l => l.LearningObjectives)
                .ThenInclude(lo => lo.Tasks)
                .ThenInclude(t => t.From)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (project == null)
                return NotFound(new Responses.BadRequestsDTO("Project not found"));

            var res = new List<Responses.TaskDTO> { };

            if (user.RoleId == Roles.PM)
            {
                foreach (var unit in project.Units)
                    foreach (var lesson in unit.Lessons)
                        foreach (var lo in lesson.LearningObjectives)
                            foreach (var task in lo.Tasks)
                                if (!task.Archived)
                                {
                                    var comments = new List<Responses.CommentDTO> { };
                                    foreach (var comment in task.Comments)
                                        comments.Add(
                                            new Responses.CommentDTO
                                            {
                                                Id = comment.Id,
                                                User = new Responses.IDName
                                                {
                                                    Id = comment.UserId,
                                                    Name = comment.User.Name
                                                },
                                                Content = comment.Content,
                                                Timestamp = comment.Timestamp
                                            }
                                        );
                                    res.Add(
                                        new Responses.TaskDTO
                                        {
                                            Attention = task.Attention,
                                            Flagged = task.Flagged,
                                            Id = task.Id,
                                            IsReview = task.IsReview,
                                            Name = task.Name,
                                            Status = task.Status.Name,
                                            TL = task.TL,
                                            IsRollback = task.IsRollback,
                                            RollbackCount = task.RollbackCount,
                                            From = task.From is null ? "" : task.From.Name,
                                            Priority = task.Priority,
                                            Comments = comments,
                                            LearningObjective = new Responses.IDName
                                            {
                                                Id = task.LearningObjectiveId,
                                                Name = task.LearningObjective.Name
                                            }
                                        }
                                    );
                                }
                return res;
            }
            else if (user.RoleId == Roles.TL)
            {
                foreach (var unit in project.Units)
                {
                    foreach (var lesson in unit.Lessons)
                    {
                        foreach (var lo in lesson.LearningObjectives)
                        {
                            foreach (var task in lo.Tasks)
                            {
                                if (!task.Archived)
                                {
                                    var comments = new List<Responses.CommentDTO> { };
                                    foreach (var comment in task.Comments)
                                        comments.Add(
                                            new Responses.CommentDTO
                                            {
                                                Id = comment.Id,
                                                User = new Responses.IDName
                                                {
                                                    Name = comment.User.Name,
                                                    Id = comment.User.Id
                                                },
                                                Content = comment.Content,
                                                Timestamp = comment.Timestamp
                                            }
                                        );
                                    if (task.GroupId == user.GroupId)
                                        res.Add(
                                            new Responses.TaskDTO
                                            {
                                                Attention = task.Attention,
                                                Flagged = task.Flagged,
                                                Id = task.Id,
                                                IsReview = task.IsReview,
                                                Name = task.Name,
                                                Status = task.Status.Name,
                                                TL = task.TL,
                                                IsRollback = task.IsRollback,
                                                RollbackCount = task.RollbackCount,
                                                From = task.From is null ? "" : task.From.Name,
                                                Priority = task.Priority,
                                                Comments = comments,
                                                LearningObjective = new Responses.IDName
                                                {
                                                    Id = task.LearningObjectiveId,
                                                    Name = task.LearningObjective.Name
                                                }
                                            }
                                        );
                                }
                            }
                        }
                    }
                }
                return res;
            }
            else if (user.RoleId == Roles.SH)
            {
                var section = await _context.Sections
                    .Include(s => s.Groups)
                    .Where(s => s.HeadId == user.Id)
                    .FirstOrDefaultAsync();

                if (section != null)
                {
                    foreach (var unit in project.Units)
                    {
                        foreach (var lesson in unit.Lessons)
                        {
                            foreach (var lo in lesson.LearningObjectives)
                            {
                                foreach (var task in lo.Tasks)
                                {
                                    if (!task.Archived)
                                    {
                                        var comments = new List<Responses.CommentDTO> { };
                                        foreach (var comment in task.Comments)
                                            comments.Add(
                                                new Responses.CommentDTO
                                                {
                                                    Id = comment.Id,
                                                    User = new Responses.IDName
                                                    {
                                                        Id = comment.Id,
                                                        Name = comment.User.Name
                                                    },
                                                    Content = comment.Content,
                                                    Timestamp = comment.Timestamp
                                                }
                                            );
                                        var groupsList = new List<int> { };

                                        foreach (var group in section.Groups)
                                            groupsList.Add(group.Id);

                                        if (
                                            task.GroupId == user.GroupId
                                            || groupsList.Contains(task.GroupId)
                                        )
                                            res.Add(
                                                new Responses.TaskDTO
                                                {
                                                    Attention = task.Attention,
                                                    Flagged = task.Flagged,
                                                    Id = task.Id,
                                                    IsReview = task.IsReview,
                                                    Name = task.Name,
                                                    Status = task.Status.Name,
                                                    TL = task.TL,
                                                    IsRollback = task.IsRollback,
                                                    RollbackCount = task.RollbackCount,
                                                    From = task.From is null ? "" : task.From.Name,
                                                    Priority = task.Priority,
                                                    Comments = comments,
                                                    LearningObjective = new Responses.IDName
                                                    {
                                                        Id = task.LearningObjectiveId,
                                                        Name = task.LearningObjective.Name
                                                    }
                                                }
                                            );
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var unit in project.Units)
            {
                foreach (var lesson in unit.Lessons)
                {
                    foreach (var lo in lesson.LearningObjectives)
                    {
                        foreach (var task in lo.Tasks)
                        {
                            if (!task.Archived)
                            {
                                var comments = new List<Responses.CommentDTO> { };
                                foreach (var comment in task.Comments)
                                    comments.Add(
                                        new Responses.CommentDTO
                                        {
                                            Id = comment.Id,
                                            User = new Responses.IDName
                                            {
                                                Id = comment.User.Id,
                                                Name = comment.User.Name
                                            },
                                            Content = comment.Content,
                                            Timestamp = comment.Timestamp
                                        }
                                    );
                                if (
                                    task.GroupId == user.GroupId
                                    && (task.StatusId == 1 || task.UserId == user.Id)
                                )
                                    res.Add(
                                        new Responses.TaskDTO
                                        {
                                            Attention = task.Attention,
                                            Flagged = task.Flagged,
                                            Id = task.Id,
                                            IsReview = task.IsReview,
                                            Name = task.Name,
                                            Status = task.Status.Name,
                                            TL = task.TL,
                                            IsRollback = task.IsRollback,
                                            RollbackCount = task.RollbackCount,
                                            From = task.From is null ? "" : task.From.Name,
                                            Priority = task.Priority,
                                            Comments = comments,
                                            LearningObjective = new Responses.IDName
                                            {
                                                Id = task.LearningObjectiveId,
                                                Name = task.LearningObjective.Name
                                            }
                                        }
                                    );
                            }
                        }
                    }
                }
            }
            return res;
        }
        catch (System.Exception err)
        {
            Console.WriteLine(err.Message);
            return BadRequest(new Responses.BadRequestsDTO("Error occuried"));
        }
    }

    // POST:
    // Update Task's Status to Todo
    [HttpPost("todo/{id}")]
    [Authorize]
    public async Task<ActionResult<Responses.ITaskDTO>> MoveToTodo(int id)
    {
        try
        {
            var authRes = _tokenService.GetUserIdFromToken();
            if (authRes.Error)
                return BadRequest(
                    new BaseResponseService { Error = true, Message = authRes.Message }
                );
            var uid = Int64.Parse(authRes.Data!);
            var user = await _context.Users
                .Where(u => u.Id == uid && !u.Archived)
                .Include(u => u.Group)
                .FirstOrDefaultAsync();
            if (user == null)
                return Unauthorized(new Responses.BadRequestsDTO("Please login"));
            var task = await _context.Tasks
                .Where(t => !t.Archived && t.Id == id)
                .Include(t => t.Group)
                .Include(t => t.Status)
                .Include(t => t.LearningObjective)
                .Include(t => t.User)
                .FirstOrDefaultAsync();

            if (task == null)
                return NotFound(new Responses.BadRequestsDTO("Task not found"));

            if (task.StatusId != Statuses.Backlog)
                return BadRequest(new Responses.BadRequestsDTO("Task is not in Backlog"));

            var ToDoStatus = await _context.Statuses.FindAsync(Statuses.ToDo);
            task.Status = ToDoStatus!;
            task.StatusId = Statuses.ToDo;
            task.User = user;
            task.UserId = user.Id;

            var newAssignemt = new Assignment
            {
                TaskId = task.Id,
                Task = task,
                By = user,
                To = user,
                ById = user.Id,
                ToId = user.Id
            };

            _context.Assignments.Add(newAssignemt);

            await _context.SaveChangesAsync();

            return await GetTask(task.Id);
        }
        catch (System.Exception err)
        {
            Console.WriteLine(err.Message);
            return BadRequest(new Responses.BadRequestsDTO("Error occuried"));
        }
    }

    // POST:
    // Update Task's Status to Doing
    [Authorize]
    [HttpPost("doing/{id}")]
    public async Task<ActionResult<Responses.ITaskDTO>> MoveToDoing(int id)
    {
        try
        {
            var authRes = _tokenService.GetUserIdFromToken();
            if (authRes.Error)
                return BadRequest(
                    new BaseResponseService { Error = true, Message = authRes.Message }
                );
            var uid = Int64.Parse(authRes.Data!);
            var user = await _context.Users
                .Where(u => u.Id == uid && !u.Archived)
                .Include(u => u.Group)
                .FirstOrDefaultAsync();
            if (user == null)
                return Unauthorized(new Responses.BadRequestsDTO("Please login"));

            var task = await _context.Tasks
                .Where(t => !t.Archived && t.Id == id)
                .Include(t => t.Group)
                .Include(t => t.Status)
                .Include(t => t.LearningObjective)
                .Include(t => t.User)
                .FirstOrDefaultAsync();

            if (task == null)
                return NotFound(new Responses.BadRequestsDTO("Task not found"));
            if (task.UserId != null && task.UserId != user.Id)
                return BadRequest(new Responses.BadRequestsDTO("Task is not Assigned for you"));
            if (task.StatusId != Statuses.ToDo)
                return BadRequest(new Responses.BadRequestsDTO("Task is not in Todo"));

            if (task.Attention)
                task.Attention = false;

            var DoingStatus = await _context.Statuses.FindAsync(Statuses.Doing);
            task.Status = DoingStatus!;
            task.StatusId = Statuses.Doing;

            var startAct = await _context.ActivityTypes.FindAsync(1);

            if (startAct == null)
                return NotFound(new Responses.BadRequestsDTO("I somehow failed"));

            var newEA = new EndActivity
            {
                Task = task,
                TaskId = task.Id,
                User = user,
                UserId = user.Id,
                EndActivityType = null,
                EndActivityTypeId = null,
                StartDate = DateTime.Now,
                EndDate = null
            };

            var newAct = new Activity
            {
                Task = task,
                User = user,
                TaskId = task.Id,
                UserId = user.Id,
                ActivityType = startAct,
                ActivityTypeId = startAct.Id
            };

            _context.Activities.Add(newAct);
            _context.EndActivities.Add(newEA);

            await _context.SaveChangesAsync();

            return await GetTask(task.Id);
        }
        catch (System.Exception err)
        {
            Console.WriteLine(err.Message);
            return BadRequest(new Responses.BadRequestsDTO("Error occuried"));
        }
    }

    // POST:
    // Update Task's Status to Done
    [Authorize]
    [HttpPost("done/{id}")]
    public async Task<ActionResult<Responses.ITaskDTO>> CompleteTask(int id)
    {
        try
        {
            var authRes = _tokenService.GetUserIdFromToken();
            if (authRes.Error)
                return BadRequest(
                    new BaseResponseService { Error = true, Message = authRes.Message }
                );
            var uid = Int64.Parse(authRes.Data!);
            var user = await _context.Users
                .Where(u => u.Id == uid && !u.Archived)
                .Include(u => u.Group)
                .FirstOrDefaultAsync();
            if (user == null)
                return Unauthorized(new Responses.BadRequestsDTO("Please login"));
            var task = await _context.Tasks
                .Where(t => !t.Archived && t.Id == id)
                .Include(t => t.Group)
                .Include(t => t.From)
                .Include(t => t.Status)
                .Include(t => t.LearningObjective)
                .Include(t => t.User)
                .FirstOrDefaultAsync();
            if (task == null)
                return NotFound(new Responses.BadRequestsDTO("Task not found"));
            if (task.UserId != null && task.UserId != user.Id)
                return BadRequest(new Responses.BadRequestsDTO("Task is not Assigned for you"));
            if (task.StatusId != Statuses.Doing && task.StatusId != Statuses.Done)
                return BadRequest(new Responses.BadRequestsDTO("Line 802: Task is not in Doing"));

            var DoneStatus = await _context.Statuses.FindAsync(Statuses.Done);
            task.Status = DoneStatus!;
            task.StatusId = Statuses.Done;

            var doneAct = await _context.ActivityTypes.FindAsync(2);

            if (doneAct == null)
                return NotFound(new Responses.BadRequestsDTO("I somehow failed"));

            var newAct = new Activity
            {
                Task = task,
                User = user,
                TaskId = task.Id,
                UserId = user.Id,
                ActivityType = doneAct,
                ActivityTypeId = doneAct.Id
            };

            _context.Activities.Add(newAct);

            var completeEA = await _context.EndActivityTypes.FindAsync(3);

            if (completeEA != null)
            {
                var currentEA = await _context.EndActivities
                    .Where(
                        _ =>
                            _.UserId == user.Id
                            && _.TaskId == task.Id
                            && _.EndDate == null
                            && _.EndActivityTypeId == null
                    )
                    .FirstOrDefaultAsync();

                if (currentEA != null)
                {
                    currentEA.EndActivityTypeId = completeEA.Id;
                    currentEA.EndActivityType = completeEA;
                    currentEA.EndDate = DateTime.Now;
                }
            }

            task.From = null;

            var paths = await _pathService.GetPathByTask(task);

            foreach (var path in paths)
            {
                var nextStep = path.NextStep;

                if (nextStep.Order == 1)
                {
                    var loPaths = await _pathService.GetPathByLearningObjective(
                        task.LearningObjective
                    );

                    var node = await _context.Nodes
                        .Where(n => n.Id == nextStep.NodeId && !n.Archived)
                        .Include(n => n.Requires)
                        .ThenInclude(n => n.Steps)
                        .FirstOrDefaultAsync();

                    if (node is null)
                        throw new NotImplementedException("NODE IS NULL");

                    bool isReady = true;
                    foreach (var requiredNode in node.Requires)
                    {
                        var lastStep = requiredNode.Steps
                            .Where(s => s.Order == requiredNode.Steps.Count)
                            .FirstOrDefault();

                        if (lastStep is null)
                            throw new NotImplementedException("BAD IMPLEMENTATION");

                        var lastStepPaths = loPaths.Where(p => p.StepId == lastStep.Id).ToList();

                        foreach (var item in lastStepPaths)
                        {
                            if (
                                !(
                                    item.Task is not null
                                    && (item.Task.StatusId == 4 || item.Task.StatusId == 5)
                                )
                            )
                            {
                                isReady = false;
                                break;
                            }
                        }
                    }

                    if (isReady)
                    {
                        var newTask = await _taskService.CreateTaskWithStep(
                            nextStep,
                            task.LearningObjective
                        );
                        await _pathService.UpdatePathTask(newTask, nextStep);
                    }
                }
                else
                {
                    var newTask = await _taskService.CreateTaskWithStep(
                        nextStep,
                        task.LearningObjective
                    );
                    await _pathService.UpdatePathTask(newTask, nextStep);
                }
            }

            await _context.SaveChangesAsync();

            return await GetTask(task.Id);
        }
        catch (System.Exception err)
        {
            Console.Error.WriteLine(err.Message);
            return BadRequest(new Responses.BadRequestsDTO("Error occuried"));
        }
    }

    // POST:
    // Approve's Status to Done
    [Authorize]
    [HttpPost("approve/{id}")]
    public async Task<ActionResult<Responses.ITaskDTO>> ApproveTask(int id)
    {
        try
        {
            var authRes = _tokenService.GetUserIdFromToken();
            if (authRes.Error)
                return BadRequest(
                    new BaseResponseService { Error = true, Message = authRes.Message }
                );
            var uid = Int64.Parse(authRes.Data!);
            var user = await _context.Users
                .Where(u => u.Id == uid && !u.Archived)
                .Include(u => u.Group)
                .FirstOrDefaultAsync();
            if (user == null)
                return Unauthorized(new Responses.BadRequestsDTO("Please login"));
            var task = await _context.Tasks
                .Where(t => !t.Archived && t.Id == id)
                .Include(t => t.Group)
                .Include(t => t.Status)
                .Include(t => t.LearningObjective)
                .Include(t => t.User)
                .FirstOrDefaultAsync();
            if (task == null)
                return NotFound(new Responses.BadRequestsDTO("Task not found"));

            if (task.StatusId != Statuses.Doing && task.StatusId != Statuses.Done)
                return BadRequest(new Responses.BadRequestsDTO("Task is not in Doing"));
            if (!task.IsReview)
                return BadRequest(new Responses.BadRequestsDTO("Task is not in a Reviewable"));

            var done = await _context.Statuses.FindAsync(4);
            if (done != null)
            {
                task.StatusId = 4;
                task.Status = done;

                var doneAct = await _context.ActivityTypes.FindAsync(2);

                if (doneAct == null)
                    return NotFound(new Responses.BadRequestsDTO("I somehow failed"));

                var newAct = new Activity
                {
                    Task = task,
                    User = user,
                    TaskId = task.Id,
                    UserId = user.Id,
                    ActivityType = doneAct,
                    ActivityTypeId = doneAct.Id
                };

                _context.Activities.Add(newAct);

                await _context.SaveChangesAsync();
            }

            return await CompleteTask(id);
        }
        catch (System.Exception err)
        {
            Console.WriteLine(err.Message);
            return BadRequest(new Responses.BadRequestsDTO("Error occuried"));
        }
    }

    // POST:
    // Update Task's Status to Doing
    [Authorize]
    [HttpPost("rollback/{id}")]
    public async Task<ActionResult<Responses.ITaskDTO>> RollbackTask(
        int id,
        Requests.RollbackDTO req
    )
    {
        try
        {
            var authRes = _tokenService.GetUserIdFromToken();
            if (authRes.Error)
                return BadRequest(
                    new BaseResponseService { Error = true, Message = authRes.Message }
                );
            var uid = Int64.Parse(authRes.Data!);
            var user = await _context.Users
                .Where(u => u.Id == uid && !u.Archived)
                .Include(u => u.Group)
                .FirstOrDefaultAsync();
            if (user == null)
                return Unauthorized(new Responses.BadRequestsDTO("Please login"));

            var task = await _context.Tasks
                .Where(t => !t.Archived && t.Id == id)
                .Include(t => t.Group)
                .Include(t => t.Status)
                .Include(t => t.LearningObjective)
                .Include(t => t.User)
                .FirstOrDefaultAsync();

            if (task == null)
                return NotFound(new Responses.BadRequestsDTO("Task not found"));

            var rollbackStep = await _context.Steps
                .Where(s => s.Id == req.StepId)
                .Include(s => s.Node)
                .Include(s => s.TaskBank)
                .ThenInclude(tb => tb.Group)
                .FirstOrDefaultAsync();

            if (rollbackStep is null)
                return BadRequest(new Responses.BadRequestsDTO("Step is not found."));

            if (task.StatusId != Statuses.Doing && task.StatusId != Statuses.Done)
                return BadRequest(new Responses.BadRequestsDTO("Task is not in Doing"));
            if (!task.IsReview)
                return BadRequest(new Responses.BadRequestsDTO("Task is not in a Reviewable"));

            var RollbackStatus = await _context.Statuses.FindAsync(Statuses.Rollback);
            if (RollbackStatus == null)
                return NotFound(
                    new Responses.BadRequestsDTO("Done status is not found (Contact the tech)")
                );
            task.Status = RollbackStatus;
            task.StatusId = Statuses.Rollback;

            var doneAct = await _context.ActivityTypes.FindAsync(2);
            var completeEA = await _context.EndActivityTypes.FindAsync(3);
            if (completeEA != null)
            {
                var currentEA = await _context.EndActivities
                    .Where(
                        _ =>
                            _.UserId == user.Id
                            && _.TaskId == task.Id
                            && _.EndDate == null
                            && _.EndActivityTypeId == null
                    )
                    .FirstOrDefaultAsync();

                if (currentEA != null)
                {
                    currentEA.EndActivityTypeId = completeEA.Id;
                    currentEA.EndActivityType = completeEA;
                    currentEA.EndDate = DateTime.Now;
                }
            }
            if (doneAct == null)
                return NotFound(new Responses.BadRequestsDTO("I somehow failed"));

            var newAct = new Activity
            {
                Task = task,
                User = user,
                TaskId = task.Id,
                UserId = user.Id,
                ActivityType = doneAct,
                ActivityTypeId = doneAct.Id
            };

            _context.Activities.Add(newAct);

            await _context.SaveChangesAsync();

            await _pathService.GeneratePathFromStartPoint(rollbackStep, task.LearningObjective);
            var newTask = await _taskService.CreateTaskWithStep(
                rollbackStep,
                task.LearningObjective,
                task
            );
            await _pathService.UpdatePathTask(newTask, rollbackStep);

            await _context.SaveChangesAsync();

            return await GetTask(task.Id);
        }
        catch (System.Exception err)
        {
            Console.WriteLine(err);
            Console.WriteLine(err.Message);
            return BadRequest(new Responses.BadRequestsDTO("Error occuried"));
        }
    }

    // GET:
    // Returns list of previous nodes and steps
    [Authorize]
    [HttpGet("previous/{taskId}")]
    public async Task<ActionResult<List<Responses.IDName>>> GetPreviousTasks(int taskId)
    {
        var task = await _context.Tasks
            .Where(t => t.Id == taskId)
            .Include(t => t.Status)
            .Include(t => t.Step)
            .FirstOrDefaultAsync();

        if (task == null)
            return BadRequest(new Responses.BadRequestsDTO("Task not found"));

        var res = new List<Responses.IDName> { };

        if (task.Step is null)
            return Ok(res);

        var schema = await _context.Schemas
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Steps)
            .ThenInclude(s => s.TaskBank)
            .ThenInclude(tb => tb.Group)
            .Include(s => s.Nodes)
            .ThenInclude(n => n.Previous)
            .Where(s => !s.Archived && s.Nodes.Any(n => !n.Archived && n.Id == task.Step.NodeId))
            .FirstOrDefaultAsync();

        if (schema is null)
            return Ok(res);

        var nodes = new List<Node>
        {
            schema.Nodes.Where(n => !n.Archived && n.Steps.Any(s => s.Id == task.StepId)).First()
        };

        while (true)
        {
            var prevList = new List<Node> { };

            foreach (var item in nodes)
                foreach (var prev in item.Previous)
                {
                    var check = nodes.Any(n => !n.Archived && n.Id == prev.Id);
                    if (!check)
                        prevList.Add(prev);
                }

            if (prevList.Count == 0)
                break;

            foreach (var item in prevList)
                nodes.Add(item);
        }

        foreach (var item in nodes)
            foreach (var step in item.Steps)
                if (
                    !step.Archived
                    && step.TaskBank.TypeId != 3
                    && (
                        step.NodeId != task.Step.NodeId
                        || (step.NodeId == task.Step.NodeId && step.Order < task.Step.Order)
                    )
                )
                    res.Add(new Responses.IDName { Id = step.Id, Name = step.TaskBank.Name });

        return Ok(res);
    }

    // POST:
    // Assign Task to user
    [Authorize]
    [HttpPost("{id}/assign")]
    public async Task<ActionResult<Responses.SuccessDTO>> AssignUser(
        int id,
        Requests.TaskUserAssignDTO req
    )
    {
        try
        {
            var authRes = _tokenService.GetUserIdFromToken();
            if (authRes.Error)
                return BadRequest(
                    new BaseResponseService { Error = true, Message = authRes.Message }
                );
            var uid = Int64.Parse(authRes.Data!);
            var activeUser = await _context.Users
                .Where(u => u.Id == uid && !u.Archived)
                .Include(u => u.Group)
                .FirstOrDefaultAsync();
            if (activeUser == null)
                return Unauthorized(new Responses.BadRequestsDTO("Please login"));

            var task = await _context.Tasks.Where(t => t.Id == id).FirstOrDefaultAsync();

            if (task == null)
                return NotFound(new Responses.BadRequestsDTO("Task Not Found"));

            var newAssignment = new Assignment { };

            var reassignEA = await _context.EndActivityTypes.FindAsync(5);

            if (reassignEA != null)
            {
                var currentEA = await _context.EndActivities
                    .Where(
                        _ =>
                            _.UserId == task.UserId
                            && _.TaskId == task.Id
                            && _.EndDate == null
                            && _.EndActivityTypeId == null
                    )
                    .FirstOrDefaultAsync();

                if (currentEA != null)
                {
                    currentEA.EndActivityTypeId = reassignEA.Id;
                    currentEA.EndActivityType = reassignEA;
                    currentEA.EndDate = DateTime.Now;
                }
            }

            if (req.UserId != 0)
            {
                var user = await _context.Users
                    .Where(u => u.Id == req.UserId && !u.Archived)
                    .FirstOrDefaultAsync();

                if (user == null)
                    return NotFound(new Responses.BadRequestsDTO("User not found"));

                var TodoStatus = await _context.Statuses
                    .Where(s => s.Id == 2)
                    .FirstOrDefaultAsync();

                if (TodoStatus == null)
                    return NotFound(new Responses.BadRequestsDTO("Status error"));

                task.User = user;
                task.UserId = user.Id;
                task.Status = TodoStatus;
                task.StatusId = TodoStatus.Id;

                newAssignment.TaskId = task.Id;
                newAssignment.Task = task;
                newAssignment.By = activeUser;
                newAssignment.ById = activeUser.Id;
                newAssignment.To = user;
                newAssignment.ToId = user.Id;

                _context.Assignments.Add(newAssignment);
                await _context.SaveChangesAsync();

                return Ok(new Responses.SuccessDTO("User assigned to task"));
            }

            var BacklogStatus = await _context.Statuses
                .Where(s => s.Id == Statuses.Backlog)
                .FirstOrDefaultAsync();

            if (BacklogStatus == null)
                return NotFound(new Responses.BadRequestsDTO("Status error"));

            task.User = null;
            task.UserId = null;
            task.Status = BacklogStatus;
            task.StatusId = BacklogStatus.Id;

            newAssignment.TaskId = task.Id;
            newAssignment.Task = task;
            newAssignment.By = activeUser;
            newAssignment.ById = activeUser.Id;
            newAssignment.To = null;
            newAssignment.ToId = null;

            _context.Assignments.Add(newAssignment);

            await _context.SaveChangesAsync();

            return Ok(new Responses.SuccessDTO("User unassigned from task"));
        }
        catch (System.Exception err)
        {
            Console.WriteLine(err.Message);
            return BadRequest(new Responses.BadRequestsDTO("Error occuried"));
        }
    }

    // POST:
    // Flag Task
    [HttpPost("flag/{id}")]
    public async Task<ActionResult<Responses.ITaskDTO>> FlagTask(int id)
    {
        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == id)
            .Include(t => t.Group)
            .Include(t => t.LearningObjective)
            .Include(t => t.Status)
            .Include(t => t.User)
            .FirstOrDefaultAsync();
        if (task == null)
            return NotFound(new Responses.BadRequestsDTO("Task not found"));

        task.Flagged = true;

        var flagEA = await _context.EndActivityTypes.FindAsync(1);

        if (flagEA != null)
        {
            var currentEA = await _context.EndActivities
                .Where(
                    _ =>
                        _.UserId == task.UserId
                        && _.TaskId == task.Id
                        && _.EndDate == null
                        && _.EndActivityTypeId == null
                )
                .FirstOrDefaultAsync();

            if (currentEA != null)
            {
                currentEA.EndActivityTypeId = flagEA.Id;
                currentEA.EndActivityType = flagEA;
                currentEA.EndDate = DateTime.Now;
            }
        }

        await _context.SaveChangesAsync();

        return await GetTask(task.Id);
    }

    // POST:
    // Flag Task
    [HttpPost("unflag/{id}")]
    public async Task<ActionResult<Responses.ITaskDTO>> UnflagTask(int id)
    {
        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == id)
            .Include(t => t.Group)
            .Include(t => t.LearningObjective)
            .Include(t => t.Status)
            .Include(t => t.User)
            .FirstOrDefaultAsync();
        if (task == null)
        {
            return NotFound(new Responses.BadRequestsDTO("Task not found"));
        }

        task.Flagged = false;
        task.Attention = true;

        var status = await _context.Statuses
            .Where(s => s.Id == Statuses.ToDo)
            .FirstOrDefaultAsync();
        if (status == null)
            return BadRequest(
                new Responses.BadRequestsDTO(
                    "I somehow failed to find ToDo Status in a lookup table"
                )
            );
        task.Status = status;
        task.StatusId = 2;
        task.Attention = true;

        await _context.SaveChangesAsync();

        return await GetTask(task.Id);
    }

    // POST:
    // Pause Task
    [HttpPost("{id}/pause")]
    public async Task<ActionResult<Responses.ITaskDTO>> PauseTask(int id)
    {
        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == id)
            .Include(t => t.Group)
            .Include(t => t.LearningObjective)
            .Include(t => t.Status)
            .Include(t => t.User)
            .FirstOrDefaultAsync();
        if (task == null)
            return NotFound(new Responses.BadRequestsDTO("Task not found"));

        task.Pause = true;

        if (task.StatusId != 2)
        {
            var status = await _context.Statuses
                .Where(s => s.Id == Statuses.ToDo)
                .FirstOrDefaultAsync();
            if (status == null)
                return BadRequest(
                    new Responses.BadRequestsDTO(
                        "I somehow failed to find ToDo Status in a lookup table"
                    )
                );
            task.Status = status;
            task.StatusId = status.Id;
        }

        var pauseEA = await _context.EndActivityTypes.FindAsync(2);
        if (pauseEA != null)
        {
            var currentEA = await _context.EndActivities
                .Where(
                    _ =>
                        _.UserId == task.UserId
                        && _.TaskId == task.Id
                        && _.EndDate == null
                        && _.EndActivityTypeId == null
                )
                .FirstOrDefaultAsync();

            if (currentEA != null)
            {
                currentEA.EndActivityTypeId = pauseEA.Id;
                currentEA.EndActivityType = pauseEA;
                currentEA.EndDate = DateTime.Now;
            }
        }

        await _context.SaveChangesAsync();

        return await GetTask(task.Id);
    }

    // POST:
    // Pause Task
    [HttpPost("{id}/unpause")]
    public async Task<ActionResult<Responses.ITaskDTO>> UnpauseTask(int id)
    {
        var task = await _context.Tasks
            .Where(t => !t.Archived && t.Id == id)
            .Include(t => t.Group)
            .Include(t => t.LearningObjective)
            .Include(t => t.Status)
            .Include(t => t.User)
            .FirstOrDefaultAsync();
        if (task == null)
        {
            return NotFound(new Responses.BadRequestsDTO("Task not found"));
        }

        task.Pause = false;

        if (task.StatusId != 2)
        {
            var status = await _context.Statuses
                .Where(s => s.Id == Statuses.Doing)
                .FirstOrDefaultAsync();
            if (status == null)
                return BadRequest(
                    new Responses.BadRequestsDTO(
                        "I somehow failed to find ToDo Status in a lookup table"
                    )
                );
            task.Status = status;
            task.StatusId = status.Id;
        }

        await _context.SaveChangesAsync();

        return await GetTask(task.Id);
    }
}
