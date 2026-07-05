using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using Task = System.Threading.Tasks.Task;

namespace AutomatedTaskSystem.Test.Integration;

/// <summary>
/// Integration tests for SubjectController (hierarchy successor to ProjectController).
/// Serves as a safety net while refactoring the controller.
/// </summary>
[Collection("Integration")]
public class ProjectControllerIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private HttpClient _client = null!;
    private ProjectTestData _data = null!;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public ProjectControllerIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = await _factory.CreateAuthenticatedClientAsync();
        _data = await ProjectTestData.SeedAsync(_factory.Services);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ── GET /subjects/years ──────────────────────────────────────────────

    [Fact]
    public async Task GetActiveYears_ReturnsSeededYear()
    {
        var response = await _client.GetAsync("/subjects/years");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<IdNameDto>>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.Contains(body.Data!, y => y.Id == _data.YearId && y.Name == "2025-2026");
    }

    // ── POST /subjects ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateProject_WithValidYear_ReturnsCreatedProject()
    {
        var response = await _client.PostAsJsonAsync("/subjects", new
        {
            name = "New Integration Project",
            description = "Created via integration test",
            termId = _data.TermId,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProjectDto>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.Equal("New Integration Project", body.Data!.Name);
        Assert.Equal(_data.TermId, body.Data.TermId);
    }

    [Fact]
    public async Task CreateProject_WithInvalidTerm_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync("/subjects", new
        {
            name = "Invalid Term Project",
            description = "Should fail",
            termId = 999999,
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── GET /subjects ────────────────────────────────────────────────────

    [Fact]
    public async Task GetProjects_ReturnsNonEmptyList()
    {
        var response = await _client.GetAsync("/subjects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProjectDto>>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.Contains(body.Data!, p => p.Id == _data.ProjectId);
    }

    [Fact]
    public async Task GetProjectsForSprint_ExcludesHoldAndClosedProjects()
    {
        var response = await _client.GetAsync("/subjects/GetAllForSprint");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProjectDto>>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.All(body.Data!, p =>
            Assert.NotEqual(ProjectStatusEnum.Hold, p.Status));
        Assert.All(body.Data!, p =>
            Assert.NotEqual(ProjectStatusEnum.Closed, p.Status));
    }

    // ── GET /subjects/assignment ─────────────────────────────────────────

    [Fact]
    public async Task GetAssignedProject_AsOwner_ReturnsActiveProjects()
    {
        var response = await _client.GetAsync("/subjects/assignment");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProjectDto>>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.Contains(body.Data!, p => p.Id == _data.ProjectId);
    }

    // ── GET /subjects/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task GetProject_WithExistingId_ReturnsProject()
    {
        var response = await _client.GetAsync($"/subjects/{_data.ProjectId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProjectDto>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.Equal(_data.ProjectId, body.Data!.Id);
        Assert.Equal("Integration Test Project", body.Data.Name);
    }

    [Fact]
    public async Task GetProject_WithMissingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/subjects/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── GET /subjects/{id}/details ───────────────────────────────────────

    [Fact]
    public async Task GetProjectDetails_WithExistingId_ReturnsDetailedProject()
    {
        var response = await _client.GetAsync($"/subjects/{_data.ProjectId}/details");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DetailedProjectDto>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.Equal(_data.ProjectId, body.Data!.Id);
        Assert.NotNull(body.Data.Units);
    }

    [Fact]
    public async Task GetProjectDetails_WithMissingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/subjects/999999/details");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── GET /subjects/{id}/los ───────────────────────────────────────────

    [Fact]
    public async Task GetProjectLearningObjectives_WithExistingProject_ReturnsList()
    {
        var response = await _client.GetAsync($"/subjects/{_data.ProjectId}/los");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<IdNameDto>>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.NotNull(body.Data);
    }

    // ── PATCH /subjects/{id} ─────────────────────────────────────────────

    [Fact]
    public async Task EditProject_WithValidData_UpdatesProject()
    {
        var response = await _client.PatchAsJsonAsync($"/subjects/{_data.ProjectId}", new
        {
            name = "Updated Integration Project",
            description = "Updated description",
            termId = _data.TermId,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProjectDto>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.Equal("Updated Integration Project", body.Data!.Name);
        Assert.Equal("Updated description", body.Data.Description);
    }

    [Fact]
    public async Task EditProject_WithInvalidTerm_ReturnsBadRequest()
    {
        var response = await _client.PatchAsJsonAsync($"/subjects/{_data.ProjectId}", new
        {
            name = "Updated Integration Project",
            description = "Updated description",
            termId = 999999,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EditProject_WithMissingId_ReturnsNotFound()
    {
        var response = await _client.PatchAsJsonAsync("/subjects/999999", new
        {
            name = "Ghost Project",
            description = "N/A",
            termId = _data.TermId,
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── POST /subjects/{id}/units ────────────────────────────────────────

    [Fact]
    public async Task AddUnit_ToExistingProject_ReturnsUnit()
    {
        var response = await _client.PostAsJsonAsync($"/subjects/{_data.ProjectId}/units", new
        {
            name = "Unit Alpha",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProjectUnitDto>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.Equal("Unit Alpha", body.Data!.Name);
        Assert.True(body.Data.Id > 0);
    }

    [Fact]
    public async Task AddUnit_ToMissingProject_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync("/subjects/999999/units", new { name = "Orphan Unit" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── POST /subjects/{id}/assign & unassign ────────────────────────────

    [Fact]
    public async Task AssignToProject_AddsMemberToProject()
    {
        await _client.PostAsJsonAsync($"/subjects/{_data.ProjectId}/unassign", new
        {
            userIds = new[] { _data.MemberUserId },
        });

        var response = await _client.PostAsJsonAsync($"/subjects/{_data.ProjectId}/assign", new
        {
            userIds = new[] { _data.MemberUserId },
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<IdNameDto>>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.Contains(body.Data!, u => u.Id == _data.MemberUserId);
    }

    [Fact]
    public async Task GetAssignedUsers_ReturnsAssignedMember()
    {
        await _client.PostAsJsonAsync($"/subjects/{_data.ProjectId}/assign", new
        {
            userIds = new[] { _data.MemberUserId },
        });

        var response = await _client.GetAsync($"/subjects/{_data.ProjectId}/users/assigned");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<UserDto>>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.Contains(body.Data!, u => u.Id == _data.MemberUserId);
    }

    [Fact]
    public async Task GetUnassignedUsers_ExcludesAssignedMember()
    {
        await _client.PostAsJsonAsync($"/subjects/{_data.ProjectId}/assign", new
        {
            userIds = new[] { _data.MemberUserId },
        });

        var response = await _client.GetAsync($"/subjects/{_data.ProjectId}/users/unassigned");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<UserDto>>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.DoesNotContain(body.Data!, u => u.Id == _data.MemberUserId);
    }

    [Fact]
    public async Task UnassignFromProject_RemovesMemberFromProject()
    {
        await _client.PostAsJsonAsync($"/subjects/{_data.ProjectId}/assign", new
        {
            userIds = new[] { _data.MemberUserId },
        });

        var response = await _client.PostAsJsonAsync($"/subjects/{_data.ProjectId}/unassign", new
        {
            userIds = new[] { _data.MemberUserId },
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<IdNameDto>>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.Contains(body.Data!, u => u.Id == _data.MemberUserId);
    }

    [Fact]
    public async Task AssignToProject_WithMissingProject_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync("/subjects/999999/assign", new
        {
            userIds = new[] { _data.MemberUserId },
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── PATCH /subjects/{id}/status ──────────────────────────────────────

    [Fact]
    public async Task UpdateStatus_ToHold_PutsProjectOnHold()
    {
        var projectId = await CreateIsolatedProjectAsync("Hold Test Project");

        var response = await _client.PatchAsJsonAsync($"/subjects/{projectId}/status", new
        {
            status = ProjectStatusEnum.Hold,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProjectDto>>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.Equal(ProjectStatusEnum.Hold, body.Data!.Status);
    }

    [Fact]
    public async Task UpdateStatus_ToHold_WhenAlreadyOnHold_ReturnsBadRequest()
    {
        var projectId = await CreateIsolatedProjectAsync("Double Hold Project");
        await _client.PatchAsJsonAsync($"/subjects/{projectId}/status", new { status = ProjectStatusEnum.Hold });

        var response = await _client.PatchAsJsonAsync($"/subjects/{projectId}/status", new
        {
            status = ProjectStatusEnum.Hold,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ToActive_FromHold_ReactivatesProject()
    {
        var projectId = await CreateIsolatedProjectAsync("Reactivate Project");
        await _client.PatchAsJsonAsync($"/subjects/{projectId}/status", new { status = ProjectStatusEnum.Hold });

        var response = await _client.PatchAsJsonAsync($"/subjects/{projectId}/status", new
        {
            status = ProjectStatusEnum.Active,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProjectDto>>(JsonOptions);
        Assert.Equal(ProjectStatusEnum.Active, body!.Data!.Status);
    }

    [Fact]
    public async Task UpdateStatus_ToClosed_ClosesProject()
    {
        var projectId = await CreateIsolatedProjectAsync("Close Test Project");

        var response = await _client.PatchAsJsonAsync($"/subjects/{projectId}/status", new
        {
            status = ProjectStatusEnum.Closed,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProjectDto>>(JsonOptions);
        Assert.Equal(ProjectStatusEnum.Closed, body!.Data!.Status);
    }

    [Fact]
    public async Task UpdateStatus_WithMissingProject_ReturnsNotFound()
    {
        var response = await _client.PatchAsJsonAsync("/subjects/999999/status", new
        {
            status = ProjectStatusEnum.Hold,
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── DELETE /subjects/{id} ────────────────────────────────────────────

    [Fact]
    public async Task DeleteProject_WithExistingId_ArchivesProject()
    {
        var projectId = await CreateIsolatedProjectAsync("Delete Test Project");

        var deleteResponse = await _client.DeleteAsync($"/subjects/{projectId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/subjects/{projectId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProject_WithMissingId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/subjects/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<int> CreateIsolatedProjectAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/subjects", new
        {
            name,
            description = "Isolated test project",
            termId = _data.TermId,
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProjectDto>>(JsonOptions);
        return body!.Data!.Id;
    }

    private sealed class ApiResponse<T>
    {
        public T? Data { get; set; }
        public bool Error { get; set; }
        public string? Message { get; set; }
    }

    private sealed class IdNameDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int TermId { get; set; }
        public ProjectStatusEnum Status { get; set; }
    }

    private sealed class DetailedProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public ProjectStatusEnum Status { get; set; }
        public List<ProjectUnitDto> Units { get; set; } = [];
    }

    private sealed class ProjectUnitDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
