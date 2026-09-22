using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Contracts.Notifications;
using TaskManagementSystem.Api.Contracts.Sprints;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Modules.Workflows.Domain;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class SprintsNotificationsIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task ListSprints_WhenArchivedQueryIsMissingOrEmpty_ReturnsOk()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var omitted = await client.GetAsync("/sprints");
        var empty = await client.GetAsync("/sprints?archived=");
        var explicitFalse = await client.GetAsync("/sprints?archived=false");

        omitted.StatusCode.Should().Be(HttpStatusCode.OK);
        empty.StatusCode.Should().Be(HttpStatusCode.OK);
        explicitFalse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await omitted.Content.ReadFromJsonAsync<List<SprintListItemResponse>>()).Should().NotBeNull();
        (await empty.Content.ReadFromJsonAsync<List<SprintListItemResponse>>()).Should().NotBeNull();
    }

    [Fact]
    public async Task SprintsAndNotificationsFlow_WhenTicketAssigned_CreatesNotification()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await CreateTicketSetupAsync(client);

        var createSprintResponse = await client.PostAsJsonAsync(
            "/sprints",
            new CreateSprintRequest
            {
                Name = "Integration Sprint",
                Description = "Sprint tests",
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 30)
            });
        createSprintResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var sprint = await createSprintResponse.Content.ReadFromJsonAsync<SprintDetailResponse>();

        var addLoResponse = await client.PostAsJsonAsync(
            $"/sprints/{sprint!.Id}/learning-objectives",
            new AddSprintLearningObjectivesRequest { LearningObjectiveIds = [setup.LearningObjectiveId] });
        addLoResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listLoResponse = await client.GetAsync($"/sprints/{sprint.Id}/learning-objectives");
        var learningObjectiveIds = await listLoResponse.Content.ReadFromJsonAsync<List<int>>();
        learningObjectiveIds!.Should().ContainSingle(id => id == setup.LearningObjectiveId);

        var createTicketResponse = await client.PostAsJsonAsync(
            "/tickets",
            new CreateTicketRequest
            {
                LearningObjectiveId = setup.LearningObjectiveId,
                TicketBankItemId = setup.TicketBankItemId
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

        var listSprintTicketsResponse = await client.GetAsync($"/sprints/{sprint.Id}/tickets");
        var sprintTickets = await listSprintTicketsResponse.Content.ReadFromJsonAsync<List<TicketListItemResponse>>();
        sprintTickets!.Should().ContainSingle(item => item.Id == ticket.Id);

        NotificationListPageResponse? notifications = null;
        for (var attempt = 0; attempt < 40; attempt++)
        {
            await factory.DrainOutboxesAsync();

            var notificationsResponse = await client.GetAsync("/notifications?isRead=false");
            notificationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            notifications = await notificationsResponse.Content.ReadFromJsonAsync<NotificationListPageResponse>();

            if (notifications!.Items.Any(item =>
                    item.Title == "Task assigned" &&
                    item.Type == "Assignment" &&
                    item.RelatedEntityId == ticket.Id))
            {
                break;
            }

            await Task.Delay(250);
        }

        notifications!.Items.Should().Contain(item =>
            item.Title == "Task assigned" &&
            item.Type == "Assignment" &&
            item.RelatedEntityId == ticket.Id);

        var notificationId = notifications.Items.First(item => item.RelatedEntityId == ticket.Id).Id;
        var markReadResponse = await client.PatchAsync($"/notifications/{notificationId}/read", null);
        markReadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var readNotification = await markReadResponse.Content.ReadFromJsonAsync<NotificationDetailResponse>();
        readNotification!.IsRead.Should().BeTrue();
    }

    private static async Task<TicketSetup> CreateTicketSetupAsync(HttpClient client)
    {
        var createYearResponse = await client.PostAsJsonAsync(
            "/curriculum/years",
            new CreateAcademicYearRequest { Name = "Sprint Year", Description = "Sprint tests" });
        var year = await createYearResponse.Content.ReadFromJsonAsync<AcademicYearDetailResponse>();

        var createProjectResponse = await client.PostAsJsonAsync(
            $"/curriculum/years/{year!.Id}/projects",
            new CreateProjectRequest { Name = "Sprint Project", Description = "Sprint tests" });
        var project = await createProjectResponse.Content.ReadFromJsonAsync<CurriculumProjectDetailResponse>();

        var createTermResponse = await client.PostAsJsonAsync(
            $"/curriculum/projects/{project!.Id}/terms",
            new CreateTermRequest { Name = "Sprint Term" });
        var term = await createTermResponse.Content.ReadFromJsonAsync<CurriculumTermDetailResponse>();

        var createGroupResponse = await client.PostAsJsonAsync(
            $"/curriculum/terms/{term!.Id}/subject-groups",
            new CreateSubjectGroupRequest { Name = "Sprint Group" });
        var group = await createGroupResponse.Content.ReadFromJsonAsync<SubjectGroupDetailResponse>();

        var createSubjectResponse = await client.PostAsJsonAsync(
            $"/curriculum/subject-groups/{group!.Id}/subjects",
            new CreateSubjectRequest { Name = "Sprint Subject", Description = "Sprint subject" });
        var subject = await createSubjectResponse.Content.ReadFromJsonAsync<SubjectDetailResponse>();

        var createUnitResponse = await client.PostAsJsonAsync(
            $"/curriculum/subjects/{subject!.Id}/units",
            new CreateUnitRequest { Name = "Sprint Unit" });
        var unit = await createUnitResponse.Content.ReadFromJsonAsync<UnitDetailResponse>();

        var createLessonResponse = await client.PostAsJsonAsync(
            $"/curriculum/units/{unit!.Id}/lessons",
            new CreateLessonRequest { Name = "Sprint Lesson" });
        var lesson = await createLessonResponse.Content.ReadFromJsonAsync<LessonDetailResponse>();

        var createSchemaResponse = await client.PostAsJsonAsync(
            "/workflows/schemas",
            new CreateSchemaRequest { Name = "Sprint Schema", Description = "Sprint workflow" });
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

        var createTicketBankResponse = await client.PostAsJsonAsync(
            "/workflows/ticket-bank",
            new CreateTicketBankItemRequest
            {
                Name = "Sprint task",
                Duration = 60,
                Type = TicketBankType.Creation,
                TeamLeaderOnly = false,
                TeamId = team.Id
            });
        var taskBank = await IntegrationHttpAssertions.EnsureAsync<TicketBankListItemResponse>(
            createTicketBankResponse,
            HttpStatusCode.Created);

        var createStepResponse = await client.PostAsJsonAsync(
            $"/workflows/nodes/{node.Id}/steps",
            new CreateStepRequest
            {
                TicketBankId = taskBank.Id,
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
                Name = "Sprint LO",
                Tag = "SPR-1",
                Template = "template",
                Environment = "lab"
            });
        var learningObjective = await IntegrationHttpAssertions.EnsureAsync<LearningObjectiveDetailResponse>(
            createLoResponse,
            HttpStatusCode.Created);

        return new TicketSetup(learningObjective.Id, taskBank.Id, step.Id);
    }

    private sealed record TicketSetup(int LearningObjectiveId, int TicketBankItemId, int StepId);

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
