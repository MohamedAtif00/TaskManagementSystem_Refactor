using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Curriculum.Features;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

public sealed class SubjectOutlineQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<SubjectOutlineResult?> GetBySubjectAsync(
        int subjectId,
        CancellationToken cancellationToken = default)
    {
        const string subjectSql = """
            SELECT [Id]
            FROM [curriculum].[Subjects]
            WHERE [Id] = @SubjectId AND [Archived] = 0
            """;

        const string outlineSql = """
            SELECT
                u.[Id] AS UnitId,
                u.[Name] AS UnitName,
                l.[Id] AS LessonId,
                l.[Name] AS LessonName,
                lo.[Id] AS LearningObjectiveId,
                lo.[Name] AS LearningObjectiveName,
                lo.[Tag],
                lo.[Template],
                lo.[Environment]
            FROM [curriculum].[Units] u
            LEFT JOIN [curriculum].[Lessons] l
                ON l.[UnitId] = u.[Id] AND l.[Archived] = 0
            LEFT JOIN [curriculum].[LearningObjectives] lo
                ON lo.[LessonId] = l.[Id] AND lo.[Archived] = 0
            WHERE u.[SubjectId] = @SubjectId
              AND u.[Archived] = 0
            ORDER BY u.[Name], l.[Name], lo.[Name]
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var subject = await connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(subjectSql, new { SubjectId = subjectId }, cancellationToken: cancellationToken));
        if (subject is null)
        {
            return null;
        }

        var rows = (await connection.QueryAsync<OutlineRow>(
            new CommandDefinition(outlineSql, new { SubjectId = subjectId }, cancellationToken: cancellationToken))).AsList();

        var units = rows
            .GroupBy(row => new UnitKey(row.UnitId, row.UnitName))
            .Select(unit => new SubjectOutlineUnitResult(
                unit.Key.Id,
                unit.Key.Name,
                unit.Where(row => row.LessonId is not null)
                    .GroupBy(row => new LessonKey(row.LessonId!.Value, row.LessonName ?? string.Empty))
                    .Select(lesson => new SubjectOutlineLessonResult(
                        lesson.Key.Id,
                        lesson.Key.Name,
                        lesson.Where(row => row.LearningObjectiveId is not null)
                            .Select(row => new SubjectOutlineLearningObjectiveResult(
                                row.LearningObjectiveId!.Value,
                                row.LearningObjectiveName ?? string.Empty,
                                row.Tag ?? string.Empty,
                                row.Template ?? string.Empty,
                                row.Environment ?? string.Empty,
                                lesson.Key.Id))
                            .ToList()))
                    .ToList()))
            .ToList();

        return new SubjectOutlineResult(units);
    }

    private sealed record UnitKey(int Id, string Name);

    private sealed record LessonKey(int Id, string Name);

    private sealed record OutlineRow(
        int UnitId,
        string UnitName,
        int? LessonId,
        string? LessonName,
        int? LearningObjectiveId,
        string? LearningObjectiveName,
        string? Tag,
        string? Template,
        string? Environment);
}
