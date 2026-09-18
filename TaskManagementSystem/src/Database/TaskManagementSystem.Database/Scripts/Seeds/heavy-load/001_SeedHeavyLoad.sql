-- Optional dev heavy-load seed. All synthetic rows use SEED_* name prefixes (users: Code SD0001–SD0049).
-- Not part of DbUp. Run via scripts/seed-heavy-load.ps1

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM [organization].[Teams] WHERE [Name] LIKE N'SEED_%')
BEGIN
    RAISERROR(N'Heavy-load seed already present. Run seed-heavy-load.ps1 -Clear or 002_ClearHeavyLoad.sql first.', 16, 1);
    RETURN;
END
GO

DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @TypeId INT = (SELECT TOP 1 [Id] FROM [workflows].[SchemaTypes] ORDER BY [Id]);
DECLARE @IntegrationTeamId INT = (SELECT TOP 1 [Id] FROM [organization].[Teams] WHERE [Name] = N'Integration Test Team');

IF @TypeId IS NULL
BEGIN
    INSERT INTO [workflows].[SchemaTypes] ([Name], [Description])
    VALUES (N'SEED_DefaultType', N'Seeded schema type');
    SET @TypeId = SCOPE_IDENTITY();
END

-- Teams (5)
DECLARE @TeamIds TABLE ([Idx] INT NOT NULL PRIMARY KEY, [Id] INT NOT NULL);
DECLARE @t INT = 1;
WHILE @t <= 5
BEGIN
    DECLARE @TeamId INT;
    INSERT INTO [organization].[Teams] ([Name], [Archived])
    VALUES (CONCAT(N'SEED_Team_', RIGHT(CONCAT(N'0', @t), 2)), 0);
    SET @TeamId = SCOPE_IDENTITY();
    INSERT INTO @TeamIds ([Idx], [Id]) VALUES (@t, @TeamId);
    SET @t += 1;
END

-- Users (49; preserves existing TST001)
DECLARE @UserIds TABLE ([Idx] INT NOT NULL PRIMARY KEY, [Id] INT NOT NULL, [TeamId] INT NOT NULL);
DECLARE @u INT = 1;
WHILE @u <= 49
BEGIN
    DECLARE @UserTeamId INT = (SELECT [Id] FROM @TeamIds WHERE [Idx] = ((@u - 1) % 5) + 1);
    DECLARE @UserCode NVARCHAR(6) = CONCAT(N'SD', RIGHT(CONCAT(N'0000', @u), 4));
    DECLARE @NewUserId INT;

    INSERT INTO [identity].[Users]
    (
        [Name], [Code], [HR_code], [Role], [AccountType], [OnBoard], [Archived], [TeamId],
        [Annual_leave], [Annual_leave_MAX], [Emergency_leave], [Emergency_leave_MAX], [Sick_leave],
        [Permission], [Permission_MAX], [WorkFromHome], [WorkFromHome_MAX],
        [FromNextBalanceDaysUsed], [OldAnnualBalance]
    )
    VALUES
    (
        CONCAT(N'SEED_User_', RIGHT(CONCAT(N'0000', @u), 5)),
        @UserCode,
        CONCAT(N'HR', RIGHT(CONCAT(N'0000', @u), 4)),
        3, 0, 1, 0, @UserTeamId,
        5, 30, 0, 5, 0, 0, 10, 0, 5, 0, 0
    );
    SET @NewUserId = SCOPE_IDENTITY();
    INSERT INTO @UserIds ([Idx], [Id], [TeamId]) VALUES (@u, @NewUserId, @UserTeamId);
    SET @u += 1;
END

DECLARE @FirstUserId INT = (SELECT TOP 1 [Id] FROM @UserIds ORDER BY [Idx]);
DECLARE @SecondUserId INT = (SELECT TOP 1 [Id] FROM @UserIds WHERE [Idx] = 2);
DECLARE @ThirdUserId INT = (SELECT TOP 1 [Id] FROM @UserIds WHERE [Idx] = 3);

