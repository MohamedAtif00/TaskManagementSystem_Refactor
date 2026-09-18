using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Modules.Curriculum.Features;

namespace TaskManagementSystem.Api.Endpoints.Curriculum;

internal static class CurriculumMapping
{
    internal static AcademicYearListItemResponse MapAcademicYearListItem(AcademicYearListItemResult year) =>
        new() { Id = year.Id, Name = year.Name, Description = year.Description };

    internal static AcademicYearDetailResponse MapAcademicYearDetail(AcademicYearDetailResult year) =>
        new() { Id = year.Id, Name = year.Name, Description = year.Description };

    internal static CurriculumProjectListItemResponse MapProjectListItem(CurriculumProjectListItemResult project) =>
        new() { Id = project.Id, Name = project.Name, Description = project.Description, YearId = project.YearId };

    internal static CurriculumProjectDetailResponse MapProjectDetail(CurriculumProjectDetailResult project) =>
        new() { Id = project.Id, Name = project.Name, Description = project.Description, YearId = project.YearId };

    internal static CurriculumTermListItemResponse MapTermListItem(CurriculumTermListItemResult term) =>
        new() { Id = term.Id, Name = term.Name, StartDate = term.StartDate, EndDate = term.EndDate, ProjectId = term.ProjectId };

    internal static CurriculumTermDetailResponse MapTermDetail(CurriculumTermDetailResult term) =>
        new() { Id = term.Id, Name = term.Name, StartDate = term.StartDate, EndDate = term.EndDate, ProjectId = term.ProjectId };

    internal static SubjectGroupListItemResponse MapSubjectGroupListItem(SubjectGroupListItemResult group) =>
        new() { Id = group.Id, Name = group.Name, TermId = group.TermId };

    internal static SubjectGroupDetailResponse MapSubjectGroupDetail(SubjectGroupDetailResult group) =>
        new() { Id = group.Id, Name = group.Name, TermId = group.TermId };

    internal static SubjectListItemResponse MapSubjectListItem(SubjectListItemResult subject) =>
        new() { Id = subject.Id, Name = subject.Name, Description = subject.Description, Status = subject.Status, SubjectGroupId = subject.SubjectGroupId };

    internal static SubjectDetailResponse MapSubjectDetail(SubjectDetailResult subject) =>
        new() { Id = subject.Id, Name = subject.Name, Description = subject.Description, Status = subject.Status, SubjectGroupId = subject.SubjectGroupId, ArchivedWithFolder = subject.ArchivedWithFolder };

    internal static UnitListItemResponse MapUnitListItem(UnitListItemResult unit) =>
        new() { Id = unit.Id, Name = unit.Name, SubjectId = unit.SubjectId };

    internal static UnitDetailResponse MapUnitDetail(UnitDetailResult unit) =>
        new() { Id = unit.Id, Name = unit.Name, SubjectId = unit.SubjectId };

    internal static LessonListItemResponse MapLessonListItem(LessonListItemResult lesson) =>
        new() { Id = lesson.Id, Name = lesson.Name, UnitId = lesson.UnitId };

    internal static LessonDetailResponse MapLessonDetail(LessonDetailResult lesson) =>
        new() { Id = lesson.Id, Name = lesson.Name, UnitId = lesson.UnitId };

    internal static LearningObjectiveListItemResponse MapLearningObjectiveListItem(LearningObjectiveListItemResult objective) =>
        new()
        {
            Id = objective.Id,
            Name = objective.Name,
            Tag = objective.Tag,
            Template = objective.Template,
            Environment = objective.Environment,
            CreateAt = objective.CreateAt,
            StartedAt = objective.StartedAt,
            DoneAt = objective.DoneAt,
            LessonId = objective.LessonId,
            SchemaId = objective.SchemaId
        };

    internal static LearningObjectiveDetailResponse MapLearningObjectiveDetail(LearningObjectiveDetailResult objective) =>
        new()
        {
            Id = objective.Id,
            Name = objective.Name,
            Tag = objective.Tag,
            Template = objective.Template,
            Environment = objective.Environment,
            CreateAt = objective.CreateAt,
            StartedAt = objective.StartedAt,
            DoneAt = objective.DoneAt,
            LessonId = objective.LessonId,
            SchemaId = objective.SchemaId
        };

    internal static SubjectUserResponse MapSubjectUser(SubjectUserResult user) =>
        new() { Id = user.Id, Name = user.Name };

    internal static YearTreeResponse MapYearTree(YearTreeResult tree) =>
        new()
        {
            Id = tree.Id,
            Name = tree.Name,
            Description = tree.Description,
            Projects = tree.Projects.Select(project => new YearTreeProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Terms = project.Terms.Select(term => new YearTreeTermResponse
                {
                    Id = term.Id,
                    Name = term.Name,
                    StartDate = term.StartDate,
                    EndDate = term.EndDate,
                    SubjectGroups = term.SubjectGroups.Select(group => new YearTreeSubjectGroupResponse
                    {
                        Id = group.Id,
                        Name = group.Name,
                        Subjects = group.Subjects.Select(subject => new YearTreeSubjectResponse
                        {
                            Id = subject.Id,
                            Name = subject.Name,
                            Description = subject.Description,
                            Status = subject.Status
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).ToList()
        };
}
