namespace AutomatedTaskSystem.Services.TaskLogger;

internal static class TaskLoggerSql
{
    /// <summary>
    /// Light CTE with work time, expected duration, and points.
    /// Placeholders: {DATE_FILTER_BASE}, {ROLE_FILTER}
    /// </summary>
    internal const string LightBaseCte = """
        WITH ActivityDates AS (
            SELECT TaskId, MAX(TimeStamp) AS ReportDate
            FROM TaskActivities
            WHERE Type IN (@statusDone, @statusRollback, @rollbackActivity)
              AND TimeStamp >= @fromDate
              AND TimeStamp < DATEADD(DAY, 1, @toDate)
            GROUP BY TaskId
        ),
        WorkTimes AS (
            SELECT twt.TaskId, SUM(twt.Duration) / 60000.0 AS ActualMinutes
            FROM TaskWorkTimes twt
            INNER JOIN Users wu ON wu.Id = twt.UserId AND wu.Archived = 0
            GROUP BY twt.TaskId
        ),
        BaseRows AS (
            SELECT
                t.Id AS TaskId,
                CAST(COALESCE(ad.ReportDate, t.CreatedAt) AS date) AS ReportDate,
                usr.Name AS Member,
                ISNULL(lo.Name, '') AS LoCode,
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
                END AS Subject,
                COALESCE(NULLIF(tb.Name, ''), t.Name) AS TaskName,
                CAST(ROUND(
                    COALESCE(NULLIF(wt.ActualMinutes, 0), NULLIF(CAST(t.Duration AS float), 0), 0)
                , 0) AS float) AS ActualMinutes,
                CAST(COALESCE(NULLIF(CAST(st.Duration AS float), 0), NULLIF(CAST(tb.Duration AS float), 0), NULLIF(CAST(t.Duration AS float), 0), 0) AS float) AS ExpectedMinutes,
                CAST(
                    CASE
                        WHEN COALESCE(NULLIF(CAST(st.Duration AS float), 0), NULLIF(CAST(tb.Duration AS float), 0), NULLIF(CAST(t.Duration AS float), 0), 0) <= 0
                            THEN 0
                        WHEN ROUND(COALESCE(NULLIF(wt.ActualMinutes, 0), NULLIF(CAST(t.Duration AS float), 0), 0), 0)
                             < COALESCE(NULLIF(CAST(st.Duration AS float), 0), NULLIF(CAST(tb.Duration AS float), 0), NULLIF(CAST(t.Duration AS float), 0), 0)
                            THEN 3
                        WHEN ROUND(COALESCE(NULLIF(wt.ActualMinutes, 0), NULLIF(CAST(t.Duration AS float), 0), 0), 0)
                             = COALESCE(NULLIF(CAST(st.Duration AS float), 0), NULLIF(CAST(tb.Duration AS float), 0), NULLIF(CAST(t.Duration AS float), 0), 0)
                            THEN 2
                        ELSE 1
                    END
                AS float) AS Points,
                CASE
                    WHEN t.Flagged = 1 THEN 'Red Flag'
                    WHEN t.Status = @taskRollback OR t.IsRollback = 1 THEN 'Rollback'
                    WHEN t.Status = @taskDone THEN 'Approved'
                    ELSE 'Hold'
                END AS [Status]
            FROM Tasks t
            INNER JOIN Groups g ON g.Id = t.GroupId
            INNER JOIN LearningObjectives lo ON lo.Id = t.LearningObjectiveId
            INNER JOIN Lessons l ON l.Id = lo.LessonId
            INNER JOIN Units un ON un.Id = l.UnitId
            INNER JOIN Subjects s ON s.Id = un.SubjectId
            INNER JOIN Users usr ON usr.Id = t.UserId AND usr.Archived = 0
            LEFT JOIN Steps st ON st.Id = t.StepId
            LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
            LEFT JOIN ActivityDates ad ON ad.TaskId = t.Id
            LEFT JOIN WorkTimes wt ON wt.TaskId = t.Id
            WHERE t.Archived = 0 AND lo.Archived = 0 AND l.Archived = 0
              AND un.Archived = 0 AND s.Archived = 0
              {DATE_FILTER_BASE}
              {ROLE_FILTER}
        )
        """;