-- Sections (3) + links
DECLARE @SectionIds TABLE ([Idx] INT NOT NULL PRIMARY KEY, [Id] INT NOT NULL);
DECLARE @s INT = 1;
WHILE @s <= 3
BEGIN
    DECLARE @HeadId INT = CASE @s WHEN 1 THEN @FirstUserId WHEN 2 THEN @SecondUserId ELSE @ThirdUserId END;
    DECLARE @SectionId INT;
    INSERT INTO [organization].[Sections] ([Name], [Archived], [HeadId])
    VALUES (CONCAT(N'SEED_Section_', RIGHT(CONCAT(N'0', @s), 2)), 0, @HeadId);
    SET @SectionId = SCOPE_IDENTITY();
    INSERT INTO @SectionIds ([Idx], [Id]) VALUES (@s, @SectionId);

    INSERT INTO [organization].[SectionTeams] ([SectionId], [TeamId])
    SELECT @SectionId, [Id]
    FROM @TeamIds
    WHERE [Idx] IN ((@s - 1) * 2 + 1, (@s - 1) * 2 + 2);

    SET @s += 1;
END

-- Employee balances for seed users
INSERT INTO [hr].[EmployeeBalances]
(
    [UserId], [TeamId], [TeamleaderId], [Role],
    [AnnualLeave], [AnnualLeaveMax], [EmergencyLeave], [EmergencyLeaveMax], [SickLeave],
    [Permission], [PermissionMax], [WorkFromHome], [WorkFromHomeMax],
    [FromNextBalanceDaysUsed], [OldAnnualBalance]
)
SELECT
    u.[Id], u.[TeamId], u.[TeamleaderId], u.[Role],
    u.[Annual_leave], u.[Annual_leave_MAX], u.[Emergency_leave], u.[Emergency_leave_MAX], u.[Sick_leave],
    u.[Permission], u.[Permission_MAX], u.[WorkFromHome], u.[WorkFromHome_MAX],
    u.[FromNextBalanceDaysUsed], u.[OldAnnualBalance]
FROM [identity].[Users] AS u
WHERE u.[Code] LIKE N'SD%'
  AND NOT EXISTS (SELECT 1 FROM [hr].[EmployeeBalances] AS eb WHERE eb.[UserId] = u.[Id]);

-- Workflows: 2 schemas, 15 task bank, 10 nodes, 30 steps
DECLARE @SchemaIds TABLE ([Idx] INT NOT NULL PRIMARY KEY, [Id] INT NOT NULL);
DECLARE @SchemaIdx INT = 1;
WHILE @SchemaIdx <= 2
BEGIN
    DECLARE @SchemaId INT;
    INSERT INTO [workflows].[Schemas] ([Name], [Description], [Archived], [TypeId])
    VALUES (CONCAT(N'SEED_Schema_', RIGHT(CONCAT(N'0', @SchemaIdx), 2)), N'Seeded workflow schema', 0, @TypeId);
    SET @SchemaId = SCOPE_IDENTITY();
    INSERT INTO @SchemaIds ([Idx], [Id]) VALUES (@SchemaIdx, @SchemaId);
    SET @SchemaIdx += 1;
END

DECLARE @TaskBankIds TABLE ([Idx] INT NOT NULL PRIMARY KEY, [Id] INT NOT NULL, [TeamId] INT NOT NULL);
DECLARE @tb INT = 1;
WHILE @tb <= 15
BEGIN
    DECLARE @TbTeamId INT = (SELECT [Id] FROM @TeamIds WHERE [Idx] = ((@tb - 1) % 5) + 1);
    DECLARE @TaskBankId INT;
    INSERT INTO [workflows].[TaskBank] ([Name], [Duration], [Type], [Active], [TL], [TeamId])
    VALUES (CONCAT(N'SEED_TaskBank_', RIGHT(CONCAT(N'00', @tb), 3)), 60 + (@tb % 30), @tb % 3, 1, CASE WHEN @tb % 4 = 0 THEN 1 ELSE 0 END, @TbTeamId);
    SET @TaskBankId = SCOPE_IDENTITY();
    INSERT INTO @TaskBankIds ([Idx], [Id], [TeamId]) VALUES (@tb, @TaskBankId, @TbTeamId);
    SET @tb += 1;
END

