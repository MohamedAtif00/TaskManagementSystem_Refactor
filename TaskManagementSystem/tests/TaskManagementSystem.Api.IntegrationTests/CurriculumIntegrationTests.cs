using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class CurriculumIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task CurriculumFlow_WhenCreatedAndUserAssigned_Succeeds()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var createYearResponse = await client.PostAsJsonAsync(
            "/curriculum/years",
            new CreateAcademicYearRequest { Name = "Integration Year", Description = "Test year" });
        createYearResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var year = await createYearResponse.Content.ReadFromJsonAsync<AcademicYearDetailResponse>();

        var createProjectResponse = await client.PostAsJsonAsync(
            $"/curriculum/years/{year!.Id}/projects",
            new CreateProjectRequest { Name = "Integration Project", Description = "Test project" });
        createProjectResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var project = await createProjectResponse.Content.ReadFromJsonAsync<CurriculumProjectDetailResponse>();

        var createTermResponse = await client.PostAsJsonAsync(
            $"/curriculum/projects/{project!.Id}/terms",
            new CreateTermRequest { Name = "Term 1" });
        createTermResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var term = await createTermResponse.Content.ReadFromJsonAsync<CurriculumTermDetailResponse>();

        var createGroupResponse = await client.PostAsJsonAsync(
            $"/curriculum/terms/{term!.Id}/subject-groups",
            new CreateSubjectGroupRequest { Name = "Science Group" });
        createGroupResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var group = await createGroupResponse.Content.ReadFromJsonAsync<SubjectGroupDetailResponse>();

        var createSubjectResponse = await client.PostAsJsonAsync(
            $"/curriculum/subject-groups/{group!.Id}/subjects",
            new CreateSubjectRequest { Name = "Physics", Description = "Physics subject" });
        createSubjectResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var subject = await createSubjectResponse.Content.ReadFromJsonAsync<SubjectDetailResponse>();

        var createUnitResponse = await client.PostAsJsonAsync(
            $"/curriculum/subjects/{subject!.Id}/units",
            new CreateUnitRequest { Name = "Unit 1" });
        createUnitResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var unit = await createUnitResponse.Content.ReadFromJsonAsync<UnitDetailResponse>();

        var createLessonResponse = await client.PostAsJsonAsync(
            $"/curriculum/units/{unit!.Id}/lessons",
            new CreateLessonRequest { Name = "Lesson 1" });
        createLessonResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var lesson = await createLessonResponse.Content.ReadFromJsonAsync<LessonDetailResponse>();

        var createSchemaResponse = await client.PostAsJsonAsync(
            "/workflows/schemas",
            new CreateSchemaRequest { Name = "Curriculum Schema", Description = "For LO tests" });
        var schema = await IntegrationHttpAssertions.EnsureAsync<SchemaDetailResponse>(
            createSchemaResponse,
            HttpStatusCode.Created);

        var createLoResponse = await client.PostAsJsonAsync(
            $"/curriculum/lessons/{lesson!.Id}/learning-objectives",
            new CreateLearningObjectiveRequest
            {
                SchemaId = schema.Id,
                Name = "Understand forces",
                Tag = "PHY-1",
                Template = "template",
                Environment = "lab"
            });
        var learningObjective = await IntegrationHttpAssertions.EnsureAsync<LearningObjectiveDetailResponse>(
            createLoResponse,
            HttpStatusCode.Created);
        learningObjective!.SchemaId.Should().Be(schema.Id);

        var usersResponse = await client.GetAsync("/identity/users");
        var users = await usersResponse.Content.ReadFromJsonAsync<List<IdentityUserListItemResponse>>();
        var testUser = users!.Single(user => user.Name == IntegrationTestDataSeeder.TestUserName);

        var assignUsersResponse = await client.PostAsJsonAsync(
            $"/curriculum/subjects/{subject.Id}/users",
            new AssignSubjectUsersRequest { UserIds = [testUser.Id] });
        assignUsersResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listUsersResponse = await client.GetAsync($"/curriculum/subjects/{subject.Id}/users");
        var assignedUsers = await listUsersResponse.Content.ReadFromJsonAsync<List<SubjectUserResponse>>();
        assignedUsers!.Should().ContainSingle(user => user.Id == testUser.Id);

        var treeResponse = await client.GetAsync($"/curriculum/years/{year.Id}/tree");
        treeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tree = await treeResponse.Content.ReadFromJsonAsync<YearTreeResponse>();
        tree!.Projects.Should().ContainSingle(projectItem => projectItem.Id == project.Id);
        tree.Projects.Single().Terms.Should().ContainSingle(termItem => termItem.Id == term.Id);
    }

    private sealed class IdentityUserListItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
