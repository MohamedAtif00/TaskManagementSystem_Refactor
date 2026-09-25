using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using TaskManagementSystem.Api.Contracts.Analytics;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Contracts.Sprints;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class AnalyticsIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task GetSubjectOverview_WhenTicketsHaveMixedStatuses_ReturnsStatusCounts()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);

        var createTicketResponse = await client.PostAsJsonAsync(
            "/tickets",
            new CreateTicketRequest
            {
                LearningObjectiveId = setup.LearningObjectiveId,
                TicketBankItemId = setup.TicketBankItemId
            });
        var ticket = await IntegrationHttpAssertions.EnsureAsync<TicketDetailResponse>(
            createTicketResponse,
            HttpStatusCode.Created);

        var usersResponse = await client.GetAsync("/identity/users");
        var users = await usersResponse.Content.ReadFromJsonAsync<List<IdentityUserListItemResponse>>();
        var testUser = users!.Single(user => user.Name == IntegrationTestDataSeeder.TestUserName);

        var assignResponse = await client.PatchAsJsonAsync(
            $"/tickets/{ticket.Id}/assign",
            new AssignTicketRequest { UserId = testUser.Id });
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await client.GetAsync($"/analytics/subjects/{setup.SubjectId}/overview");
        var overview = await IntegrationHttpAssertions.EnsureAsync<OverviewResponse>(response, HttpStatusCode.OK);

        overview.ScopeId.Should().Be(setup.SubjectId);
        overview.ScopeType.Should().Be("subject");
        overview.ToDoTickets.Should().BeGreaterThanOrEqualTo(1);
        overview.TotalLearningObjectives.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetSubjectOverview_WhenLearningObjectiveHasNoTicket_CountsAsIdle()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);
        _ = await CreateSecondLearningObjectiveAsync(client, setup);

        var response = await client.GetAsync($"/analytics/subjects/{setup.SubjectId}/overview");
        var overview = await IntegrationHttpAssertions.EnsureAsync<OverviewResponse>(response, HttpStatusCode.OK);

        overview.TotalLearningObjectives.Should().BeGreaterThanOrEqualTo(2);
        overview.IdleLearningObjectives.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetSubjectOverview_WhenScopeIsEmpty_ReturnsZeros()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var subjectId = await CreateSubjectWithoutLearningObjectivesAsync(client);

        var response = await client.GetAsync($"/analytics/subjects/{subjectId}/overview");
        var overview = await IntegrationHttpAssertions.EnsureAsync<OverviewResponse>(response, HttpStatusCode.OK);

        overview.TotalLearningObjectives.Should().Be(0);
        overview.IdleLearningObjectives.Should().Be(0);
        overview.RunningLearningObjectives.Should().Be(0);
        overview.DoneLearningObjectives.Should().Be(0);
        overview.ProgressPercent.Should().Be(0);
        overview.BacklogTickets.Should().Be(0);
        overview.ToDoTickets.Should().Be(0);
        overview.DoingTickets.Should().Be(0);
        overview.DoneTickets.Should().Be(0);
    }

    [Fact]
    public async Task GetSubjectOverview_WhenSubjectArchived_ReturnsNotFound()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);

        var archiveResponse = await client.DeleteAsync($"/curriculum/subjects/{setup.SubjectId}");
        archiveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await client.GetAsync($"/analytics/subjects/{setup.SubjectId}/overview");
        var problem = await ReadProblemDetailsAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        problem.Code.Should().Be("subject_not_found");
    }

    [Fact]
    public async Task GetSubjectOverview_WhenSubjectMissing_ReturnsNotFound()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/analytics/subjects/999999/overview");
        var problem = await ReadProblemDetailsAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        problem.Code.Should().Be("subject_not_found");
    }

    [Fact]
    public async Task GetSprintOverview_WhenSprintMissing_ReturnsNotFound()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/analytics/sprints/999999/overview");
        var problem = await ReadProblemDetailsAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        problem.Code.Should().Be("sprint_not_found");
    }

    [Fact]
    public async Task GetSubjectOverview_WhenUnauthenticated_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/analytics/subjects/1/overview");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSubjectOverview_WhenMissingPermission_ReturnsForbidden()
    {
        await factory.SeedTestUserAsync();
        var client = CreateClientWithPermissions(factory, PermissionCodes.Organization.Read);

        var response = await client.GetAsync("/analytics/subjects/1/overview");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSprintOverview_WhenSprintHasLinkedLearningObjectives_ReturnsOverview()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);

        var createSprintResponse = await client.PostAsJsonAsync(
            "/sprints",
            new CreateSprintRequest
            {
                Name = "Analytics Sprint",
                Description = "Analytics tests",
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 30)
            });
        var sprint = await IntegrationHttpAssertions.EnsureAsync<SprintDetailResponse>(
            createSprintResponse,
            HttpStatusCode.Created);

        var addLoResponse = await client.PostAsJsonAsync(
            $"/sprints/{sprint.Id}/learning-objectives",
            new AddSprintLearningObjectivesRequest { LearningObjectiveIds = [setup.LearningObjectiveId] });
        addLoResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await client.GetAsync($"/analytics/sprints/{sprint.Id}/overview");
        var overview = await IntegrationHttpAssertions.EnsureAsync<OverviewResponse>(response, HttpStatusCode.OK);

        overview.ScopeId.Should().Be(sprint.Id);
        overview.ScopeType.Should().Be("sprint");
        overview.TotalLearningObjectives.Should().Be(1);
    }

    private static HttpClient CreateClientWithPermissions(
        TmsWebApplicationFactory factory,
        params string[] permissions)
    {
        var client = factory.CreateClient();
        var token = BuildJwt(1, "Member", permissions);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string BuildJwt(int userId, string role, IEnumerable<string> permissions)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TmsWebApplicationFactory.TestJwtSigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new("Id", userId.ToString()),
            new(ClaimTypes.Role, role),
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        claims.AddRange(permissions.Select(permission => new Claim(IdentityClaimTypes.Permission, permission)));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static async Task<int> CreateSubjectWithoutLearningObjectivesAsync(HttpClient client)
    {
        var createYearResponse = await client.PostAsJsonAsync(
            "/curriculum/years",
            new CreateAcademicYearRequest { Name = "Empty Analytics Year", Description = "Empty scope" });
        var year = await createYearResponse.Content.ReadFromJsonAsync<AcademicYearDetailResponse>();

        var createProjectResponse = await client.PostAsJsonAsync(
            $"/curriculum/years/{year!.Id}/projects",
            new CreateProjectRequest { Name = "Empty Analytics Project", Description = "Empty scope" });
        var project = await createProjectResponse.Content.ReadFromJsonAsync<CurriculumProjectDetailResponse>();

        var createTermResponse = await client.PostAsJsonAsync(
            $"/curriculum/projects/{project!.Id}/terms",
            new CreateTermRequest { Name = "Empty Analytics Term" });
        var term = await createTermResponse.Content.ReadFromJsonAsync<CurriculumTermDetailResponse>();

        var createGroupResponse = await client.PostAsJsonAsync(
            $"/curriculum/terms/{term!.Id}/subject-groups",
            new CreateSubjectGroupRequest { Name = "Empty Analytics Group" });
        var group = await createGroupResponse.Content.ReadFromJsonAsync<SubjectGroupDetailResponse>();

        var createSubjectResponse = await client.PostAsJsonAsync(
            $"/curriculum/subject-groups/{group!.Id}/subjects",
            new CreateSubjectRequest { Name = "Empty Analytics Subject", Description = "No LOs" });
        var subject = await createSubjectResponse.Content.ReadFromJsonAsync<SubjectDetailResponse>();

        return subject!.Id;
    }

    private static async Task<int> CreateSecondLearningObjectiveAsync(
        HttpClient client,
        TicketIntegrationTests.TicketSetup setup)
    {
        var unitsResponse = await client.GetAsync($"/curriculum/subjects/{setup.SubjectId}/units");
        var units = await unitsResponse.Content.ReadFromJsonAsync<List<UnitListItemResponse>>();
        var unitId = units!.First().Id;

        var lessonsResponse = await client.GetAsync($"/curriculum/units/{unitId}/lessons");
        var lessons = await lessonsResponse.Content.ReadFromJsonAsync<List<LessonDetailResponse>>();
        var lessonId = lessons!.First().Id;

        var schemaResponse = await client.GetAsync("/workflows/schemas");
        var schemas = await schemaResponse.Content.ReadFromJsonAsync<List<SchemaListItemResponse>>();
        var schemaId = schemas!.First().Id;

        var createLoResponse = await client.PostAsJsonAsync(
            $"/curriculum/lessons/{lessonId}/learning-objectives",
            new CreateLearningObjectiveRequest
            {
                SchemaId = schemaId,
                Name = "Second Analytics LO",
                Tag = "ANL-2",
                Template = "template",
                Environment = "lab"
            });
        var learningObjective = await IntegrationHttpAssertions.EnsureAsync<LearningObjectiveDetailResponse>(
            createLoResponse,
            HttpStatusCode.Created);

        return learningObjective.Id;
    }

    private static async Task<TicketDetailResponse> CreateTicketAsync(
        HttpClient client,
        int learningObjectiveId,
        int ticketBankItemId)
    {
        var response = await client.PostAsJsonAsync(
            "/tickets",
            new CreateTicketRequest
            {
                LearningObjectiveId = learningObjectiveId,
                TicketBankItemId = ticketBankItemId
            });

        return (await IntegrationHttpAssertions.EnsureAsync<TicketDetailResponse>(response, HttpStatusCode.Created))!;
    }

    private static async Task AssignTicketAsync(HttpClient client, int ticketId)
    {
        var usersResponse = await client.GetAsync("/identity/users");
        var users = await usersResponse.Content.ReadFromJsonAsync<List<IdentityUserListItemResponse>>();
        var testUser = users!.Single(user => user.Name == IntegrationTestDataSeeder.TestUserName);

        var assignResponse = await client.PatchAsJsonAsync(
            $"/tickets/{ticketId}/assign",
            new AssignTicketRequest { UserId = testUser.Id });
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<ProblemDetailsDto> ReadProblemDetailsAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return new ProblemDetailsDto
        {
            Detail = json.TryGetProperty("detail", out var detail) ? detail.GetString() : null,
            Code = json.TryGetProperty("code", out var code) ? code.GetString() : null
        };
    }

    private sealed class ProblemDetailsDto
    {
        public string? Detail { get; init; }
        public string? Code { get; init; }
    }

    private sealed class IdentityUserListItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

}
