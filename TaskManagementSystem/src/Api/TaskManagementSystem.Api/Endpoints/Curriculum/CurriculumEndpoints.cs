using MediatR;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Curriculum.Features;
using TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.ArchiveAcademicYear;
using TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.CreateAcademicYear;
using TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.GetAcademicYearById;
using TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.ListAcademicYears;
using TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.UpdateAcademicYear;
using TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.ArchiveLearningObjective;
using TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.CreateLearningObjective;
using TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.GetLearningObjectiveById;
using TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.ListLearningObjectivesByLesson;
using TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.UpdateLearningObjective;
using TaskManagementSystem.Modules.Curriculum.Features.Lessons.ArchiveLesson;
using TaskManagementSystem.Modules.Curriculum.Features.Lessons.CreateLesson;
using TaskManagementSystem.Modules.Curriculum.Features.Lessons.GetLessonById;
using TaskManagementSystem.Modules.Curriculum.Features.Lessons.ListLessonsByUnit;
using TaskManagementSystem.Modules.Curriculum.Features.Lessons.UpdateLesson;
using TaskManagementSystem.Modules.Curriculum.Features.Projects.ArchiveProject;
using TaskManagementSystem.Modules.Curriculum.Features.Projects.CreateProject;
using TaskManagementSystem.Modules.Curriculum.Features.Projects.GetProjectById;
using TaskManagementSystem.Modules.Curriculum.Features.Projects.ListProjectsByYear;
using TaskManagementSystem.Modules.Curriculum.Features.Projects.UpdateProject;
using TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.ArchiveSubjectGroup;
using TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.CreateSubjectGroup;
using TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.GetSubjectGroupById;
using TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.ListSubjectGroupsByTerm;
using TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.UpdateSubjectGroup;
using TaskManagementSystem.Modules.Curriculum.Features.Subjects.ArchiveSubject;
using TaskManagementSystem.Modules.Curriculum.Features.Subjects.AssignSubjectUsers;
using TaskManagementSystem.Modules.Curriculum.Features.Subjects.CreateSubject;
using TaskManagementSystem.Modules.Curriculum.Features.Subjects.GetSubjectById;
using TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectUsers;
using TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectsBySubjectGroup;
using TaskManagementSystem.Modules.Curriculum.Features.Subjects.UnassignSubjectUsers;
using TaskManagementSystem.Modules.Curriculum.Features.Subjects.UpdateSubject;
using TaskManagementSystem.Modules.Curriculum.Features.Subjects.UpdateSubjectStatus;
using TaskManagementSystem.Modules.Curriculum.Features.Terms.ArchiveTerm;
using TaskManagementSystem.Modules.Curriculum.Features.Terms.CreateTerm;
using TaskManagementSystem.Modules.Curriculum.Features.Terms.GetTermById;
using TaskManagementSystem.Modules.Curriculum.Features.Terms.ListTermsByProject;
using TaskManagementSystem.Modules.Curriculum.Features.Terms.UpdateTerm;
using TaskManagementSystem.Modules.Curriculum.Features.Units.ArchiveUnit;
using TaskManagementSystem.Modules.Curriculum.Features.Units.CreateUnit;
using TaskManagementSystem.Modules.Curriculum.Features.Units.GetUnitById;
using TaskManagementSystem.Modules.Curriculum.Features.Units.ListUnitsBySubject;
using TaskManagementSystem.Modules.Curriculum.Features.Units.UpdateUnit;
using TaskManagementSystem.Modules.Curriculum.Features.YearTree.GetYearTree;

namespace TaskManagementSystem.Api.Endpoints.Curriculum;

public static class CurriculumEndpoints
{
    public static RouteGroupBuilder MapCurriculumEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/curriculum").WithTags("Curriculum").RequireAuthorization();

        var years = group.MapGroup("/years");
        years.MapGet("", ListAcademicYearsAsync);
        years.MapGet("/{id:int}", GetAcademicYearByIdAsync);
        years.MapPost("", CreateAcademicYearAsync);
        years.MapPut("/{id:int}", UpdateAcademicYearAsync);
        years.MapDelete("/{id:int}", ArchiveAcademicYearAsync);
        years.MapGet("/{yearId:int}/tree", GetYearTreeAsync);
        years.MapGet("/{yearId:int}/projects", ListProjectsByYearAsync);
        years.MapPost("/{yearId:int}/projects", CreateProjectAsync);

