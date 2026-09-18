using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Contracts.Notifications;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Modules.Ticket.Domain;
using DomainTaskStatus = TaskManagementSystem.Modules.Ticket.Domain.TaskStatus;
using TaskManagementSystem.Modules.Workflows.Domain;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class TicketIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task TicketFlow_WhenCreatedAndUpdated_Succeeds()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await CreateTicketSetupAsync(client);

        var createTicketResponse = await client.PostAsJsonAsync(
            "/tickets",
            new CreateTicketRequest
            {
                LearningObjectiveId = setup.LearningObjectiveId,
                TaskBankItemId = setup.TaskBankItemId
            });
        createTicketResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var ticket = await createTicketResponse.Content.ReadFromJsonAsync<TicketDetailResponse>();
        ticket!.Status.Should().Be(DomainTaskStatus.Backlog);
        ticket.StepId.Should().Be(setup.StepId);

        var getTicketResponse = await client.GetAsync($"/tickets/{ticket.Id}");
        getTicketResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var usersResponse = await client.GetAsync("/identity/users");
        var users = await usersResponse.Content.ReadFromJsonAsync<List<IdentityUserListItemResponse>>();
        var testUser = users!.Single(user => user.Name == IntegrationTestDataSeeder.TestUserName);

        var assignResponse = await client.PatchAsJsonAsync(
            $"/tickets/{ticket.Id}/assign",
            new AssignTicketRequest { UserId = testUser.Id });
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignedTicket = await assignResponse.Content.ReadFromJsonAsync<TicketDetailResponse>();
        assignedTicket!.UserId.Should().Be(testUser.Id);
        assignedTicket.Status.Should().Be(DomainTaskStatus.ToDo);

        var commentResponse = await client.PostAsJsonAsync(
            $"/tickets/{ticket.Id}/comments",
            new AddCommentRequest { Content = "Starting work" });
        commentResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listCommentsResponse = await client.GetAsync($"/tickets/{ticket.Id}/comments");
        var comments = await listCommentsResponse.Content.ReadFromJsonAsync<List<CommentListItemResponse>>();
        comments!.Should().ContainSingle(comment => comment.Content == "Starting work");

        var startWorkTimeResponse = await client.PostAsync($"/tickets/{ticket.Id}/work-times/start", null);
        startWorkTimeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var stopWorkTimeResponse = await client.PostAsync($"/tickets/{ticket.Id}/work-times/stop", null);
        stopWorkTimeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var proceedResponse = await client.PatchAsync($"/tickets/{ticket.Id}/proceed", null);
        proceedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var proceededTicket = await proceedResponse.Content.ReadFromJsonAsync<TicketDetailResponse>();
        proceededTicket!.Status.Should().Be(DomainTaskStatus.Done);

        var listByLoResponse = await client.GetAsync($"/learning-objectives/{setup.LearningObjectiveId}/tickets");
        var loTickets = await listByLoResponse.Content.ReadFromJsonAsync<List<TicketListItemResponse>>();
        loTickets!.Should().ContainSingle(item => item.Id == ticket.Id);

        var listBySubjectResponse = await client.GetAsync($"/subjects/{setup.SubjectId}/tickets");
        var subjectTickets = await listBySubjectResponse.Content.ReadFromJsonAsync<List<TicketListItemResponse>>();
        subjectTickets!.Should().ContainSingle(item => item.Id == ticket.Id);
    }

    [Fact]
    public async Task AssignTicket_WritesNotificationViaOutbox()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await CreateTicketSetupAsync(client);

        var createTicketResponse = await client.PostAsJsonAsync(
            "/tickets",
            new CreateTicketRequest
            {
                LearningObjectiveId = setup.LearningObjectiveId,
                TaskBankItemId = setup.TaskBankItemId
            });
        createTicketResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var ticket = await createTicketResponse.Content.ReadFromJsonAsync<TicketDetailResponse>();

        var usersResponse = await client.GetAsync("/identity/users");
        var users = await usersResponse.Content.ReadFromJsonAsync<List<IdentityUserListItemResponse>>();
        var testUser = users!.Single(user => user.Name == IntegrationTestDataSeeder.TestUserName);

        var assignResponse = await client.PatchAsJsonAsync(
            $"/tickets/{ticket!.Id}/assign",
            new AssignTicketRequest { UserId = testUser.Id });
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        NotificationListItemResponse? notification = null;
        for (var attempt = 0; attempt < 20; attempt++)
        {
            await factory.DrainOutboxesAsync();

            var notificationsResponse = await client.GetAsync("/notifications?page=1&pageSize=20");
            notificationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var notifications = await notificationsResponse.Content.ReadFromJsonAsync<NotificationListPageResponse>();
            notification = notifications!.Items.FirstOrDefault(item =>
                item.Title == "Task assigned" && item.RelatedEntityId == ticket.Id);

            if (notification is not null)
            {
                break;
            }

            await Task.Delay(250);
        }

        notification.Should().NotBeNull();

        await using var connection = new SqlConnection(factory.ConnectionString);
        var outboxCount = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM [ticket].[OutboxMessages]
            WHERE [Payload] LIKE @TicketIdPattern
            """,
            new { TicketIdPattern = $"%\"ticketId\":{ticket!.Id}%" });

        outboxCount.Should().BeGreaterThan(0);

        var inboxCount = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM [notifications].[InboxMessages]
            WHERE [ConsumerName] = @ConsumerName
              AND [Payload] LIKE @TicketIdPattern
            """,
            new
            {
                ConsumerName = "Notifications.OnTicketAssigned",
                TicketIdPattern = $"%\"ticketId\":{ticket.Id}%"
            });

        inboxCount.Should().Be(1);

        var notificationCount = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM [notifications].[Notifications]
            WHERE [RelatedEntityId] = @TicketId
              AND [Title] = 'Task assigned'
            """,
            new { TicketId = ticket.Id });

        notificationCount.Should().Be(1);
    }

    private static async Task<TicketSetup> CreateTicketSetupAsync(HttpClient client)
    {
        var createYearResponse = await client.PostAsJsonAsync(
            "/curriculum/years",
            new CreateAcademicYearRequest { Name = "Ticket Year", Description = "Ticket tests" });
        var year = await createYearResponse.Content.ReadFromJsonAsync<AcademicYearDetailResponse>();

        var createProjectResponse = await client.PostAsJsonAsync(
            $"/curriculum/years/{year!.Id}/projects",
            new CreateProjectRequest { Name = "Ticket Project", Description = "Ticket tests" });
        var project = await createProjectResponse.Content.ReadFromJsonAsync<CurriculumProjectDetailResponse>();

        var createTermResponse = await client.PostAsJsonAsync(
            $"/curriculum/projects/{project!.Id}/terms",
            new CreateTermRequest { Name = "Ticket Term" });
        var term = await createTermResponse.Content.ReadFromJsonAsync<CurriculumTermDetailResponse>();

        var createGroupResponse = await client.PostAsJsonAsync(
            $"/curriculum/terms/{term!.Id}/subject-groups",
            new CreateSubjectGroupRequest { Name = "Ticket Group" });
        var group = await createGroupResponse.Content.ReadFromJsonAsync<SubjectGroupDetailResponse>();

        var createSubjectResponse = await client.PostAsJsonAsync(
            $"/curriculum/subject-groups/{group!.Id}/subjects",
            new CreateSubjectRequest { Name = "Ticket Subject", Description = "Ticket subject" });
        var subject = await createSubjectResponse.Content.ReadFromJsonAsync<SubjectDetailResponse>();

        var createUnitResponse = await client.PostAsJsonAsync(
            $"/curriculum/subjects/{subject!.Id}/units",
            new CreateUnitRequest { Name = "Ticket Unit" });
        var unit = await createUnitResponse.Content.ReadFromJsonAsync<UnitDetailResponse>();

        var createLessonResponse = await client.PostAsJsonAsync(
            $"/curriculum/units/{unit!.Id}/lessons",
            new CreateLessonRequest { Name = "Ticket Lesson" });
        var lesson = await createLessonResponse.Content.ReadFromJsonAsync<LessonDetailResponse>();

        var createSchemaResponse = await client.PostAsJsonAsync(
            "/workflows/schemas",
            new CreateSchemaRequest { Name = "Ticket Schema", Description = "Ticket workflow" });
        var schema = await IntegrationHttpAssertions.EnsureAsync<SchemaDetailResponse>(
            createSchemaResponse,
            HttpStatusCode.Created);

        var createNodeResponse = await client.PostAsJsonAsync(
            $"/workflows/schemas/{schema.Id}/nodes",
            new CreateNodeRequest { Name = "Start", IsStart = true, IsEnd = true });
        var node = await IntegrationHttpAssertions.EnsureAsync<NodeListItemResponse>(
            createNodeResponse,
            HttpStatusCode.Created);

        var teamsResponse = await client.GetAsync("/organization/teams");
        var teams = await IntegrationHttpAssertions.EnsureAsync<List<TeamListItemResponse>>(
            teamsResponse,
            HttpStatusCode.OK);
        var team = teams.First();

        var createTaskBankResponse = await client.PostAsJsonAsync(
            "/workflows/task-bank",
            new CreateTaskBankItemRequest
            {
                Name = "Create lesson content",
                Duration = 60,
                Type = TaskBankType.Creation,
                TeamLeaderOnly = false,
                TeamId = team.Id
            });
        var taskBank = await IntegrationHttpAssertions.EnsureAsync<TaskBankListItemResponse>(
            createTaskBankResponse,
            HttpStatusCode.Created);

        var createStepResponse = await client.PostAsJsonAsync(
            $"/workflows/nodes/{node.Id}/steps",
            new CreateStepRequest
            {
                TaskBankId = taskBank.Id,
                Duration = 60,
                Priority = 2
            });
        var step = await IntegrationHttpAssertions.EnsureAsync<StepListItemResponse>(
            createStepResponse,
            HttpStatusCode.Created);

        var createLoResponse = await client.PostAsJsonAsync(
            $"/curriculum/lessons/{lesson!.Id}/learning-objectives",
            new CreateLearningObjectiveRequest
            {
                SchemaId = schema.Id,
                Name = "Ticket LO",
                Tag = "TCK-1",
                Template = "template",
                Environment = "lab"
            });
        var learningObjective = await IntegrationHttpAssertions.EnsureAsync<LearningObjectiveDetailResponse>(
            createLoResponse,
            HttpStatusCode.Created);

        return new TicketSetup(
            subject!.Id,
            learningObjective.Id,
            taskBank.Id,
            step.Id);
    }

    private sealed record TicketSetup(
        int SubjectId,
        int LearningObjectiveId,
        int TaskBankItemId,
        int StepId);

    private sealed class IdentityUserListItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TeamListItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
