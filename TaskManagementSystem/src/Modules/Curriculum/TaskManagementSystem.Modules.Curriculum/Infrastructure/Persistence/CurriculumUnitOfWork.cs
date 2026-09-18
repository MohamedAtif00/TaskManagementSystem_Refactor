using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence;

internal sealed class CurriculumUnitOfWork(
    CurriculumDbContext context,
    IDomainEventDispatcher domainEventDispatcher)
    : UnitOfWork<CurriculumDbContext>(context, domainEventDispatcher), ICurriculumUnitOfWork
{
    private readonly Lazy<AcademicYearRepository> _academicYears =
        LazyRepositoryFactory.Create(() => new AcademicYearRepository(context));

    private readonly Lazy<CurriculumProjectRepository> _projects =
        LazyRepositoryFactory.Create(() => new CurriculumProjectRepository(context));

    private readonly Lazy<CurriculumTermRepository> _terms =
        LazyRepositoryFactory.Create(() => new CurriculumTermRepository(context));

    private readonly Lazy<SubjectGroupRepository> _subjectGroups =
        LazyRepositoryFactory.Create(() => new SubjectGroupRepository(context));

    private readonly Lazy<SubjectRepository> _subjects =
        LazyRepositoryFactory.Create(() => new SubjectRepository(context));

    private readonly Lazy<UnitRepository> _units =
        LazyRepositoryFactory.Create(() => new UnitRepository(context));

    private readonly Lazy<LessonRepository> _lessons =
        LazyRepositoryFactory.Create(() => new LessonRepository(context));

    private readonly Lazy<LearningObjectiveRepository> _learningObjectives =
        LazyRepositoryFactory.Create(() => new LearningObjectiveRepository(context));

    private readonly Lazy<SubjectUserAssignmentRepository> _subjectUserAssignments =
        LazyRepositoryFactory.Create(() => new SubjectUserAssignmentRepository(context));

    public IAcademicYearRepository AcademicYears => _academicYears.Value;
    public ICurriculumProjectRepository Projects => _projects.Value;
    public ICurriculumTermRepository Terms => _terms.Value;
    public ISubjectGroupRepository SubjectGroups => _subjectGroups.Value;
    public ISubjectRepository Subjects => _subjects.Value;
    public IUnitRepository Units => _units.Value;
    public ILessonRepository Lessons => _lessons.Value;
    public ILearningObjectiveRepository LearningObjectives => _learningObjectives.Value;
    public ISubjectUserAssignmentRepository SubjectUserAssignments => _subjectUserAssignments.Value;
}
