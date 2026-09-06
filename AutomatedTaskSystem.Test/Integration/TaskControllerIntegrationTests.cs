using System.Net;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Task = System.Threading.Tasks.Task;

namespace AutomatedTaskSystem.Test.Integration;

[Collection("Integration")]
public class TaskControllerIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private HttpClient _client = null!;
    private ProjectTestData _projectData = null!;

    public TaskControllerIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = await _factory.CreateAuthenticatedClientAsync();
        _projectData = await ProjectTestData.SeedAsync(_factory.Services);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CompleteTask_WithParallelTerminalTaskRemaining_DoesNotMarkLearningObjectiveDone()
    {
        var scenario = await SeedParallelTerminalTasksAsync();

        var firstCompletion = await _client.PatchAsync($"/tasks/{scenario.FirstTaskId}/complete", null);
        Assert.Equal(HttpStatusCode.OK, firstCompletion.StatusCode);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            var doneAtAfterFirstCompletion = await db.LearningObjectives
                .Where(lo => lo.Id == scenario.LearningObjectiveId)
                .Select(lo => lo.DoneAt)
                .FirstAsync();

            Assert.Null(doneAtAfterFirstCompletion);
        }

        var secondCompletion = await _client.PatchAsync($"/tasks/{scenario.SecondTaskId}/complete", null);
        Assert.Equal(HttpStatusCode.OK, secondCompletion.StatusCode);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            var taskStatuses = await db.Tasks
                .Where(t => !t.Archived && t.LearningObjectiveId == scenario.LearningObjectiveId)
                .Select(t => t.Status)
                .ToListAsync();
            var doneAtAfterSecondCompletion = await db.LearningObjectives
                .Where(lo => lo.Id == scenario.LearningObjectiveId)
                .Select(lo => lo.DoneAt)
                .FirstAsync();

            Assert.All(taskStatuses, status => Assert.Equal(TaskStatusEnum.Done, status));
            Assert.NotNull(doneAtAfterSecondCompletion);
        }
    }

    [Fact]
    public async Task CompleteTask_WhenForkCreatesParallelTasks_DoesNotMarkLearningObjectiveDone()
    {
        var scenario = await SeedForkIntoParallelTasksAsync();

        var completion = await _client.PatchAsync($"/tasks/{scenario.StartTaskId}/complete", null);
        Assert.Equal(HttpStatusCode.OK, completion.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();

        var doneAt = await db.LearningObjectives
            .Where(lo => lo.Id == scenario.LearningObjectiveId)
            .Select(lo => lo.DoneAt)
            .FirstAsync();

        var remainingTasks = await db.Tasks
            .Where(t => !t.Archived && t.LearningObjectiveId == scenario.LearningObjectiveId)
            .Select(t => new { t.Status, t.StepId })
            .ToListAsync();

        Assert.Null(doneAt);
        Assert.Equal(3, remainingTasks.Count);
        Assert.Contains(remainingTasks, t => t.Status == TaskStatusEnum.Done && t.StepId == scenario.StartStepId);
        Assert.Contains(remainingTasks, t => t.Status != TaskStatusEnum.Done && t.StepId == scenario.FirstEndStepId);
        Assert.Contains(remainingTasks, t => t.Status != TaskStatusEnum.Done && t.StepId == scenario.SecondEndStepId);
    }

    [Fact]
    public async Task CompleteTask_WithMissingParallelTerminalTask_DoesNotMarkLearningObjectiveDone()
    {
        var scenario = await SeedParallelTerminalTasksAsync(seedSecondTask: false);

        var completion = await _client.PatchAsync($"/tasks/{scenario.FirstTaskId}/complete", null);
        Assert.Equal(HttpStatusCode.OK, completion.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        var doneAt = await db.LearningObjectives
            .Where(lo => lo.Id == scenario.LearningObjectiveId)
            .Select(lo => lo.DoneAt)
            .FirstAsync();

        Assert.Null(doneAt);
    }

    private async Task<ParallelTerminalScenario> SeedParallelTerminalTasksAsync(bool seedSecondTask = true)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var owner = await db.Users
            .Where(u => u.Code == IntegrationTestWebAppFactory.TestUserCode)
            .FirstAsync();

        var group = new Group
        {
            Name = $"Task Integration Group {suffix}",
            ColorCode = "#123456",
        };

        var taskBankOne = new TaskBank
        {
            Name = $"Parallel End Task 1 {suffix}",
            Group = group,
            Duration = 1,
        };
        var taskBankTwo = new TaskBank
        {
            Name = $"Parallel End Task 2 {suffix}",
            Group = group,
            Duration = 1,
        };

        var schema = new Schema
        {
            Name = $"Parallel End Schema {suffix}",
            Description = "Integration test schema with parallel terminal nodes.",
        };

        var nodeOne = new Node
        {
            Name = $"Parallel Terminal Node 1 {suffix}",
            Schema = schema,
            Order = 1,
            isStart = true,
            isEnd = true,
        };
        var nodeTwo = new Node
        {
            Name = $"Parallel Terminal Node 2 {suffix}",
            Schema = schema,
            Order = 2,
            isStart = true,
            isEnd = true,
        };
        schema.Nodes.AddRange(nodeOne, nodeTwo);

        var stepOne = new Step
        {
            Node = nodeOne,
            TaskBank = taskBankOne,
            Order = 1,
            Duration = 1,
        };
        var stepTwo = new Step
        {
            Node = nodeTwo,
            TaskBank = taskBankTwo,
            Order = 1,
            Duration = 1,
        };

        var unit = new Unit
        {
            Name = $"Task Integration Unit {suffix}",
            SubjectId = _projectData.ProjectId,
        };
        var lesson = new Lesson
        {
            Name = $"Task Integration Lesson {suffix}",
            Unit = unit,
        };
        var learningObjective = new LearningObjective
        {
            Name = $"Task Integration LO {suffix}",
            Lesson = lesson,
            Schema = schema,
            Tag = $"tag-{suffix}",
            Environment = "integration",
            Template = "template",
        };

        var taskOne = new Models.Task
        {
            Name = $"Parallel Terminal Task One {suffix}",
            LearningObjective = learningObjective,
            Step = stepOne,
            Group = group,
            UserId = owner.Id,
            Status = TaskStatusEnum.Doing,
        };
        var taskTwo = new Models.Task
        {
            Name = $"Parallel Terminal Task Two {suffix}",
            LearningObjective = learningObjective,
            Step = stepTwo,
            Group = group,
            UserId = owner.Id,
            Status = TaskStatusEnum.Doing,
        };

        db.Nodes.AddRange(nodeOne, nodeTwo);
        db.Steps.AddRange(stepOne, stepTwo);
        db.Tasks.Add(taskOne);
        if (seedSecondTask)
            db.Tasks.Add(taskTwo);
        await db.SaveChangesAsync();

        return new ParallelTerminalScenario(taskOne.Id, seedSecondTask ? taskTwo.Id : 0, learningObjective.Id);
    }

    private async Task<ForkParallelScenario> SeedForkIntoParallelTasksAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var owner = await db.Users
            .Where(u => u.Code == IntegrationTestWebAppFactory.TestUserCode)
            .FirstAsync();

        var group = new Group
        {
            Name = $"Fork Integration Group {suffix}",
            ColorCode = "#654321",
        };

        var startTaskBank = new TaskBank
        {
            Name = $"Fork Start Task {suffix}",
            Group = group,
            Duration = 1,
            TL = true,
        };
        var endTaskBankOne = new TaskBank
        {
            Name = $"Fork End Task 1 {suffix}",
            Group = group,
            Duration = 1,
            TL = true,
        };
        var endTaskBankTwo = new TaskBank
        {
            Name = $"Fork End Task 2 {suffix}",
            Group = group,
            Duration = 1,
            TL = true,
        };

        var schema = new Schema
        {
            Name = $"Fork Parallel Schema {suffix}",
            Description = "Integration test schema that forks into parallel terminal nodes.",
        };

        var startNode = new Node
        {
            Name = $"Fork Start Node {suffix}",
            Schema = schema,
            Order = 1,
            isStart = true,
            isEnd = false,
        };
        var endNodeOne = new Node
        {
            Name = $"Fork End Node 1 {suffix}",
            Schema = schema,
            Order = 2,
            isStart = false,
            isEnd = true,
        };
        var endNodeTwo = new Node
        {
            Name = $"Fork End Node 2 {suffix}",
            Schema = schema,
            Order = 3,
            isStart = false,
            isEnd = true,
        };

        startNode.Next.Add(endNodeOne);
        startNode.Next.Add(endNodeTwo);
        endNodeOne.Previous.Add(startNode);
        endNodeTwo.Previous.Add(startNode);
        schema.Nodes.AddRange(startNode, endNodeOne, endNodeTwo);

        var startStep = new Step
        {
            Node = startNode,
            TaskBank = startTaskBank,
            Order = 1,
            Duration = 1,
        };
        var endStepOne = new Step
        {
            Node = endNodeOne,
            TaskBank = endTaskBankOne,
            Order = 1,
            Duration = 1,
        };
        var endStepTwo = new Step
        {
            Node = endNodeTwo,
            TaskBank = endTaskBankTwo,
            Order = 1,
            Duration = 1,
        };

        var unit = new Unit
        {
            Name = $"Fork Integration Unit {suffix}",
            SubjectId = _projectData.ProjectId,
        };
        var lesson = new Lesson
        {
            Name = $"Fork Integration Lesson {suffix}",
            Unit = unit,
        };
        var learningObjective = new LearningObjective
        {
            Name = $"Fork Integration LO {suffix}",
            Lesson = lesson,
            Schema = schema,
            Tag = $"fork-{suffix}",
            Environment = "integration",
            Template = "template",
        };

        var startTask = new Models.Task
        {
            Name = $"Fork Start Task {suffix}",
            LearningObjective = learningObjective,
            Step = startStep,
            Group = group,
            UserId = owner.Id,
            Status = TaskStatusEnum.Doing,
            TL = true,
        };

        db.Nodes.AddRange(startNode, endNodeOne, endNodeTwo);
        db.Steps.AddRange(startStep, endStepOne, endStepTwo);
        db.Tasks.Add(startTask);
        await db.SaveChangesAsync();

        return new ForkParallelScenario(
            startTask.Id,
            learningObjective.Id,
            startStep.Id,
            endStepOne.Id,
            endStepTwo.Id);
    }

    private sealed record ParallelTerminalScenario(int FirstTaskId, int SecondTaskId, int LearningObjectiveId);

    private sealed record ForkParallelScenario(
        int StartTaskId,
        int LearningObjectiveId,
        int StartStepId,
        int FirstEndStepId,
        int SecondEndStepId);
}
