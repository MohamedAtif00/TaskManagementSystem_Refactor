using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features;

public sealed record AcademicYearListItemResult(int Id, string Name, string? Description)
{
    public static AcademicYearListItemResult From(AcademicYear year) =>
        new(year.Id, year.Name, year.Description);
}

public sealed record AcademicYearDetailResult(int Id, string Name, string? Description)
{
    public static AcademicYearDetailResult From(AcademicYear year) =>
        new(year.Id, year.Name, year.Description);
}

public sealed record CurriculumProjectListItemResult(int Id, string Name, string? Description, int YearId)
{
    public static CurriculumProjectListItemResult From(CurriculumProject project) =>
        new(project.Id, project.Name, project.Description, project.YearId);
}

public sealed record CurriculumProjectDetailResult(int Id, string Name, string? Description, int YearId)
{
    public static CurriculumProjectDetailResult From(CurriculumProject project) =>
        new(project.Id, project.Name, project.Description, project.YearId);
}

public sealed record CurriculumTermListItemResult(int Id, string Name, DateTime? StartDate, DateTime? EndDate, int ProjectId)
{
    public static CurriculumTermListItemResult From(CurriculumTerm term) =>
        new(term.Id, term.Name, term.StartDate, term.EndDate, term.ProjectId);
}

public sealed record CurriculumTermDetailResult(int Id, string Name, DateTime? StartDate, DateTime? EndDate, int ProjectId)
{
    public static CurriculumTermDetailResult From(CurriculumTerm term) =>
        new(term.Id, term.Name, term.StartDate, term.EndDate, term.ProjectId);
}

public sealed record SubjectGroupListItemResult(int Id, string Name, int TermId)
{
    public static SubjectGroupListItemResult From(SubjectGroup group) =>
        new(group.Id, group.Name, group.TermId);
}

public sealed record SubjectGroupDetailResult(int Id, string Name, int TermId)
{
    public static SubjectGroupDetailResult From(SubjectGroup group) =>
        new(group.Id, group.Name, group.TermId);
}

public sealed record SubjectListItemResult(int Id, string Name, string Description, SubjectStatus Status, int SubjectGroupId)
{
    public static SubjectListItemResult From(Subject subject) =>
        new(subject.Id, subject.Name, subject.Description, subject.Status, subject.SubjectGroupId);
}

public sealed record SubjectDetailResult(
    int Id,
    string Name,
    string Description,
    SubjectStatus Status,
    int SubjectGroupId,
    bool ArchivedWithFolder)
{
    public static SubjectDetailResult From(Subject subject) =>
        new(subject.Id, subject.Name, subject.Description, subject.Status, subject.SubjectGroupId, subject.ArchivedWithFolder);
}

public sealed record UnitListItemResult(int Id, string Name, int SubjectId)
{
    public static UnitListItemResult From(Unit unit) =>
        new(unit.Id, unit.Name, unit.SubjectId);
}

public sealed record UnitDetailResult(int Id, string Name, int SubjectId)
{
    public static UnitDetailResult From(Unit unit) =>
        new(unit.Id, unit.Name, unit.SubjectId);
}

public sealed record LessonListItemResult(int Id, string Name, int UnitId)
{
    public static LessonListItemResult From(Lesson lesson) =>
        new(lesson.Id, lesson.Name, lesson.UnitId);
}

public sealed record LessonDetailResult(int Id, string Name, int UnitId)
{
    public static LessonDetailResult From(Lesson lesson) =>
        new(lesson.Id, lesson.Name, lesson.UnitId);
}

public sealed record LearningObjectiveListItemResult(
    int Id,
    string Name,
    string Tag,
    string Template,
    string Environment,
    DateTime CreateAt,
    DateTime? StartedAt,
    DateTime? DoneAt,
    int LessonId,
    int SchemaId)
{
    public static LearningObjectiveListItemResult From(LearningObjective objective) =>
        new(
            objective.Id,
            objective.Name,
            objective.Tag,
            objective.Template,
            objective.Environment,
            objective.CreateAt,
            objective.StartedAt,
            objective.DoneAt,
            objective.LessonId,
            objective.SchemaId);
}

public sealed record LearningObjectiveDetailResult(
    int Id,
    string Name,
    string Tag,
    string Template,
    string Environment,
    DateTime CreateAt,
    DateTime? StartedAt,
    DateTime? DoneAt,
    int LessonId,
    int SchemaId)
{
    public static LearningObjectiveDetailResult From(LearningObjective objective) =>
        new(
            objective.Id,
            objective.Name,
            objective.Tag,
            objective.Template,
            objective.Environment,
            objective.CreateAt,
            objective.StartedAt,
            objective.DoneAt,
            objective.LessonId,
            objective.SchemaId);
}

public sealed record SubjectUserResult(int Id, string Name);

public sealed record YearTreeResult(int Id, string Name, string? Description, IReadOnlyList<YearTreeProjectResult> Projects);

public sealed record YearTreeProjectResult(
    int Id,
    string Name,
    string? Description,
    IReadOnlyList<YearTreeTermResult> Terms);

public sealed record YearTreeTermResult(
    int Id,
    string Name,
    DateTime? StartDate,
    DateTime? EndDate,
    IReadOnlyList<YearTreeSubjectGroupResult> SubjectGroups);

public sealed record YearTreeSubjectGroupResult(
    int Id,
    string Name,
    IReadOnlyList<YearTreeSubjectResult> Subjects);

public sealed record YearTreeSubjectResult(int Id, string Name, string Description, SubjectStatus Status);