        var projects = group.MapGroup("/projects");
        projects.MapGet("/{id:int}", GetProjectByIdAsync);
        projects.MapPut("/{id:int}", UpdateProjectAsync);
        projects.MapDelete("/{id:int}", ArchiveProjectAsync);
        projects.MapGet("/{projectId:int}/terms", ListTermsByProjectAsync);
        projects.MapPost("/{projectId:int}/terms", CreateTermAsync);

        var terms = group.MapGroup("/terms");
        terms.MapGet("/{id:int}", GetTermByIdAsync);
        terms.MapPut("/{id:int}", UpdateTermAsync);
        terms.MapDelete("/{id:int}", ArchiveTermAsync);
        terms.MapGet("/{termId:int}/subject-groups", ListSubjectGroupsByTermAsync);
        terms.MapPost("/{termId:int}/subject-groups", CreateSubjectGroupAsync);

        var subjectGroups = group.MapGroup("/subject-groups");
        subjectGroups.MapGet("/{id:int}", GetSubjectGroupByIdAsync);
        subjectGroups.MapPut("/{id:int}", UpdateSubjectGroupAsync);
        subjectGroups.MapDelete("/{id:int}", ArchiveSubjectGroupAsync);
        subjectGroups.MapGet("/{subjectGroupId:int}/subjects", ListSubjectsBySubjectGroupAsync);
        subjectGroups.MapPost("/{subjectGroupId:int}/subjects", CreateSubjectAsync);

        var subjects = group.MapGroup("/subjects");
        subjects.MapGet("/{id:int}", GetSubjectByIdAsync);
        subjects.MapPut("/{id:int}", UpdateSubjectAsync);
        subjects.MapDelete("/{id:int}", ArchiveSubjectAsync);
        subjects.MapPut("/{id:int}/status", UpdateSubjectStatusAsync);
        subjects.MapGet("/{id:int}/users", ListSubjectUsersAsync);
        subjects.MapPost("/{id:int}/users", AssignSubjectUsersAsync);
        subjects.MapPost("/{id:int}/users/unassign", UnassignSubjectUsersAsync);
        subjects.MapGet("/{subjectId:int}/units", ListUnitsBySubjectAsync);
        subjects.MapPost("/{subjectId:int}/units", CreateUnitAsync);

        var units = group.MapGroup("/units");
        units.MapGet("/{id:int}", GetUnitByIdAsync);
        units.MapPut("/{id:int}", UpdateUnitAsync);
        units.MapDelete("/{id:int}", ArchiveUnitAsync);
        units.MapGet("/{unitId:int}/lessons", ListLessonsByUnitAsync);
        units.MapPost("/{unitId:int}/lessons", CreateLessonAsync);

        var lessons = group.MapGroup("/lessons");
        lessons.MapGet("/{id:int}", GetLessonByIdAsync);
        lessons.MapPut("/{id:int}", UpdateLessonAsync);
        lessons.MapDelete("/{id:int}", ArchiveLessonAsync);
        lessons.MapGet("/{lessonId:int}/learning-objectives", ListLearningObjectivesByLessonAsync);
        lessons.MapPost("/{lessonId:int}/learning-objectives", CreateLearningObjectiveAsync);

        var learningObjectives = group.MapGroup("/learning-objectives");
        learningObjectives.MapGet("/{id:int}", GetLearningObjectiveByIdAsync);
        learningObjectives.MapPut("/{id:int}", UpdateLearningObjectiveAsync);
        learningObjectives.MapDelete("/{id:int}", ArchiveLearningObjectiveAsync);