DECLARE @NodeIds TABLE ([Idx] INT NOT NULL PRIMARY KEY, [Id] INT NOT NULL, [SchemaId] INT NOT NULL);
DECLARE @NodeIdx INT = 1;
WHILE @NodeIdx <= 10
BEGIN
    DECLARE @NodeSchemaId INT = (SELECT [Id] FROM @SchemaIds WHERE [Idx] = ((@NodeIdx - 1) / 5) + 1);
    DECLARE @NodeOrder INT = ((@NodeIdx - 1) % 5) + 1;
    DECLARE @NodeId INT;
    INSERT INTO [workflows].[Nodes] ([Name], [Order], [isStart], [isEnd], [Archived], [SchemaId])
    VALUES
    (
        CONCAT(N'SEED_Node_', RIGHT(CONCAT(N'00', @NodeIdx), 3)),
        @NodeOrder,
        CASE WHEN @NodeOrder = 1 THEN 1 ELSE 0 END,
        CASE WHEN @NodeOrder = 5 THEN 1 ELSE 0 END,
        0,
        @NodeSchemaId
    );
    SET @NodeId = SCOPE_IDENTITY();
    INSERT INTO @NodeIds ([Idx], [Id], [SchemaId]) VALUES (@NodeIdx, @NodeId, @NodeSchemaId);
    SET @NodeIdx += 1;
END

DECLARE @StepIds TABLE ([Idx] INT NOT NULL PRIMARY KEY, [Id] INT NOT NULL);
DECLARE @StepIdx INT = 1;
WHILE @StepIdx <= 30
BEGIN
    DECLARE @StepNodeId INT = (SELECT [Id] FROM @NodeIds WHERE [Idx] = ((@StepIdx - 1) % 10) + 1);
    DECLARE @StepTaskBankId INT = (SELECT [Id] FROM @TaskBankIds WHERE [Idx] = ((@StepIdx - 1) % 15) + 1);
    DECLARE @StepId INT;
    INSERT INTO [workflows].[Steps] ([Order], [Duration], [Priority], [Archived], [NodeId], [TaskBankId])
    VALUES (((@StepIdx - 1) % 3) + 1, 30 + (@StepIdx % 20), @StepIdx % 3, 0, @StepNodeId, @StepTaskBankId);
    SET @StepId = SCOPE_IDENTITY();
    INSERT INTO @StepIds ([Idx], [Id]) VALUES (@StepIdx, @StepId);
    SET @StepIdx += 1;
END

DECLARE @PrimarySchemaId INT = (SELECT TOP 1 [Id] FROM @SchemaIds ORDER BY [Idx]);
DECLARE @LoIds TABLE ([Id] INT NOT NULL PRIMARY KEY);

