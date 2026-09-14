using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.Curriculum.Application;

public interface ICurriculumUnitOfWork : IUnitOfWork
{
    IAcademicYearRepository AcademicYears { get; }
    ICurriculumProjectRepository Projects { get; }
    ICurriculumTermRepository Terms { get; }
    ISubjectGroupRepository SubjectGroups { get; }
    ISubjectRepository Subjects { get; }
    IUnitRepository Units { get; }
    ILessonRepository Lessons { get; }
    ILearningObjectiveRepository LearningObjectives { get; }
    ISubjectUserAssignmentRepository SubjectUserAssignments { get; }
}
