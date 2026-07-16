namespace AutomatedTaskSystem.Services.DailyReport;

internal static class DailyReportSql
{
    /// <summary>Minimal CTE for counts, aggregates, and filter dropdowns — no rollback/note joins.</summary>
    internal const string LightBaseCte = """
        WITH ActivityDates AS (
            SELECT TaskId, MAX(TimeStamp) AS ReportDate
            FROM TaskActivities
            WHERE Type IN (@statusDone, @statusRollback, @rollbackActivity)
            GROUP BY TaskId
        ),
        BaseRows AS (
            SELECT
                t.Id AS TaskId,
                CAST(COALESCE(ad.ReportDate, t.CreatedAt) AS date) AS ReportDate,
                g.Name AS Team,
                CASE
                    WHEN s.Name LIKE '%[_]2a' OR s.Name LIKE '%[_]2e' OR LOWER(s.Name) LIKE '%term2%' OR LOWER(s.Name) LIKE '%term_2%' THEN 'Term 2'
                    ELSE 'Term 1'
                END AS Semester,
                CASE
                    WHEN s.Name LIKE 'ara[_]%' THEN 'Arabic'
                    WHEN s.Name LIKE 'eng[_]%' THEN 'English'
                    WHEN s.Name LIKE 'mth[_]%' THEN 'Math (A)'
                    WHEN s.Name LIKE 'sci[_]%' THEN 'Science (A)'
                    WHEN s.Name LIKE 'soc[_]%' THEN 'Social'
                    WHEN s.Name LIKE 'ict[_]%' THEN 'ICT (A)'
                    WHEN s.Name LIKE 'mul[_]%' THEN 'MUL (A)'
                    WHEN s.Name LIKE 'rel[_]%' THEN 'Religion'
                    ELSE 'Other'
                END AS Subjects,
                CASE LOWER(
                    CASE
                        WHEN CHARINDEX('_', s.Name) > 0
                             AND CHARINDEX('_', s.Name, CHARINDEX('_', s.Name) + 1) > CHARINDEX('_', s.Name)
                        THEN SUBSTRING(s.Name, CHARINDEX('_', s.Name) + 1,
                            CHARINDEX('_', s.Name, CHARINDEX('_', s.Name) + 1) - CHARINDEX('_', s.Name) - 1)
                        ELSE ''
                    END)
                    WHEN '1k' THEN 'Kg1' WHEN '2k' THEN 'Kg2'
                    WHEN '1r' THEN 'Grade 1' WHEN '2r' THEN 'Grade 2' WHEN '3r' THEN 'Grade 3'
                    WHEN '4r' THEN 'Grade 4' WHEN '5r' THEN 'Grade 5' WHEN '6r' THEN 'Grade 6'
                    WHEN '7r' THEN 'Grade 7'
                    ELSE CASE WHEN CHARINDEX('_', s.Name) > 0
                         AND CHARINDEX('_', s.Name, CHARINDEX('_', s.Name) + 1) > CHARINDEX('_', s.Name)
                        THEN SUBSTRING(s.Name, CHARINDEX('_', s.Name) + 1,
                            CHARINDEX('_', s.Name, CHARINDEX('_', s.Name) + 1) - CHARINDEX('_', s.Name) - 1)
                        ELSE '' END
                END AS Grade,
                COALESCE(NULLIF(tb.Name, ''), t.Name) AS TaskName,
                CASE
                    WHEN t.Status = @taskRollback OR t.IsRollback = 1 THEN 'Rollback'
                    WHEN t.Status = @taskDone THEN 'Approved'
                    ELSE 'Hold'
                END AS [Status],
                CASE t.Priority
                    WHEN @priorityHigh THEN 'High'
                    WHEN @priorityMedium THEN 'Medium'
                    WHEN @priorityLow THEN 'Low'
                    ELSE ''
                END AS [Priority]
            FROM Tasks t
            INNER JOIN Groups g ON g.Id = t.GroupId
            INNER JOIN LearningObjectives lo ON lo.Id = t.LearningObjectiveId
            INNER JOIN Lessons l ON l.Id = lo.LessonId
            INNER JOIN Units un ON un.Id = l.UnitId
            INNER JOIN Subjects s ON s.Id = un.SubjectId
            LEFT JOIN Steps st ON st.Id = t.StepId
            LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
            LEFT JOIN ActivityDates ad ON ad.TaskId = t.Id
            WHERE t.Archived = 0 AND lo.Archived = 0 AND l.Archived = 0
              AND un.Archived = 0 AND s.Archived = 0
              {DATE_FILTER_BASE}
              {ROLE_FILTER}
        )
        """;

