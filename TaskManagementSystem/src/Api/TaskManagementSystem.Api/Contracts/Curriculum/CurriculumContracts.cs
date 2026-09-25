using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Api.Contracts.Curriculum;

public sealed class CreateAcademicYearRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class UpdateAcademicYearRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class AcademicYearListItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class AcademicYearDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class CurriculumProjectListItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int YearId { get; set; }
}

public sealed class CurriculumProjectDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int YearId { get; set; }
}

public sealed class CreateTermRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class UpdateTermRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class CurriculumTermListItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int ProjectId { get; set; }
}

public sealed class CurriculumTermDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int ProjectId { get; set; }
}

public sealed class CreateSubjectGroupRequest
{
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateSubjectGroupRequest
{
    public string Name { get; set; } = string.Empty;
}

public sealed class SubjectGroupListItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TermId { get; set; }
}

public sealed class SubjectGroupDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TermId { get; set; }
}

public sealed class CreateSubjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class UpdateSubjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class UpdateSubjectStatusRequest
{
    public SubjectStatus Status { get; set; }
}

public sealed class SubjectListItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SubjectStatus Status { get; set; }
    public int SubjectGroupId { get; set; }
}

public sealed class SubjectDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SubjectStatus Status { get; set; }
    public int SubjectGroupId { get; set; }
    public bool ArchivedWithFolder { get; set; }
}

public sealed class CreateUnitRequest
{
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateUnitRequest
{
    public string Name { get; set; } = string.Empty;
}

public sealed class UnitListItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SubjectId { get; set; }
}

public sealed class UnitDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SubjectId { get; set; }
}

public sealed class CreateLessonRequest
{
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateLessonRequest
{
    public string Name { get; set; } = string.Empty;
}

public sealed class LessonListItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UnitId { get; set; }
}

public sealed class LessonDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UnitId { get; set; }
}

public sealed class CreateLearningObjectiveRequest
{
    public int SchemaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}

public sealed class UpdateLearningObjectiveRequest
{
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? DoneAt { get; set; }
}

public sealed class LearningObjectiveListItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public DateTime CreateAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? DoneAt { get; set; }
    public int LessonId { get; set; }
    public int SchemaId { get; set; }
}

public sealed class LearningObjectiveDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public DateTime CreateAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? DoneAt { get; set; }
    public int LessonId { get; set; }
    public int SchemaId { get; set; }
}

public sealed class SubjectUserResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class AssignSubjectUsersRequest
{
    public IReadOnlyList<int> UserIds { get; set; } = [];
}

public sealed class UnassignSubjectUsersRequest
{
    public IReadOnlyList<int> UserIds { get; set; } = [];
}

public sealed class YearTreeResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyList<YearTreeProjectResponse> Projects { get; set; } = [];
}

public sealed class YearTreeProjectResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyList<YearTreeTermResponse> Terms { get; set; } = [];
}

public sealed class YearTreeTermResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public IReadOnlyList<YearTreeSubjectGroupResponse> SubjectGroups { get; set; } = [];
}

public sealed class YearTreeSubjectGroupResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<YearTreeSubjectResponse> Subjects { get; set; } = [];
}

public sealed class YearTreeSubjectResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SubjectStatus Status { get; set; }
}

public sealed class SubjectCatalogListItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public SubjectStatus Status { get; set; }
    public int ProgressPercent { get; set; }
}

public sealed class SubjectCatalogPageResponse
{
    public IReadOnlyList<SubjectCatalogListItemResponse> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public sealed class SubjectCatalogFilterOptionsResponse
{
    public IReadOnlyList<string> Years { get; set; } = [];
    public IReadOnlyList<string> Terms { get; set; } = [];
}