    /// <summary>Unbounded ActivityDates for rankings when range = all.</summary>
    internal const string RankingsBaseCte = """
        WITH ActivityDates AS (
            SELECT TaskId, MAX(TimeStamp) AS ReportDate
            FROM TaskActivities
            WHERE Type IN (@statusDone, @statusRollback, @rollbackActivity)
              {RANKING_ACTIVITY_DATE}
            GROUP BY TaskId
        ),
        WorkTimes AS (
            SELECT twt.TaskId, SUM(twt.Duration) / 60000.0 AS ActualMinutes
            FROM TaskWorkTimes twt
            INNER JOIN Users wu ON wu.Id = twt.UserId AND wu.Archived = 0
            GROUP BY twt.TaskId
        ),
        BaseRows AS (
            SELECT
                usr.Name AS Member,
                CAST(
                    CASE
                        WHEN COALESCE(NULLIF(CAST(st.Duration AS float), 0), NULLIF(CAST(tb.Duration AS float), 0), NULLIF(CAST(t.Duration AS float), 0), 0) <= 0
                            THEN 0
                        WHEN ROUND(COALESCE(NULLIF(wt.ActualMinutes, 0), NULLIF(CAST(t.Duration AS float), 0), 0), 0)
                             < COALESCE(NULLIF(CAST(st.Duration AS float), 0), NULLIF(CAST(tb.Duration AS float), 0), NULLIF(CAST(t.Duration AS float), 0), 0)
                            THEN 3
                        WHEN ROUND(COALESCE(NULLIF(wt.ActualMinutes, 0), NULLIF(CAST(t.Duration AS float), 0), 0), 0)
                             = COALESCE(NULLIF(CAST(st.Duration AS float), 0), NULLIF(CAST(tb.Duration AS float), 0), NULLIF(CAST(t.Duration AS float), 0), 0)
                            THEN 2
                        ELSE 1
                    END
                AS float) AS Points
            FROM Tasks t
            INNER JOIN Groups g ON g.Id = t.GroupId
            INNER JOIN LearningObjectives lo ON lo.Id = t.LearningObjectiveId
            INNER JOIN Lessons l ON l.Id = lo.LessonId
            INNER JOIN Units un ON un.Id = l.UnitId
            INNER JOIN Subjects s ON s.Id = un.SubjectId
            INNER JOIN Users usr ON usr.Id = t.UserId AND usr.Archived = 0
            LEFT JOIN Steps st ON st.Id = t.StepId
            LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
            LEFT JOIN ActivityDates ad ON ad.TaskId = t.Id
            LEFT JOIN WorkTimes wt ON wt.TaskId = t.Id
            WHERE t.Archived = 0 AND lo.Archived = 0 AND l.Archived = 0
              AND un.Archived = 0 AND s.Archived = 0
              {DATE_FILTER_BASE}
              {ROLE_FILTER}
        )
        """;

    internal const string FilteredRows = """
        SELECT * FROM BaseRows
        WHERE 1 = 1
          {SEARCH_FILTER}
          {MEMBER_FILTER}
          {SUBJECT_FILTER}
          {STATUS_FILTER}
          {TASK_FILTER}
        """;

    internal const string CountAndPagedTaskIds = """
        SELECT * INTO #TaskLoggerFiltered FROM Filtered;
        SELECT COUNT(1) FROM #TaskLoggerFiltered;
        SELECT TaskId
        FROM #TaskLoggerFiltered
        ORDER BY ReportDate DESC, TaskId DESC
        OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY
        """;

    internal const string SummarySql = """
        SELECT * INTO #TaskLoggerSummary FROM Filtered;
        SELECT
            COUNT(1) AS TotalTasks,
            ISNULL(SUM(ActualMinutes), 0) AS TotalTimeMinutes,
            SUM(CASE WHEN [Status] = 'Rollback' THEN 1 ELSE 0 END) AS RollbackTasks,
            ISNULL(SUM(Points), 0) AS TotalPoints
        FROM #TaskLoggerSummary
        """;

    internal const string RankingsSql = """
        SELECT TOP 5 Member, SUM(Points) AS Points
        FROM Filtered
        WHERE Member IS NOT NULL AND Member <> ''
        GROUP BY Member
        ORDER BY SUM(Points) DESC, Member
        """;