-- Curriculum tree: 2×2×2×2×3×3×3×2 = 432 LOs
DECLARE @y INT = 1;
WHILE @y <= 2
BEGIN
    DECLARE @YearId INT;
    INSERT INTO [curriculum].[AcademicYears] ([Name], [Description], [Archived])
    VALUES (CONCAT(N'SEED_Year_', RIGHT(CONCAT(N'0', @y), 2)), N'Seeded academic year', 0);
    SET @YearId = SCOPE_IDENTITY();

    DECLARE @p INT = 1;
    WHILE @p <= 2
    BEGIN
        DECLARE @ProjectId INT;
        INSERT INTO [curriculum].[CurriculumProjects] ([Name], [Description], [Archived], [YearId])
        VALUES (CONCAT(N'SEED_Project_Y', @y, N'_P', @p), N'Seeded project', 0, @YearId);
        SET @ProjectId = SCOPE_IDENTITY();

        DECLARE @term INT = 1;
        WHILE @term <= 2
        BEGIN
            DECLARE @TermId INT;
            INSERT INTO [curriculum].[CurriculumTerms] ([Name], [StartDate], [EndDate], [Archived], [ProjectId])
            VALUES
            (
                CONCAT(N'SEED_Term_Y', @y, N'_P', @p, N'_T', @term),
                DATEADD(MONTH, (@term - 1) * 4, @Now),
                DATEADD(MONTH, (@term - 1) * 4 + 3, @Now),
                0,
                @ProjectId
            );
            SET @TermId = SCOPE_IDENTITY();

            DECLARE @sg INT = 1;
            WHILE @sg <= 2
            BEGIN
                DECLARE @SubjectGroupId INT;
                INSERT INTO [curriculum].[SubjectGroups] ([Name], [Archived], [TermId])
                VALUES (CONCAT(N'SEED_SubjectGroup_Y', @y, N'_SG', @sg), 0, @TermId);
                SET @SubjectGroupId = SCOPE_IDENTITY();

                DECLARE @sub INT = 1;
                WHILE @sub <= 3
                BEGIN
                    DECLARE @SubjectId INT;
                    INSERT INTO [curriculum].[Subjects] ([Name], [Description], [Status], [Archived], [ArchivedWithFolder], [SubjectGroupId])
                    VALUES (CONCAT(N'SEED_Subject_', @y, N'_', @sub), N'Seeded subject', 0, 0, 0, @SubjectGroupId);
                    SET @SubjectId = SCOPE_IDENTITY();

                    DECLARE @unit INT = 1;
                    WHILE @unit <= 3
                    BEGIN
                        DECLARE @UnitId INT;
                        INSERT INTO [curriculum].[Units] ([Name], [Archived], [SubjectId])
                        VALUES (CONCAT(N'SEED_Unit_', @y, N'_', @unit), 0, @SubjectId);
                        SET @UnitId = SCOPE_IDENTITY();

                        DECLARE @lesson INT = 1;
                        WHILE @lesson <= 3
                        BEGIN
                            DECLARE @LessonId INT;
                            INSERT INTO [curriculum].[Lessons] ([Name], [Archived], [UnitId])
                            VALUES (CONCAT(N'SEED_Lesson_', @y, N'_', @lesson), 0, @UnitId);
                            SET @LessonId = SCOPE_IDENTITY();

                            DECLARE @LoId INT;
                            INSERT INTO [curriculum].[LearningObjectives]
                            (
                                [Name], [Tag], [Template], [Environment], [CreateAt], [StartedAt], [DoneAt],
                                [Archived], [LessonId], [SchemaId]
                            )
                            VALUES
                            (
                                CONCAT(N'SEED_LO_Y', @y, N'_P', @p, N'_T', @term, N'_SG', @sg, N'_S', @sub, N'_U', @unit, N'_L', @lesson),
                                CONCAT(N'SEED_TAG_', @y, N'_', @lesson),
                                N'default',
                                N'prod',
                                @Now,
                                NULL,
                                NULL,
                                0,
                                @LessonId,
                                @PrimarySchemaId
                            );
                            SET @LoId = SCOPE_IDENTITY();
                            INSERT INTO @LoIds ([Id]) VALUES (@LoId);
                            SET @lesson += 1;
                        END
                        SET @unit += 1;
                    END
                    SET @sub += 1;
                END
                SET @sg += 1;
            END
            SET @term += 1;
        END
        SET @p += 1;
    END
    SET @y += 1;
END

-- SubjectUser links (first 10 subjects × first 5 users)
INSERT INTO [curriculum].[SubjectUser] ([SubjectsId], [UsersId])
SELECT DISTINCT s.[Id], u.[Id]
FROM [curriculum].[Subjects] AS s
CROSS JOIN (SELECT TOP 5 [Id] FROM @UserIds ORDER BY [Idx]) AS u
WHERE s.[Name] LIKE N'SEED_Subject_%'
  AND s.[Id] IN (SELECT TOP 10 [Id] FROM [curriculum].[Subjects] WHERE [Name] LIKE N'SEED_%' ORDER BY [Id]);

-- Sprints (10) with ~20 LOs each
DECLARE @SprintIds TABLE ([Idx] INT NOT NULL PRIMARY KEY, [Id] INT NOT NULL);
DECLARE @sp INT = 1;
WHILE @sp <= 10
BEGIN
    DECLARE @SprintId INT;
    INSERT INTO [sprints].[Sprints] ([Name], [Description], [StartDate], [EndDate], [IsArchived])
    VALUES
    (
        CONCAT(N'SEED_Sprint_', RIGHT(CONCAT(N'0', @sp), 2)),
        N'Seeded sprint',
        DATEADD(DAY, (@sp - 1) * 14, @Now),
        DATEADD(DAY, (@sp - 1) * 14 + 13, @Now),
        0
    );
    SET @SprintId = SCOPE_IDENTITY();
    INSERT INTO @SprintIds ([Idx], [Id]) VALUES (@sp, @SprintId);

    INSERT INTO [sprints].[SprintLearningObjectives] ([SprintId], [LearningObjectiveId])
    SELECT @SprintId, lo.[Id]
    FROM (
        SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Id]) AS rn
        FROM @LoIds
    ) AS lo
    WHERE lo.rn BETWEEN ((@sp - 1) * 20) + 1 AND (@sp * 20);

    SET @sp += 1;
