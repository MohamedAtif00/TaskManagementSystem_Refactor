using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;
using TaskManagementSystem.Modules.Curriculum.Features;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

public sealed class YearTreeQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<YearTreeResult?> GetYearTreeAsync(
        int yearId,
        int[]? subjectStatuses = null,
        CancellationToken cancellationToken = default)
    {
        const string yearSql = """
            SELECT [Id], [Name], [Description]
            FROM [curriculum].[AcademicYears]
            WHERE [Id] = @YearId AND [Archived] = 0
            """;

        using var connection = connectionFactory.GetOpenConnection();

        var year = await connection.QuerySingleOrDefaultAsync<YearRow>(
            new CommandDefinition(yearSql, new { YearId = yearId }, cancellationToken: cancellationToken));

        if (year is null)
        {
            return null;
        }

        const string projectsSql = """
            SELECT [Id], [Name], [Description], [YearId]
            FROM [curriculum].[CurriculumProjects]
            WHERE [YearId] = @YearId AND [Archived] = 0
            ORDER BY [Name]
            """;

        var projects = (await connection.QueryAsync<ProjectRow>(
            new CommandDefinition(projectsSql, new { YearId = yearId }, cancellationToken: cancellationToken))).ToList();

        if (projects.Count == 0)
        {
            return new YearTreeResult(year.Id, year.Name, year.Description, []);
        }

        const string termsSql = """
            SELECT [Id], [Name], [StartDate], [EndDate], [ProjectId]
            FROM [curriculum].[CurriculumTerms]
            WHERE [ProjectId] IN @ProjectIds AND [Archived] = 0
            ORDER BY [Name]
            """;

        var projectIds = projects.Select(project => project.Id).ToArray();
        var terms = (await connection.QueryAsync<TermRow>(
            new CommandDefinition(termsSql, new { ProjectIds = projectIds }, cancellationToken: cancellationToken))).ToList();

        const string groupsSql = """
            SELECT [Id], [Name], [TermId]
            FROM [curriculum].[SubjectGroups]
            WHERE [TermId] IN @TermIds AND [Archived] = 0
            ORDER BY [Name]
            """;

        var termIds = terms.Select(term => term.Id).ToArray();
        var groups = termIds.Length == 0
            ? []
            : (await connection.QueryAsync<GroupRow>(
                new CommandDefinition(groupsSql, new { TermIds = termIds }, cancellationToken: cancellationToken))).ToList();

        var subjectsSql = subjectStatuses is { Length: > 0 }
            ? """
                SELECT [Id], [Name], [Description], [Status], [SubjectGroupId]
                FROM [curriculum].[Subjects]
                WHERE [SubjectGroupId] IN @GroupIds AND [Archived] = 0 AND [Status] IN @Statuses
                ORDER BY [Name]
                """
            : """
                SELECT [Id], [Name], [Description], [Status], [SubjectGroupId]
                FROM [curriculum].[Subjects]
                WHERE [SubjectGroupId] IN @GroupIds AND [Archived] = 0
                ORDER BY [Name]
                """;

        var groupIds = groups.Select(group => group.Id).ToArray();
        var subjects = groupIds.Length == 0
            ? []
            : (await connection.QueryAsync<SubjectRow>(
                new CommandDefinition(
                    subjectsSql,
                    new { GroupIds = groupIds, Statuses = subjectStatuses },
                    cancellationToken: cancellationToken))).ToList();

        var subjectsByGroup = subjects.GroupBy(subject => subject.SubjectGroupId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var groupsByTerm = groups.GroupBy(group => group.TermId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var termsByProject = terms.GroupBy(term => term.ProjectId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var filterEmpty = subjectStatuses is { Length: > 0 };

        var projectResults = projects.Select(project =>
        {
            var termResults = termsByProject.GetValueOrDefault(project.Id, [])
                .Select(term =>
                {
                    var groupResults = groupsByTerm.GetValueOrDefault(term.Id, [])
                        .Select(group =>
                        {
                            var subjectResults = subjectsByGroup.GetValueOrDefault(group.Id, [])
                                .Select(subject => new YearTreeSubjectResult(
                                    subject.Id,
                                    subject.Name,
                                    subject.Description,
                                    (SubjectStatus)subject.Status))
                                .ToList();

                            return new YearTreeSubjectGroupResult(group.Id, group.Name, subjectResults);
                        })
                        .Where(group => !filterEmpty || group.Subjects.Count > 0)
                        .ToList();

                    return new YearTreeTermResult(term.Id, term.Name, term.StartDate, term.EndDate, groupResults);
                })
                .Where(term => !filterEmpty || term.SubjectGroups.Count > 0)
                .ToList();

            return new YearTreeProjectResult(project.Id, project.Name, project.Description, termResults);
        })
        .Where(project => !filterEmpty || project.Terms.Count > 0)
        .ToList();

        return new YearTreeResult(year.Id, year.Name, year.Description, projectResults);
    }

    private sealed record YearRow(int Id, string Name, string? Description);
    private sealed record ProjectRow(int Id, string Name, string? Description, int YearId);
    private sealed record TermRow(int Id, string Name, DateTime? StartDate, DateTime? EndDate, int ProjectId);
    private sealed record GroupRow(int Id, string Name, int TermId);
    private sealed record SubjectRow(int Id, string Name, string Description, int Status, int SubjectGroupId);
}