    internal const string EnrichRowsSql = """
        WITH LatestRollback AS (
            SELECT r.TaskId, r.Clarification,
                ROW_NUMBER() OVER (PARTITION BY r.TaskId ORDER BY r.Id DESC) AS rn
            FROM Rollbacks r
            WHERE r.TaskId IN @taskIds
        ),
        ActivityDates AS (
            SELECT TaskId, MAX(TimeStamp) AS ReportDate
            FROM TaskActivities
            WHERE TaskId IN @taskIds
              AND Type IN (@statusDone, @statusRollback, @rollbackActivity)
            GROUP BY TaskId
        ),
        WorkTimes AS (
            SELECT twt.TaskId, SUM(twt.Duration) / 60000.0 AS ActualMinutes
            FROM TaskWorkTimes twt
            INNER JOIN Users wu ON wu.Id = twt.UserId AND wu.Archived = 0
            WHERE twt.TaskId IN @taskIds
            GROUP BY twt.TaskId
        )
        SELECT
            t.Id AS TaskId,
            CONVERT(varchar(10), CAST(COALESCE(ad.ReportDate, t.CreatedAt) AS date), 23) AS [Date],
            usr.Name AS Member,
            ISNULL(lo.Name, '') AS LoCode,
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
            END AS Subject,
            COALESCE(NULLIF(tb.Name, ''), t.Name) AS TaskName,
            CAST(ROUND(
                COALESCE(NULLIF(wt.ActualMinutes, 0), NULLIF(CAST(t.Duration AS float), 0), 0)
            , 0) AS float) AS ActualMinutes,
            CAST(COALESCE(NULLIF(CAST(st.Duration AS float), 0), NULLIF(CAST(tb.Duration AS float), 0), NULLIF(CAST(t.Duration AS float), 0), 0) AS float) AS ExpectedMinutes,
            CAST(
                CASE
                    WHEN COALESCE(NULLIF(CAST(st.Duration AS float), 0), NULLIF(CAST(tb.Duration AS float), 0), NULLIF(CAST(t.Duration AS float), 0), 0) <= 0
                        THEN 0
                    WHEN ROUND(COALESCE(NULLIF(wt.ActualMinutes, 0), NULLIF(CAST(t.Duration AS float), 0), 0), 0)
                         < COALESCE(NULLIF(CAST(st.Duration AS float), 0), NULLIF(CAST(tb.Duration AS float), 0), NULLIF(CAST(t.Duration AS float), 0), 0)
                        THEN 3
                    WHEN ROUND(COALESCE(NULLIF(wt.ActualMinutes, 0), NULLIF(CAST(t.Duration AS float), 0), 0), 0)
                         = COALESCE(NULLIF(CAST(st.Duration AS float), 0), NULLIF(CAST(tb.Duration AS float), 0), NULLIF(CAST(t.Duration AS float), 0), 0)
                        THEN 2
                    ELSE 1
                END
            AS float) AS Points,
            CASE
                WHEN t.Flagged = 1 THEN 'Red Flag'
                WHEN t.Status = @taskRollback OR t.IsRollback = 1 THEN 'Rollback'
                WHEN t.Status = @taskDone THEN 'Approved'
                ELSE 'Hold'
            END AS [Status],
            ISNULL(lr.Clarification, '') AS Notes
        FROM Tasks t
        INNER JOIN Groups g ON g.Id = t.GroupId
        INNER JOIN LearningObjectives lo ON lo.Id = t.LearningObjectiveId
        INNER JOIN Lessons l ON l.Id = lo.LessonId
        INNER JOIN Units un ON un.Id = l.UnitId
        INNER JOIN Subjects s ON s.Id = un.SubjectId
        INNER JOIN Users usr ON usr.Id = t.UserId AND usr.Archived = 0
        LEFT JOIN Steps st ON st.Id = t.StepId
        LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
        LEFT JOIN ActivityDates ad ON ad.TaskId = t.Id
        LEFT JOIN WorkTimes wt ON wt.TaskId = t.Id
        LEFT JOIN LatestRollback lr ON lr.TaskId = t.Id AND lr.rn = 1
        WHERE t.Id IN @taskIds
        """;

    internal const string LightLookupsMembers = """
        SELECT usr.Name AS Value
        FROM Users usr
        WHERE usr.Archived = 0
          AND usr.Name IS NOT NULL
          AND LTRIM(RTRIM(usr.Name)) <> ''
        ORDER BY usr.Name
        """;

    internal const string LightLookupsTaskNames = """
        SELECT DISTINCT TOP 200 COALESCE(NULLIF(tb.Name, ''), t.Name) AS Value
        FROM Tasks t
        LEFT JOIN Steps st ON st.Id = t.StepId
        LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
        WHERE t.Archived = 0
          AND t.CreatedAt >= @fromDate
          AND t.CreatedAt < DATEADD(DAY, 1, @toDate)
          {ROLE_FILTER}
        ORDER BY Value
        """;
}