END

-- Tickets (~2000)
DECLARE @LoCount INT = (SELECT COUNT(*) FROM @LoIds);
DECLARE @UserCount INT = (SELECT COUNT(*) FROM @UserIds);
DECLARE @DefaultStepId INT = (SELECT TOP 1 [Id] FROM @StepIds ORDER BY [Idx]);

;WITH nums AS (
    SELECT TOP (2000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects AS a
    CROSS JOIN sys.all_objects AS b
),
lo AS (
    SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Id]) AS rn
    FROM @LoIds
),
users AS (
    SELECT [Id], [TeamId], ROW_NUMBER() OVER (ORDER BY [Idx]) AS rn
    FROM @UserIds
)
INSERT INTO [ticket].[Tasks]
(
    [Name], [Status], [Priority], [Duration], [CreatedAt], [Attention], [Flagged], [TL],
    [IsReview], [IsRollback], [RollbackCount], [Archived], [LearningObjectiveId], [StepId], [UserId], [TeamId]
)
SELECT
    CONCAT(N'SEED_Ticket_', RIGHT(CONCAT(N'00000', n.n), 5)),
    n.n % 5,
    n.n % 3,
    30 + (n.n % 120),
    DATEADD(MINUTE, -n.n, @Now),
    CASE WHEN n.n % 7 = 0 THEN 1 ELSE 0 END,
    CASE WHEN n.n % 11 = 0 THEN 1 ELSE 0 END,
    CASE WHEN n.n % 13 = 0 THEN 1 ELSE 0 END,
    0,
    0,
    0,
    0,
    lo.[Id],
    @DefaultStepId,
    u.[Id],
    u.[TeamId]
FROM nums AS n
INNER JOIN lo ON lo.rn = ((n.n - 1) % @LoCount) + 1
INNER JOIN users AS u ON u.rn = ((n.n - 1) % @UserCount) + 1;

-- Notifications (~5000)
;WITH nums AS (
    SELECT TOP (5000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects AS a
    CROSS JOIN sys.all_objects AS b
),
users AS (
    SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Idx]) AS rn
    FROM @UserIds
)
INSERT INTO [notifications].[Notifications]
(
    [Title], [Message], [Category], [Type], [Status], [IsRead], [HasActions], [CreatedAt], [UserId]
)
SELECT
    CONCAT(N'SEED_Notification_', RIGHT(CONCAT(N'00000', n.n), 5)),
    N'Seeded notification message',
    CASE n.n % 3 WHEN 0 THEN N'Task' WHEN 1 THEN N'HR' ELSE N'System' END,
    CASE n.n % 2 WHEN 0 THEN N'Info' ELSE N'Action' END,
    N'Active',
    CASE WHEN n.n % 4 = 0 THEN 1 ELSE 0 END,
    CASE WHEN n.n % 5 = 0 THEN 1 ELSE 0 END,
    DATEADD(MINUTE, -n.n, @Now),
    u.[Id]
FROM nums AS n
INNER JOIN users AS u ON u.rn = ((n.n - 1) % @UserCount) + 1;

-- HR: leave (~200), permissions (~100), WFH (~100), forgot-clock (~50)
DECLARE @LeaveTypes TABLE ([Idx] INT NOT NULL PRIMARY KEY, [Type] NVARCHAR(20) NOT NULL);
INSERT INTO @LeaveTypes ([Idx], [Type]) VALUES (1, N'Annual'), (2, N'Emergency'), (3, N'Sick'), (4, N'Unpaid');