    internal const string BaseCte = """
        WITH ActivityDates AS (
            SELECT TaskId, MAX(TimeStamp) AS ReportDate
            FROM TaskActivities
            WHERE Type IN (@statusDone, @statusRollback, @rollbackActivity)
            GROUP BY TaskId
        ),
        LatestRollback AS (
            SELECT r.TaskId, r.Clarification, tg.Name AS ToGroupName,
                ROW_NUMBER() OVER (PARTITION BY r.TaskId ORDER BY r.Id DESC) AS rn
            FROM Rollbacks r
            LEFT JOIN Tasks tt ON tt.Id = r.ToTaskId
            LEFT JOIN Groups tg ON tg.Id = tt.GroupId
        ),
        RollbackProblem AS (
            SELECT r.TaskId,
                COALESCE(NULLIF(LTRIM(RTRIM(ri.Note)), ''), tb.Name, '') AS ProblemType,
                ROW_NUMBER() OVER (PARTITION BY r.TaskId ORDER BY ri.Id) AS rn
            FROM Rollbacks r
            INNER JOIN RollbackIssues ri ON ri.RollbackId = r.Id
            LEFT JOIN Steps st ON st.Id = ri.StepId
            LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
        ),
        BaseRows AS (
            SELECT
                t.Id AS TaskId,
                CAST(COALESCE(ad.ReportDate, t.CreatedAt) AS date) AS ReportDate,
                g.Name AS Team,
                CASE
                    WHEN s.Name LIKE '%[_]2a' OR s.Name LIKE '%[_]2e' OR LOWER(s.Name) LIKE '%term2%' OR LOWER(s.Name) LIKE '%term_2%' THEN 'Term 2'
                    ELSE 'Term 1'
                END AS Semester,
                CASE
                    WHEN s.Name LIKE 'ara[_]%' THEN 'Arabic'
                    WHEN s.Name LIKE 'eng[_]%' THEN 'English'
                    WHEN s.Name LIKE 'mth[_]%' THEN 'Math (A)'
                    WHEN s.Name LIKE 'sci[_]%' THEN 'Science (A)'
                    WHEN s.Name LIKE 'soc[_]%' THEN 'Social'
                    WHEN s.Name LIKE 'ict[_]%' THEN 'ICT (A)'
                    WHEN s.Name LIKE 'mul[_]%' THEN 'MUL (A)'
                    WHEN s.Name LIKE 'rel[_]%' THEN 'Religion'
                    ELSE 'Other'
                END AS Subjects,
                CASE LOWER(
                    CASE
                        WHEN CHARINDEX('_', s.Name) > 0
                             AND CHARINDEX('_', s.Name, CHARINDEX('_', s.Name) + 1) > CHARINDEX('_', s.Name)
                        THEN SUBSTRING(s.Name, CHARINDEX('_', s.Name) + 1,
                            CHARINDEX('_', s.Name, CHARINDEX('_', s.Name) + 1) - CHARINDEX('_', s.Name) - 1)
                        ELSE ''
                    END)
                    WHEN '1k' THEN 'Kg1' WHEN '2k' THEN 'Kg2'
                    WHEN '1r' THEN 'Grade 1' WHEN '2r' THEN 'Grade 2' WHEN '3r' THEN 'Grade 3'
                    WHEN '4r' THEN 'Grade 4' WHEN '5r' THEN 'Grade 5' WHEN '6r' THEN 'Grade 6'
                    WHEN '7r' THEN 'Grade 7'
                    ELSE CASE WHEN CHARINDEX('_', s.Name) > 0
                         AND CHARINDEX('_', s.Name, CHARINDEX('_', s.Name) + 1) > CHARINDEX('_', s.Name)
                        THEN SUBSTRING(s.Name, CHARINDEX('_', s.Name) + 1,
                            CHARINDEX('_', s.Name, CHARINDEX('_', s.Name) + 1) - CHARINDEX('_', s.Name) - 1)
                        ELSE '' END
                END AS Grade,
                COALESCE(NULLIF(tb.Name, ''), t.Name) AS TaskName,
                ISNULL(lo.Tag, '') AS LoCode,
                ISNULL(lo.Template, '') AS LoType,
                COALESCE(NULLIF(usr.Name, ''), NULLIF(lr.ToGroupName, ''), g.Name, '') AS AssignedTo,
                CASE
                    WHEN t.Status = @taskRollback OR t.IsRollback = 1 THEN 'Rollback'
                    WHEN t.Status = @taskDone THEN 'Approved'
                    ELSE 'Hold'
                END AS [Status],
                ISNULL(rp.ProblemType, '') AS ProblemType,
                CASE t.Priority
                    WHEN @priorityHigh THEN 'High'
                    WHEN @priorityMedium THEN 'Medium'
                    WHEN @priorityLow THEN 'Low'
                    ELSE ''
                END AS [Priority],
                COALESCE(NULLIF(notes.Notes, ''), lr.Clarification, '') AS Notes
            FROM Tasks t
            INNER JOIN Groups g ON g.Id = t.GroupId
            INNER JOIN LearningObjectives lo ON lo.Id = t.LearningObjectiveId
            INNER JOIN Lessons l ON l.Id = lo.LessonId
            INNER JOIN Units un ON un.Id = l.UnitId
            INNER JOIN Subjects s ON s.Id = un.SubjectId
            LEFT JOIN Users usr ON usr.Id = t.UserId
            LEFT JOIN Steps st ON st.Id = t.StepId
            LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
            LEFT JOIN ActivityDates ad ON ad.TaskId = t.Id
            LEFT JOIN DailyReportNoteOverrides notes ON notes.TaskId = t.Id
            LEFT JOIN LatestRollback lr ON lr.TaskId = t.Id AND lr.rn = 1
            LEFT JOIN RollbackProblem rp ON rp.TaskId = t.Id AND rp.rn = 1
            WHERE t.Archived = 0 AND lo.Archived = 0 AND l.Archived = 0
              AND un.Archived = 0 AND s.Archived = 0
              {DATE_FILTER_BASE}
              {ROLE_FILTER}
        )
        """;

