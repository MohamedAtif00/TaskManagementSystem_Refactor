using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Application;

public static class CurriculumErrors
{
    public static ResultError AcademicYearNotFound =>
        new("academic_year_not_found", "Academic year not found.");

    public static ResultError CurriculumProjectNotFound =>
        new("curriculum_project_not_found", "Curriculum project not found.");

    public static ResultError CurriculumTermNotFound =>
        new("curriculum_term_not_found", "Curriculum term not found.");

    public static ResultError SubjectGroupNotFound =>
        new("subject_group_not_found", "Subject group not found.");

    public static ResultError SubjectNotFound =>
        new("subject_not_found", "Subject not found.");

    public static ResultError UnitNotFound =>
        new("unit_not_found", "Unit not found.");

    public static ResultError LessonNotFound =>
        new("lesson_not_found", "Lesson not found.");

    public static ResultError LearningObjectiveNotFound =>
        new("learning_objective_not_found", "Learning objective not found.");

    public static ResultError SchemaNotFound =>
        new("schema_not_found", "Schema not found.");

    public static ResultError UserNotFound =>
        new("user_not_found", "User not found.");

    public static ResultError ParentArchived =>
        new("parent_archived", "Parent entity is archived.");
}