;WITH nums AS (
    SELECT TOP (200) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM sys.all_objects
),
users AS (
    SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Idx]) AS rn FROM @UserIds
)
INSERT INTO [hr].[LeaveRequests]
(
    [Type], [Status], [StartDate], [EndDate], [Reason], [WorkingDays], [DateCreated], [UserId]
)
SELECT
    lt.[Type],
    CASE n.n % 4 WHEN 0 THEN N'Approved' WHEN 1 THEN N'Pending' WHEN 2 THEN N'Rejected' ELSE N'Cancelled' END,
    DATEADD(DAY, n.n, @Now),
    DATEADD(DAY, n.n + 2, @Now),
    N'SEED_HEAVY_LOAD',
    3,
    @Now,
    u.[Id]
FROM nums AS n
INNER JOIN users AS u ON u.rn = ((n.n - 1) % @UserCount) + 1
INNER JOIN @LeaveTypes AS lt ON lt.[Idx] = ((n.n - 1) % 4) + 1;

;WITH nums AS (
    SELECT TOP (100) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM sys.all_objects
),
users AS (
    SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Idx]) AS rn FROM @UserIds
)
INSERT INTO [hr].[Permissions]
(
    [Type], [Status], [PermissionDate], [FromTime], [ToTime], [Reason], [CreatedAt], [UserId]
)
SELECT
    N'Personal',
    CASE n.n % 3 WHEN 0 THEN N'Approved' WHEN 1 THEN N'Pending' ELSE N'Rejected' END,
    DATEADD(DAY, n.n, @Now),
    CAST(N'09:00:00' AS TIME),
    CAST(N'11:00:00' AS TIME),
    N'SEED_HEAVY_LOAD',
    @Now,
    u.[Id]
FROM nums AS n
INNER JOIN users AS u ON u.rn = ((n.n - 1) % @UserCount) + 1;

;WITH nums AS (
    SELECT TOP (100) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM sys.all_objects
),
users AS (
    SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Idx]) AS rn FROM @UserIds
)
INSERT INTO [hr].[WorkFromHomeRequests]
(
    [Date], [DateCreated], [NoteForManager], [Status], [UserId]
)
SELECT
    DATEADD(DAY, n.n, @Now),
    @Now,
    N'SEED_HEAVY_LOAD',
    n.n % 3,
    u.[Id]
FROM nums AS n
INNER JOIN users AS u ON u.rn = ((n.n - 1) % @UserCount) + 1;

;WITH nums AS (
    SELECT TOP (50) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM sys.all_objects
),
users AS (
    SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Idx]) AS rn FROM @UserIds
)
INSERT INTO [hr].[ForgotClockRequests]
(
    [PunchType], [Status], [AttendanceDate], [IntendedTime], [Reason], [CreatedAt], [UserId]
)
SELECT
    CASE n.n % 2 WHEN 0 THEN N'In' ELSE N'Out' END,
    CASE n.n % 3 WHEN 0 THEN N'Approved' WHEN 1 THEN N'Pending' ELSE N'Rejected' END,
    DATEADD(DAY, -n.n, @Now),
    CAST(N'08:30:00' AS TIME),
    N'SEED_HEAVY_LOAD',
    @Now,
    u.[Id]
FROM nums AS n
INNER JOIN users AS u ON u.rn = ((n.n - 1) % @UserCount) + 1;

-- Public holidays (5) for GET /hr/holidays/{id}
IF NOT EXISTS (SELECT 1 FROM [hr].[PublicHolidays] WHERE [Name] LIKE N'SEED_%')
BEGIN
    DECLARE @HolidayUserId INT = COALESCE(@FirstUserId, (SELECT TOP 1 [Id] FROM [identity].[Users] WHERE [Code] = N'TST001'));
    DECLARE @h INT = 1;
    WHILE @h <= 5
    BEGIN
        INSERT INTO [hr].[PublicHolidays] ([Name], [Description], [StartDate], [EndDate], [CreatedAt], [CreatedByUserId])
        VALUES
        (
            CONCAT(N'SEED_Holiday_', RIGHT(CONCAT(N'0', @h), 2)),
            N'Seeded public holiday',
            DATEADD(DAY, @h * 30, CAST(@Now AS DATE)),
            DATEADD(DAY, @h * 30, CAST(@Now AS DATE)),
            @Now,
            @HolidayUserId
        );
        SET @h += 1;
    END
END

PRINT N'Heavy-load seed completed.';
GO