    internal const string FilteredRows = """
        SELECT * FROM BaseRows
        WHERE 1 = 1
          {TEAM_FILTER}
          {SEMESTER_FILTER}
          {SUBJECT_FILTER}
          {GRADE_FILTER}
          {TASK_FILTER}
          {STATUS_FILTER}
          {PROBLEM_FILTER}
          {PRIORITY_FILTER}
        """;

    internal const string PagedRows = """
        SELECT TaskId, CONVERT(varchar(10), ReportDate, 23) AS [Date], Team, Semester, Subjects, Grade,
               TaskName, LoCode, LoType, AssignedTo, [Status], ProblemType, [Priority], Notes
        FROM Filtered
        ORDER BY ReportDate DESC, TaskId DESC
        OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY
        """;

    internal const string CountRows = """
        SELECT COUNT(1) FROM Filtered
        """;

    /// <summary>Problem-type chart — only rollback tasks, no full report CTE.</summary>
    internal const string ProblemTypesDirect = """
        SELECT COALESCE(NULLIF(LTRIM(RTRIM(ri.Note)), ''), tb.Name, '') AS ProblemType,
               COUNT(1) AS [Count]
        FROM Tasks t
        INNER JOIN Rollbacks r ON r.TaskId = t.Id
        INNER JOIN RollbackIssues ri ON ri.RollbackId = r.Id
        LEFT JOIN Steps st ON st.Id = ri.StepId
        LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
        WHERE t.Archived = 0
          AND (t.Status = @taskRollback OR t.IsRollback = 1)
          AND CAST(t.CreatedAt AS date) >= @fromDate
          AND CAST(t.CreatedAt AS date) <= @toDate
          {ROLE_FILTER}
        GROUP BY COALESCE(NULLIF(LTRIM(RTRIM(ri.Note)), ''), tb.Name, '')
        HAVING COALESCE(NULLIF(LTRIM(RTRIM(ri.Note)), ''), tb.Name, '') <> ''
        ORDER BY COUNT(1) DESC
        """;

    internal const string LightLookupsTeams = """
        SELECT DISTINCT g.Name AS Value
        FROM Tasks t
        INNER JOIN Groups g ON g.Id = t.GroupId
        WHERE t.Archived = 0
          AND CAST(t.CreatedAt AS date) >= @fromDate
          AND CAST(t.CreatedAt AS date) <= @toDate
          {ROLE_FILTER}
        ORDER BY g.Name
        """;

    internal const string LightLookupsTaskNames = """
        SELECT DISTINCT TOP 200 COALESCE(NULLIF(tb.Name, ''), t.Name) AS Value
        FROM Tasks t
        LEFT JOIN Steps st ON st.Id = t.StepId
        LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
        WHERE t.Archived = 0
          AND CAST(t.CreatedAt AS date) >= @fromDate
          AND CAST(t.CreatedAt AS date) <= @toDate
          {ROLE_FILTER}
        ORDER BY Value
        """;
}
