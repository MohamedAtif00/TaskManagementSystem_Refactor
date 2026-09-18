using TaskManagementSystem.Api.Endpoints.Curriculum.AcademicYears;
using TaskManagementSystem.Api.Endpoints.Curriculum.LearningObjectives;
using TaskManagementSystem.Api.Endpoints.Curriculum.Lessons;
using TaskManagementSystem.Api.Endpoints.Curriculum.Projects;
using TaskManagementSystem.Api.Endpoints.Curriculum.SubjectGroups;
using TaskManagementSystem.Api.Endpoints.Curriculum.Subjects;
using TaskManagementSystem.Api.Endpoints.Curriculum.Terms;
using TaskManagementSystem.Api.Endpoints.Curriculum.Units;

namespace TaskManagementSystem.Api.Endpoints.Curriculum;

public static class CurriculumEndpoints
{
    public static RouteGroupBuilder MapCurriculumEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/curriculum").WithTags("Curriculum").RequireAuthorization();

        group.MapAcademicYearEndpoints();
        group.MapProjectEndpoints();
        group.MapTermEndpoints();
        group.MapSubjectGroupEndpoints();
        group.MapSubjectEndpoints();
        group.MapUnitEndpoints();
        group.MapLessonEndpoints();
        group.MapLearningObjectiveEndpoints();

        return group;
    }
}