        return group;
    }

    private static async Task<IResult> ListAcademicYearsAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListAcademicYearsQuery(), cancellationToken);
        return result.ToHttpResult(years => Results.Ok(years.Select(MapAcademicYearListItem).ToList()));
    }

    private static async Task<IResult> GetAcademicYearByIdAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAcademicYearByIdQuery(id), cancellationToken);
        return result.ToHttpResult(year => Results.Ok(MapAcademicYearDetail(year)));
    }

    private static async Task<IResult> CreateAcademicYearAsync(CreateAcademicYearRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateAcademicYearCommand(request.Name, request.Description), cancellationToken);
        return result.ToHttpResult(year => Results.Created($"/curriculum/years/{year.Id}", MapAcademicYearDetail(year)));
    }

    private static async Task<IResult> UpdateAcademicYearAsync(int id, UpdateAcademicYearRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAcademicYearCommand(id, request.Name, request.Description), cancellationToken);
        return result.ToHttpResult(year => Results.Ok(MapAcademicYearDetail(year)));
    }

    private static async Task<IResult> ArchiveAcademicYearAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveAcademicYearCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> GetYearTreeAsync(int yearId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetYearTreeQuery(yearId), cancellationToken);
        return result.ToHttpResult(tree => Results.Ok(MapYearTree(tree)));
    }

    private static async Task<IResult> ListProjectsByYearAsync(int yearId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListProjectsByYearQuery(yearId), cancellationToken);
        return result.ToHttpResult(projects => Results.Ok(projects.Select(MapProjectListItem).ToList()));
    }

    private static async Task<IResult> CreateProjectAsync(int yearId, CreateProjectRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateProjectCommand(yearId, request.Name, request.Description), cancellationToken);
        return result.ToHttpResult(project => Results.Created($"/curriculum/projects/{project.Id}", MapProjectDetail(project)));
    }

    private static async Task<IResult> GetProjectByIdAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProjectByIdQuery(id), cancellationToken);
        return result.ToHttpResult(project => Results.Ok(MapProjectDetail(project)));
    }

    private static async Task<IResult> UpdateProjectAsync(int id, UpdateProjectRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateProjectCommand(id, request.Name, request.Description), cancellationToken);
        return result.ToHttpResult(project => Results.Ok(MapProjectDetail(project)));
    }

    private static async Task<IResult> ArchiveProjectAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveProjectCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> ListTermsByProjectAsync(int projectId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListTermsByProjectQuery(projectId), cancellationToken);
        return result.ToHttpResult(terms => Results.Ok(terms.Select(MapTermListItem).ToList()));
    }

    private static async Task<IResult> CreateTermAsync(int projectId, CreateTermRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateTermCommand(projectId, request.Name, request.StartDate, request.EndDate), cancellationToken);
        return result.ToHttpResult(term => Results.Created($"/curriculum/terms/{term.Id}", MapTermDetail(term)));
    }

    private static async Task<IResult> GetTermByIdAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTermByIdQuery(id), cancellationToken);
        return result.ToHttpResult(term => Results.Ok(MapTermDetail(term)));
    }

    private static async Task<IResult> UpdateTermAsync(int id, UpdateTermRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateTermCommand(id, request.Name, request.StartDate, request.EndDate), cancellationToken);
        return result.ToHttpResult(term => Results.Ok(MapTermDetail(term)));
    }

    private static async Task<IResult> ArchiveTermAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveTermCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> ListSubjectGroupsByTermAsync(int termId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSubjectGroupsByTermQuery(termId), cancellationToken);
        return result.ToHttpResult(groups => Results.Ok(groups.Select(MapSubjectGroupListItem).ToList()));
    }

    private static async Task<IResult> CreateSubjectGroupAsync(int termId, CreateSubjectGroupRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateSubjectGroupCommand(termId, request.Name), cancellationToken);
        return result.ToHttpResult(group => Results.Created($"/curriculum/subject-groups/{group.Id}", MapSubjectGroupDetail(group)));
    }

    private static async Task<IResult> GetSubjectGroupByIdAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSubjectGroupByIdQuery(id), cancellationToken);
        return result.ToHttpResult(group => Results.Ok(MapSubjectGroupDetail(group)));
    }

    private static async Task<IResult> UpdateSubjectGroupAsync(int id, UpdateSubjectGroupRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSubjectGroupCommand(id, request.Name), cancellationToken);
        return result.ToHttpResult(group => Results.Ok(MapSubjectGroupDetail(group)));
    }

    private static async Task<IResult> ArchiveSubjectGroupAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveSubjectGroupCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> ListSubjectsBySubjectGroupAsync(int subjectGroupId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSubjectsBySubjectGroupQuery(subjectGroupId), cancellationToken);
        return result.ToHttpResult(subjects => Results.Ok(subjects.Select(MapSubjectListItem).ToList()));
    }

    private static async Task<IResult> CreateSubjectAsync(int subjectGroupId, CreateSubjectRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateSubjectCommand(subjectGroupId, request.Name, request.Description), cancellationToken);
        return result.ToHttpResult(subject => Results.Created($"/curriculum/subjects/{subject.Id}", MapSubjectDetail(subject)));
    }

    private static async Task<IResult> GetSubjectByIdAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSubjectByIdQuery(id), cancellationToken);
        return result.ToHttpResult(subject => Results.Ok(MapSubjectDetail(subject)));
    }

    private static async Task<IResult> UpdateSubjectAsync(int id, UpdateSubjectRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSubjectCommand(id, request.Name, request.Description), cancellationToken);
        return result.ToHttpResult(subject => Results.Ok(MapSubjectDetail(subject)));
    }

    private static async Task<IResult> ArchiveSubjectAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveSubjectCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> UpdateSubjectStatusAsync(int id, UpdateSubjectStatusRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSubjectStatusCommand(id, request.Status), cancellationToken);
        return result.ToHttpResult(subject => Results.Ok(MapSubjectDetail(subject)));
    }

    private static async Task<IResult> ListSubjectUsersAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSubjectUsersQuery(id), cancellationToken);
        return result.ToHttpResult(users => Results.Ok(users.Select(MapSubjectUser).ToList()));
    }

    private static async Task<IResult> AssignSubjectUsersAsync(int id, AssignSubjectUsersRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AssignSubjectUsersCommand(id, request.UserIds), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> UnassignSubjectUsersAsync(int id, UnassignSubjectUsersRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UnassignSubjectUsersCommand(id, request.UserIds), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> ListUnitsBySubjectAsync(int subjectId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListUnitsBySubjectQuery(subjectId), cancellationToken);
        return result.ToHttpResult(units => Results.Ok(units.Select(MapUnitListItem).ToList()));
    }

    private static async Task<IResult> CreateUnitAsync(int subjectId, CreateUnitRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateUnitCommand(subjectId, request.Name), cancellationToken);
        return result.ToHttpResult(unit => Results.Created($"/curriculum/units/{unit.Id}", MapUnitDetail(unit)));
    }

    private static async Task<IResult> GetUnitByIdAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUnitByIdQuery(id), cancellationToken);
        return result.ToHttpResult(unit => Results.Ok(MapUnitDetail(unit)));
    }

    private static async Task<IResult> UpdateUnitAsync(int id, UpdateUnitRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateUnitCommand(id, request.Name), cancellationToken);
        return result.ToHttpResult(unit => Results.Ok(MapUnitDetail(unit)));
    }

    private static async Task<IResult> ArchiveUnitAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveUnitCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> ListLessonsByUnitAsync(int unitId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListLessonsByUnitQuery(unitId), cancellationToken);
        return result.ToHttpResult(lessons => Results.Ok(lessons.Select(MapLessonListItem).ToList()));
    }

    private static async Task<IResult> CreateLessonAsync(int unitId, CreateLessonRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateLessonCommand(unitId, request.Name), cancellationToken);
        return result.ToHttpResult(lesson => Results.Created($"/curriculum/lessons/{lesson.Id}", MapLessonDetail(lesson)));
    }

    private static async Task<IResult> GetLessonByIdAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLessonByIdQuery(id), cancellationToken);
        return result.ToHttpResult(lesson => Results.Ok(MapLessonDetail(lesson)));
    }

    private static async Task<IResult> UpdateLessonAsync(int id, UpdateLessonRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateLessonCommand(id, request.Name), cancellationToken);
        return result.ToHttpResult(lesson => Results.Ok(MapLessonDetail(lesson)));
    }

    private static async Task<IResult> ArchiveLessonAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveLessonCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> ListLearningObjectivesByLessonAsync(int lessonId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListLearningObjectivesByLessonQuery(lessonId), cancellationToken);
        return result.ToHttpResult(objectives => Results.Ok(objectives.Select(MapLearningObjectiveListItem).ToList()));
    }

    private static async Task<IResult> CreateLearningObjectiveAsync(int lessonId, CreateLearningObjectiveRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateLearningObjectiveCommand(lessonId, request.SchemaId, request.Name, request.Tag, request.Template, request.Environment),
            cancellationToken);
        return result.ToHttpResult(objective => Results.Created($"/curriculum/learning-objectives/{objective.Id}", MapLearningObjectiveDetail(objective)));
    }

    private static async Task<IResult> GetLearningObjectiveByIdAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLearningObjectiveByIdQuery(id), cancellationToken);
        return result.ToHttpResult(objective => Results.Ok(MapLearningObjectiveDetail(objective)));
    }

    private static async Task<IResult> UpdateLearningObjectiveAsync(int id, UpdateLearningObjectiveRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateLearningObjectiveCommand(id, request.Name, request.Tag, request.Template, request.Environment, request.StartedAt, request.DoneAt),
            cancellationToken);
        return result.ToHttpResult(objective => Results.Ok(MapLearningObjectiveDetail(objective)));
    }

    private static async Task<IResult> ArchiveLearningObjectiveAsync(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveLearningObjectiveCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static AcademicYearListItemResponse MapAcademicYearListItem(AcademicYearListItemResult year) =>
        new() { Id = year.Id, Name = year.Name, Description = year.Description };

    private static AcademicYearDetailResponse MapAcademicYearDetail(AcademicYearDetailResult year) =>
        new() { Id = year.Id, Name = year.Name, Description = year.Description };

    private static CurriculumProjectListItemResponse MapProjectListItem(CurriculumProjectListItemResult project) =>
        new() { Id = project.Id, Name = project.Name, Description = project.Description, YearId = project.YearId };

    private static CurriculumProjectDetailResponse MapProjectDetail(CurriculumProjectDetailResult project) =>
        new() { Id = project.Id, Name = project.Name, Description = project.Description, YearId = project.YearId };

    private static CurriculumTermListItemResponse MapTermListItem(CurriculumTermListItemResult term) =>
        new() { Id = term.Id, Name = term.Name, StartDate = term.StartDate, EndDate = term.EndDate, ProjectId = term.ProjectId };

    private static CurriculumTermDetailResponse MapTermDetail(CurriculumTermDetailResult term) =>
        new() { Id = term.Id, Name = term.Name, StartDate = term.StartDate, EndDate = term.EndDate, ProjectId = term.ProjectId };

    private static SubjectGroupListItemResponse MapSubjectGroupListItem(SubjectGroupListItemResult group) =>
        new() { Id = group.Id, Name = group.Name, TermId = group.TermId };

    private static SubjectGroupDetailResponse MapSubjectGroupDetail(SubjectGroupDetailResult group) =>
        new() { Id = group.Id, Name = group.Name, TermId = group.TermId };

    private static SubjectListItemResponse MapSubjectListItem(SubjectListItemResult subject) =>
        new() { Id = subject.Id, Name = subject.Name, Description = subject.Description, Status = subject.Status, SubjectGroupId = subject.SubjectGroupId };

    private static SubjectDetailResponse MapSubjectDetail(SubjectDetailResult subject) =>
        new() { Id = subject.Id, Name = subject.Name, Description = subject.Description, Status = subject.Status, SubjectGroupId = subject.SubjectGroupId, ArchivedWithFolder = subject.ArchivedWithFolder };

    private static UnitListItemResponse MapUnitListItem(UnitListItemResult unit) =>
        new() { Id = unit.Id, Name = unit.Name, SubjectId = unit.SubjectId };

    private static UnitDetailResponse MapUnitDetail(UnitDetailResult unit) =>
        new() { Id = unit.Id, Name = unit.Name, SubjectId = unit.SubjectId };

    private static LessonListItemResponse MapLessonListItem(LessonListItemResult lesson) =>
        new() { Id = lesson.Id, Name = lesson.Name, UnitId = lesson.UnitId };

    private static LessonDetailResponse MapLessonDetail(LessonDetailResult lesson) =>
        new() { Id = lesson.Id, Name = lesson.Name, UnitId = lesson.UnitId };

    private static LearningObjectiveListItemResponse MapLearningObjectiveListItem(LearningObjectiveListItemResult objective) =>
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

    private static LearningObjectiveDetailResponse MapLearningObjectiveDetail(LearningObjectiveDetailResult objective) =>
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

    private static SubjectUserResponse MapSubjectUser(SubjectUserResult user) =>
        new() { Id = user.Id, Name = user.Name };

    private static YearTreeResponse MapYearTree(YearTreeResult tree) =>
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
