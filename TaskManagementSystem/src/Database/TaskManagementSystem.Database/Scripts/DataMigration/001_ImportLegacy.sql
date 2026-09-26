/*
    Import legacy dbo data from the source database into TaskManagementSystem.

    - Preserves legacy IDs (IDENTITY_INSERT).
    - Maps Groups -> organization.Teams, Tasks -> ticket.Tickets, TaskBank -> workflows.TicketBank.
    - Moves leave balances from Users into hr.EmployeeBalances.
    - Subject Status is copied 1:1 (0=Active, 1=Closed, 2=Hold, 3=Reopened).
    - After import, only Active subjects appear in Kanban; Hold/Closed/Reopened are managed under Curriculum.
    - Skips: RefreshTokens, SSRS report tables, Years, legacy Teams, Groups.ColorCode.
    - Keeps: identity RBAC catalog, hr.PublicHolidays, app.MigrationsJournal.

    Run on local SQL Server with both databases present:
      sqlcmd -S . -E -C -I -v SourceDb=SystemAdminDB_Test_v2 -i 001_ImportLegacy.sql

    Override the source database name with -v SourceDb=YourLegacyDb

    The -I flag is required (SET QUOTED_IDENTIFIER ON) for filtered indexes on ticket.Comments.
*/
:setvar SourceDb "SystemAdminDB_Test_v2"
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;

USE [TaskManagementSystem];
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF DB_ID(N'$(SourceDb)') IS NULL
BEGIN
    RAISERROR(N'Source database [$(SourceDb)] was not found.', 16, 1);
    RETURN;
END;

IF OBJECT_ID(N'$(SourceDb).dbo.Tasks', N'U') IS NULL
   OR OBJECT_ID(N'$(SourceDb).dbo.Groups', N'U') IS NULL
   OR OBJECT_ID(N'$(SourceDb).dbo.Users', N'U') IS NULL
BEGIN
    RAISERROR(N'Source database is missing expected legacy dbo tables.', 16, 1);
    RETURN;
END;
GO

PRINT N'=== Legacy import: disable foreign keys ===';

DECLARE @disableSql NVARCHAR(MAX) = N'';

SELECT @disableSql += N'ALTER TABLE '
    + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
    + N' NOCHECK CONSTRAINT ALL;' + CHAR(10)
FROM sys.tables AS t
INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
WHERE s.name IN (N'organization', N'identity', N'workflows', N'curriculum', N'ticket', N'sprints', N'notifications', N'hr')
  AND t.name NOT IN (N'Roles', N'Permissions', N'RolePermissions', N'PublicHolidays');

EXEC sys.sp_executesql @disableSql;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

SET IDENTITY_INSERT [organization].[Teams] OFF;
SET IDENTITY_INSERT [identity].[Users] OFF;
SET IDENTITY_INSERT [organization].[Sections] OFF;
SET IDENTITY_INSERT [organization].[SectionTeams] OFF;
SET IDENTITY_INSERT [workflows].[SchemaTypes] OFF;
SET IDENTITY_INSERT [workflows].[Schemas] OFF;
SET IDENTITY_INSERT [workflows].[TicketBank] OFF;
SET IDENTITY_INSERT [workflows].[Nodes] OFF;
SET IDENTITY_INSERT [workflows].[Steps] OFF;
SET IDENTITY_INSERT [curriculum].[AcademicYears] OFF;
SET IDENTITY_INSERT [curriculum].[CurriculumProjects] OFF;
SET IDENTITY_INSERT [curriculum].[CurriculumTerms] OFF;
SET IDENTITY_INSERT [curriculum].[SubjectGroups] OFF;
SET IDENTITY_INSERT [curriculum].[Subjects] OFF;
SET IDENTITY_INSERT [curriculum].[Units] OFF;
SET IDENTITY_INSERT [curriculum].[Lessons] OFF;
SET IDENTITY_INSERT [curriculum].[LearningObjectives] OFF;
SET IDENTITY_INSERT [sprints].[Sprints] OFF;
SET IDENTITY_INSERT [sprints].[SprintLearningObjectives] OFF;
SET IDENTITY_INSERT [ticket].[Tickets] OFF;
SET IDENTITY_INSERT [ticket].[TicketActivities] OFF;
SET IDENTITY_INSERT [ticket].[TicketWorkTimes] OFF;
SET IDENTITY_INSERT [ticket].[Comments] OFF;
SET IDENTITY_INSERT [ticket].[Rollbacks] OFF;
SET IDENTITY_INSERT [ticket].[RollbackIssues] OFF;
SET IDENTITY_INSERT [notifications].[Notifications] OFF;
SET IDENTITY_INSERT [hr].[LeaveResetLogs] OFF;
SET IDENTITY_INSERT [hr].[LeaveRequests] OFF;
SET IDENTITY_INSERT [hr].[Permissions] OFF;
SET IDENTITY_INSERT [hr].[WorkFromHomeRequests] OFF;
SET IDENTITY_INSERT [hr].[Opinions] OFF;
SET IDENTITY_INSERT [identity].[UserChanges] OFF;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

PRINT N'=== Legacy import: clear target business data ===';

DELETE FROM [ticket].[TicketActivities];
DELETE FROM [ticket].[TicketWorkTimes];
DELETE FROM [ticket].[Comments];
DELETE FROM [ticket].[RollbackIssues];
DELETE FROM [ticket].[Rollbacks];
DELETE FROM [ticket].[Tickets];
DELETE FROM [ticket].[InboxMessages];
DELETE FROM [ticket].[OutboxMessages];

DELETE FROM [workflows].[RSteps];
DELETE FROM [workflows].[NodeSequences];
DELETE FROM [workflows].[Steps];
DELETE FROM [workflows].[Nodes];
DELETE FROM [workflows].[TicketBank];
DELETE FROM [workflows].[Schemas];
DELETE FROM [workflows].[SchemaTypes];
DELETE FROM [workflows].[InboxMessages];
DELETE FROM [workflows].[OutboxMessages];

DELETE FROM [sprints].[SprintLearningObjectives];
DELETE FROM [sprints].[Sprints];
DELETE FROM [sprints].[InboxMessages];
DELETE FROM [sprints].[OutboxMessages];

DELETE FROM [curriculum].[SubjectUser];
DELETE FROM [curriculum].[LearningObjectives];
DELETE FROM [curriculum].[Lessons];
DELETE FROM [curriculum].[Units];
DELETE FROM [curriculum].[Subjects];
DELETE FROM [curriculum].[SubjectGroups];
DELETE FROM [curriculum].[CurriculumTerms];
DELETE FROM [curriculum].[CurriculumProjects];
DELETE FROM [curriculum].[AcademicYears];
DELETE FROM [curriculum].[InboxMessages];
DELETE FROM [curriculum].[OutboxMessages];

DELETE FROM [notifications].[Notifications];
DELETE FROM [notifications].[InboxMessages];
DELETE FROM [notifications].[OutboxMessages];

DELETE FROM [hr].[Opinions];
DELETE FROM [hr].[Permissions];
DELETE FROM [hr].[WorkFromHomeRequests];
DELETE FROM [hr].[LeaveRequests];
DELETE FROM [hr].[LeaveResetLogs];
DELETE FROM [hr].[ForgotClockRequests];
DELETE FROM [hr].[EmployeeBalances];
DELETE FROM [hr].[InboxMessages];
DELETE FROM [hr].[OutboxMessages];

DELETE FROM [identity].[UserChanges];
DELETE FROM [identity].[RefreshTokens];
DELETE FROM [identity].[Users];
DELETE FROM [identity].[InboxMessages];
DELETE FROM [identity].[OutboxMessages];

DELETE FROM [organization].[SectionTeams];
DELETE FROM [organization].[Sections];
DELETE FROM [organization].[Teams];
DELETE FROM [organization].[InboxMessages];
DELETE FROM [organization].[OutboxMessages];
GO

PRINT N'=== Legacy import: organization.Teams (from Groups) ===';

SET IDENTITY_INSERT [organization].[Teams] ON;

INSERT INTO [organization].[Teams] WITH (TABLOCK) ([Id], [Name], [Archived], [TeamleaderId])
SELECT [Id], [Name], [Archived], NULL
FROM [$(SourceDb)].[dbo].[Groups];

SET IDENTITY_INSERT [organization].[Teams] OFF;
GO

PRINT N'=== Legacy import: identity.Users ===';

SET IDENTITY_INSERT [identity].[Users] ON;

INSERT INTO [identity].[Users] WITH (TABLOCK)
(
    [Id], [Name], [Code], [HR_code], [Email], [Phone], [Title],
    [Role], [AccountType], [OnBoard], [Archived], [TeamId]
)
SELECT
    [Id], [Name], [Code], [HR_code], [Email], [Phone], [Title],
    [Role], [AccountType], [OnBoard], [Archived], [GroupId]
FROM [$(SourceDb)].[dbo].[Users];

SET IDENTITY_INSERT [identity].[Users] OFF;
GO

PRINT N'=== Legacy import: team leaders ===';

UPDATE t
SET t.[TeamleaderId] = leaders.[TeamleaderId]
FROM [organization].[Teams] AS t
INNER JOIN (
    SELECT
        u.[GroupId] AS [TeamId],
        (
            SELECT TOP (1) u2.[TeamleaderId]
            FROM [$(SourceDb)].[dbo].[Users] AS u2
            WHERE u2.[GroupId] = u.[GroupId]
              AND u2.[TeamleaderId] IS NOT NULL
            GROUP BY u2.[TeamleaderId]
            ORDER BY COUNT(*) DESC, u2.[TeamleaderId]
        ) AS [TeamleaderId]
    FROM [$(SourceDb)].[dbo].[Users] AS u
    WHERE u.[GroupId] IS NOT NULL
    GROUP BY u.[GroupId]
) AS leaders ON leaders.[TeamId] = t.[Id];
GO

PRINT N'=== Legacy import: hr.EmployeeBalances ===';

INSERT INTO [hr].[EmployeeBalances] WITH (TABLOCK)
(
    [UserId], [TeamId], [TeamleaderId], [Role],
    [AnnualLeave], [AnnualLeaveMax],
    [EmergencyLeave], [EmergencyLeaveMax],
    [SickLeave], [Permission], [PermissionMax],
    [WorkFromHome], [WorkFromHomeMax],
    [FromNextBalanceDaysUsed], [OldAnnualBalance]
)
SELECT
    u.[Id],
    u.[GroupId],
    u.[TeamleaderId],
    u.[Role],
    u.[Annual_leave],
    u.[Annual_leave_MAX],
    u.[Emergency_leave],
    u.[Emergency_leave_MAX],
    u.[Sick_leave],
    u.[Permission],
    u.[Permission_MAX],
    u.[WorkFromHome],
    u.[WorkFromHome_MAX],
    u.[FromNextBalanceDaysUsed],
    u.[OldAnnualBalance]
FROM [$(SourceDb)].[dbo].[Users] AS u;
GO

PRINT N'=== Legacy import: organization structure ===';

SET IDENTITY_INSERT [organization].[Sections] ON;

INSERT INTO [organization].[Sections] WITH (TABLOCK) ([Id], [Name], [Archived], [HeadId])
SELECT [Id], [Name], [Archived], [HeadId]
FROM [$(SourceDb)].[dbo].[Sections];

SET IDENTITY_INSERT [organization].[Sections] OFF;

SET IDENTITY_INSERT [organization].[SectionTeams] ON;

INSERT INTO [organization].[SectionTeams] WITH (TABLOCK) ([Id], [SectionId], [TeamId])
SELECT [Id], [SectionId], [GroupId]
FROM [$(SourceDb)].[dbo].[SectionGroups];

SET IDENTITY_INSERT [organization].[SectionTeams] OFF;
GO

PRINT N'=== Legacy import: workflows ===';

SET IDENTITY_INSERT [workflows].[SchemaTypes] ON;

INSERT INTO [workflows].[SchemaTypes] WITH (TABLOCK) ([Id], [Name], [Description])
SELECT [Id], [Name], ISNULL([Description], N'')
FROM [$(SourceDb)].[dbo].[SchemaTypes];

SET IDENTITY_INSERT [workflows].[SchemaTypes] OFF;

SET IDENTITY_INSERT [workflows].[Schemas] ON;

INSERT INTO [workflows].[Schemas] WITH (TABLOCK) ([Id], [Name], [Description], [Archived], [TypeId])
SELECT [Id], [Name], ISNULL([Description], N''), [Archived], [TypeId]
FROM [$(SourceDb)].[dbo].[Schemas];

SET IDENTITY_INSERT [workflows].[Schemas] OFF;

SET IDENTITY_INSERT [workflows].[TicketBank] ON;

INSERT INTO [workflows].[TicketBank] WITH (TABLOCK) ([Id], [Name], [Duration], [Type], [Active], [TL], [TeamId])
SELECT [Id], [Name], [Duration], [Type], [Active], [TL], [GroupId]
FROM [$(SourceDb)].[dbo].[TaskBank];

SET IDENTITY_INSERT [workflows].[TicketBank] OFF;

SET IDENTITY_INSERT [workflows].[Nodes] ON;

INSERT INTO [workflows].[Nodes] WITH (TABLOCK) ([Id], [Name], [Order], [isStart], [isEnd], [Archived], [SchemaId])
SELECT [Id], [Name], [Order], [isStart], [isEnd], [Archived], [SchemaId]
FROM [$(SourceDb)].[dbo].[Nodes];

SET IDENTITY_INSERT [workflows].[Nodes] OFF;

INSERT INTO [workflows].[NodeSequences] WITH (TABLOCK) ([NextId], [PreviousId])
SELECT [NextId], [PreviousId]
FROM [$(SourceDb)].[dbo].[NodeSequences];

SET IDENTITY_INSERT [workflows].[Steps] ON;

INSERT INTO [workflows].[Steps] WITH (TABLOCK) ([Id], [Order], [Duration], [Priority], [Archived], [NodeId], [TicketBankId])
SELECT [Id], [Order], [Duration], [Priority], [Archived], [NodeId], [TaskBankId]
FROM [$(SourceDb)].[dbo].[Steps];

SET IDENTITY_INSERT [workflows].[Steps] OFF;

INSERT INTO [workflows].[RSteps] WITH (TABLOCK) ([FromId], [RollbacksId])
SELECT [FromId], [RollbacksId]
FROM [$(SourceDb)].[dbo].[RSteps];
GO

PRINT N'=== Legacy import: curriculum ===';

SET IDENTITY_INSERT [curriculum].[AcademicYears] ON;

INSERT INTO [curriculum].[AcademicYears] WITH (TABLOCK) ([Id], [Name], [Description], [Archived])
SELECT [Id], [Name], [Description], [Archived]
FROM [$(SourceDb)].[dbo].[AcademicYears];

SET IDENTITY_INSERT [curriculum].[AcademicYears] OFF;

SET IDENTITY_INSERT [curriculum].[CurriculumProjects] ON;

INSERT INTO [curriculum].[CurriculumProjects] WITH (TABLOCK) ([Id], [Name], [Description], [Archived], [YearId])
SELECT [Id], [Name], [Description], [Archived], [YearId]
FROM [$(SourceDb)].[dbo].[CurriculumProjects];

SET IDENTITY_INSERT [curriculum].[CurriculumProjects] OFF;

SET IDENTITY_INSERT [curriculum].[CurriculumTerms] ON;

INSERT INTO [curriculum].[CurriculumTerms] WITH (TABLOCK) ([Id], [Name], [StartDate], [EndDate], [Archived], [ProjectId])
SELECT [Id], [Name], [StartDate], [EndDate], [Archived], [ProjectId]
FROM [$(SourceDb)].[dbo].[CurriculumTerms];

SET IDENTITY_INSERT [curriculum].[CurriculumTerms] OFF;

SET IDENTITY_INSERT [curriculum].[SubjectGroups] ON;

INSERT INTO [curriculum].[SubjectGroups] WITH (TABLOCK) ([Id], [Name], [Archived], [TermId])
SELECT [Id], [Name], [Archived], [TermId]
FROM [$(SourceDb)].[dbo].[SubjectGroups];

SET IDENTITY_INSERT [curriculum].[SubjectGroups] OFF;

SET IDENTITY_INSERT [curriculum].[Subjects] ON;

INSERT INTO [curriculum].[Subjects] WITH (TABLOCK)
(
    [Id], [Name], [Description], [Status], [Archived], [ArchivedWithFolder], [SubjectGroupId]
)
SELECT [Id], [Name], [Description], [Status], [Archived], [ArchivedWithFolder], [SubjectGroupId]
FROM [$(SourceDb)].[dbo].[Subjects];

SET IDENTITY_INSERT [curriculum].[Subjects] OFF;

SET IDENTITY_INSERT [curriculum].[Units] ON;

INSERT INTO [curriculum].[Units] WITH (TABLOCK) ([Id], [Name], [Archived], [SubjectId])
SELECT [Id], [Name], [Archived], [SubjectId]
FROM [$(SourceDb)].[dbo].[Units];

SET IDENTITY_INSERT [curriculum].[Units] OFF;

SET IDENTITY_INSERT [curriculum].[Lessons] ON;

INSERT INTO [curriculum].[Lessons] WITH (TABLOCK) ([Id], [Name], [Archived], [UnitId])
SELECT [Id], [Name], [Archived], [UnitId]
FROM [$(SourceDb)].[dbo].[Lessons];

SET IDENTITY_INSERT [curriculum].[Lessons] OFF;

SET IDENTITY_INSERT [curriculum].[LearningObjectives] ON;

INSERT INTO [curriculum].[LearningObjectives] WITH (TABLOCK)
(
    [Id], [Name], [Tag], [Template], [Environment],
    [CreateAt], [StartedAt], [DoneAt], [Archived], [LessonId], [SchemaId]
)
SELECT
    [Id], [Name],
    ISNULL([Tag], N''), ISNULL([Template], N''), ISNULL([Environment], N''),
    [CreateAt], [StartedAt], [DoneAt], [Archived], [LessonId], [SchemaId]
FROM [$(SourceDb)].[dbo].[LearningObjectives];

SET IDENTITY_INSERT [curriculum].[LearningObjectives] OFF;

INSERT INTO [curriculum].[SubjectUser] WITH (TABLOCK) ([SubjectsId], [UsersId])
SELECT [SubjectsId], [UsersId]
FROM [$(SourceDb)].[dbo].[SubjectUser];
GO

PRINT N'=== Legacy import: sprints ===';

SET IDENTITY_INSERT [sprints].[Sprints] ON;

INSERT INTO [sprints].[Sprints] WITH (TABLOCK)
(
    [Id], [Name], [Description], [StartDate], [EndDate], [IsArchived]
)
SELECT
    [Id], [Name], ISNULL([Description], N''), [StartDate], [EndDate], [IsArchived]
FROM [$(SourceDb)].[dbo].[Sprints];

SET IDENTITY_INSERT [sprints].[Sprints] OFF;

SET IDENTITY_INSERT [sprints].[SprintLearningObjectives] ON;

INSERT INTO [sprints].[SprintLearningObjectives] WITH (TABLOCK) ([Id], [SprintId], [LearningObjectiveId])
SELECT [Id], [SprintId], [LearningObjectiveId]
FROM [$(SourceDb)].[dbo].[SprintLearningObjectives];

SET IDENTITY_INSERT [sprints].[SprintLearningObjectives] OFF;
GO

PRINT N'=== Legacy import: ticket.Tickets ===';

SET IDENTITY_INSERT [ticket].[Tickets] ON;

INSERT INTO [ticket].[Tickets] WITH (TABLOCK)
(
    [Id], [Name], [Status], [Priority], [Duration], [CreatedAt],
    [Pause], [Attention], [Flagged], [TL], [IsReview], [IsRollback],
    [RollbackCount], [Archived], [LearningObjectiveId], [StepId], [UserId], [TeamId], [FromId]
)
SELECT
    [Id], [Name], [Status], [Priority], [Duration], [CreatedAt],
    [Pause], [Attention], [Flagged], [TL], [IsReview], [IsRollback],
    [RollbackCount], [Archived], [LearningObjectiveId], [StepId], [UserId], [GroupId], [FromId]
FROM [$(SourceDb)].[dbo].[Tasks];

SET IDENTITY_INSERT [ticket].[Tickets] OFF;
GO

PRINT N'=== Legacy import: ticket.TicketActivities (batched) ===';

DECLARE @ActivityBatchSize INT = 100000;
DECLARE @ActivityMinId INT;
DECLARE @ActivityMaxId INT;

SELECT
    @ActivityMinId = MIN([Id]),
    @ActivityMaxId = MAX([Id])
FROM [$(SourceDb)].[dbo].[TaskActivities];

SET IDENTITY_INSERT [ticket].[TicketActivities] ON;

WHILE @ActivityMinId IS NOT NULL AND @ActivityMinId <= @ActivityMaxId
BEGIN
    INSERT INTO [ticket].[TicketActivities] WITH (TABLOCK)
    (
        [Id], [Type], [TimeStamp], [AdditionalInfo],
        [TicketId], [TicketSecondaryId], [ActorOneId], [ActorTwoId]
    )
    SELECT
        [Id], [Type], [TimeStamp], [AdditionalInfo],
        [TaskId], [TaskSecondaryId], [ActorOneId], [ActorTwoId]
    FROM [$(SourceDb)].[dbo].[TaskActivities]
    WHERE [Id] >= @ActivityMinId
      AND [Id] < @ActivityMinId + @ActivityBatchSize;

    PRINT CONCAT(N'  TicketActivities batch through Id ', @ActivityMinId + @ActivityBatchSize - 1);
    SET @ActivityMinId += @ActivityBatchSize;
END;

SET IDENTITY_INSERT [ticket].[TicketActivities] OFF;
GO

PRINT N'=== Legacy import: ticket.TicketWorkTimes (batched) ===';

DECLARE @WorkTimeBatchSize INT = 100000;
DECLARE @WorkTimeMinId INT;
DECLARE @WorkTimeMaxId INT;

SELECT
    @WorkTimeMinId = MIN([Id]),
    @WorkTimeMaxId = MAX([Id])
FROM [$(SourceDb)].[dbo].[TaskWorkTimes];

SET IDENTITY_INSERT [ticket].[TicketWorkTimes] ON;

WHILE @WorkTimeMinId IS NOT NULL AND @WorkTimeMinId <= @WorkTimeMaxId
BEGIN
    INSERT INTO [ticket].[TicketWorkTimes] WITH (TABLOCK)
    (
        [Id], [StartDate], [EndDate], [Duration], [EndReason], [TicketId], [UserId]
    )
    SELECT
        [Id], [StartDate], [EndDate], [Duration], [EndReason], [TaskId], [UserId]
    FROM [$(SourceDb)].[dbo].[TaskWorkTimes]
    WHERE [Id] >= @WorkTimeMinId
      AND [Id] < @WorkTimeMinId + @WorkTimeBatchSize;

    PRINT CONCAT(N'  TicketWorkTimes batch through Id ', @WorkTimeMinId + @WorkTimeBatchSize - 1);
    SET @WorkTimeMinId += @WorkTimeBatchSize;
END;

SET IDENTITY_INSERT [ticket].[TicketWorkTimes] OFF;
GO

PRINT N'=== Legacy import: ticket comments and rollbacks ===';

SET IDENTITY_INSERT [ticket].[Comments] ON;

INSERT INTO [ticket].[Comments] WITH (TABLOCK)
(
    [Id], [Content], [CreatedAt], [Timestamp], [Archived],
    [LearningObjectiveId], [TicketId], [UserId], [ChildId]
)
SELECT
    [Id], [Content], [CreatedAt], [Timestamp], [Archived],
    [LearningObjectiveId], [TaskId], [UserId], [ChildId]
FROM [$(SourceDb)].[dbo].[Comments];

SET IDENTITY_INSERT [ticket].[Comments] OFF;

SET IDENTITY_INSERT [ticket].[Rollbacks] ON;

INSERT INTO [ticket].[Rollbacks] WITH (TABLOCK)
(
    [Id], [Clarification], [TicketId], [ToTicketId], [UserId]
)
SELECT
    [Id], [Clarification], [TaskId], [ToTaskId], [UserId]
FROM [$(SourceDb)].[dbo].[Rollbacks];

SET IDENTITY_INSERT [ticket].[Rollbacks] OFF;

SET IDENTITY_INSERT [ticket].[RollbackIssues] ON;

INSERT INTO [ticket].[RollbackIssues] WITH (TABLOCK) ([Id], [Note], [RollbackId], [StepId])
SELECT [Id], [Note], [RollbackId], [StepId]
FROM [$(SourceDb)].[dbo].[RollbackIssues];

SET IDENTITY_INSERT [ticket].[RollbackIssues] OFF;
GO

PRINT N'=== Legacy import: notifications and HR ===';

SET IDENTITY_INSERT [notifications].[Notifications] ON;

INSERT INTO [notifications].[Notifications] WITH (TABLOCK)
(
    [Id], [Title], [Message], [Category], [Type], [Status], [IsRead],
    [HasActions], [AdditionalData], [RelatedEntityId], [CreatedAt], [UserId]
)
SELECT
    [Id], [Title], [Message], [Category], [Type], [Status], [IsRead],
    [HasActions], [AdditionalData], [RelatedEntityId], [CreatedAt], [UserId]
FROM [$(SourceDb)].[dbo].[Notifications];

SET IDENTITY_INSERT [notifications].[Notifications] OFF;

SET IDENTITY_INSERT [hr].[LeaveResetLogs] ON;

INSERT INTO [hr].[LeaveResetLogs] WITH (TABLOCK) ([Id], [Year], [ExecutedAt])
SELECT [Id], [Year], [ExecutedAt]
FROM [$(SourceDb)].[dbo].[LeaveResetLogs];

SET IDENTITY_INSERT [hr].[LeaveResetLogs] OFF;

SET IDENTITY_INSERT [hr].[LeaveRequests] ON;

INSERT INTO [hr].[LeaveRequests] WITH (TABLOCK)
(
    [Id], [Type], [Status], [StartDate], [EndDate], [Reason], [NoteForManager],
    [MedicalCertificateFileName], [MedicalCertificatePath], [DateCreated],
    [UserId], [TeamleaderId], [SectionheadId], [WorkingDays]
)
SELECT
    [Id], [Type], [Status], [StartDate], [EndDate], [Reason], [NoteForManager],
    [MedicalCertificateFileName], [MedicalCertificatePath], [DateCreated],
    [UserId], [TeamleaderId], [SectionheadId],
    CASE
        WHEN [StartDate] IS NULL OR [EndDate] IS NULL THEN 0
        WHEN DATEDIFF(DAY, [StartDate], [EndDate]) + 1 < 0 THEN 0
        ELSE DATEDIFF(DAY, [StartDate], [EndDate]) + 1
    END
FROM [$(SourceDb)].[dbo].[LeaveRequests];

SET IDENTITY_INSERT [hr].[LeaveRequests] OFF;

SET IDENTITY_INSERT [hr].[Permissions] ON;

INSERT INTO [hr].[Permissions] WITH (TABLOCK)
(
    [Id], [Type], [Status], [PermissionDate], [FromTime], [ToTime],
    [Reason], [CreatedAt], [UpdatedAt], [UserId], [TeamleaderId], [SectionheadId]
)
SELECT
    [Id], [Type], [Status], [PermissionDate], [FromTime], [ToTime],
    [Reason], [CreatedAt], [UpdatedAt], [UserId], [TeamleaderId], [SectionheadId]
FROM [$(SourceDb)].[dbo].[Permissions];

SET IDENTITY_INSERT [hr].[Permissions] OFF;

SET IDENTITY_INSERT [hr].[WorkFromHomeRequests] ON;

INSERT INTO [hr].[WorkFromHomeRequests] WITH (TABLOCK)
(
    [Id], [Date], [DateCreated], [NoteForManager], [Status],
    [UserId], [TeamleaderId], [SectionheadId]
)
SELECT
    [Id], [Date], [DateCreated], [NoteForManager], [Status],
    [UserId], [TeamleaderId], [SectionheadId]
FROM [$(SourceDb)].[dbo].[WorkFromHomeRequests];

SET IDENTITY_INSERT [hr].[WorkFromHomeRequests] OFF;

SET IDENTITY_INSERT [hr].[Opinions] ON;

INSERT INTO [hr].[Opinions] WITH (TABLOCK)
(
    [Id], [Comment], [IsApproved], [CreatedAt], [UserId],
    [LeaveRequestId], [PermissionId], [WorkFromHomeRequestId]
)
SELECT
    [Id], [Comment], [IsApproved], [CreatedAt], [UserId],
    [LeaveRequestId], [PermissionId], [WorkFromHomeRequestId]
FROM [$(SourceDb)].[dbo].[Opinions];

SET IDENTITY_INSERT [hr].[Opinions] OFF;

SET IDENTITY_INSERT [identity].[UserChanges] ON;

INSERT INTO [identity].[UserChanges] WITH (TABLOCK)
(
    [Id], [Action], [Changes], [ChangedAt], [ChangedByUserName], [UserId], [ChangedByUserId]
)
SELECT
    [Id], [Action], [Changes], [ChangedAt], [ChangedByUserName], [UserId], [ChangedByUserId]
FROM [$(SourceDb)].[dbo].[UserChanges];

SET IDENTITY_INSERT [identity].[UserChanges] OFF;
GO

PRINT N'=== Legacy import: reseed identities ===';

DECLARE @ReseedSql NVARCHAR(MAX) = N'';

SELECT @ReseedSql += N'
IF EXISTS (SELECT 1 FROM ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N')
    DBCC CHECKIDENT (' + QUOTENAME(CONCAT(s.name, '.', t.name), '''') + N', RESEED);'
FROM sys.tables AS t
INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
INNER JOIN sys.identity_columns AS ic ON ic.object_id = t.object_id
WHERE s.name IN (N'organization', N'identity', N'workflows', N'curriculum', N'ticket', N'sprints', N'notifications', N'hr')
  AND t.name NOT IN (N'Roles', N'Permissions', N'RolePermissions');

EXEC sys.sp_executesql @ReseedSql;
GO

PRINT N'=== Legacy import: re-enable foreign keys ===';

DECLARE @enableSql NVARCHAR(MAX) = N'';

SELECT @enableSql += N'ALTER TABLE '
    + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
    + N' WITH CHECK CHECK CONSTRAINT ALL;' + CHAR(10)
FROM sys.tables AS t
INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
WHERE s.name IN (N'organization', N'identity', N'workflows', N'curriculum', N'ticket', N'sprints', N'notifications', N'hr')
  AND t.name NOT IN (N'Roles', N'Permissions', N'RolePermissions', N'PublicHolidays');

EXEC sys.sp_executesql @enableSql;
GO

PRINT N'=== Legacy import: validation ===';

DECLARE @Validation TABLE
(
    [Entity] NVARCHAR(100) NOT NULL,
    [SourceCount] BIGINT NOT NULL,
    [TargetCount] BIGINT NOT NULL
);

INSERT INTO @Validation ([Entity], [SourceCount], [TargetCount])
VALUES
    (N'Teams (Groups)', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[Groups]), (SELECT COUNT(*) FROM [organization].[Teams])),
    (N'Users', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[Users]), (SELECT COUNT(*) FROM [identity].[Users])),
    (N'EmployeeBalances', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[Users]), (SELECT COUNT(*) FROM [hr].[EmployeeBalances])),
    (N'Sections', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[Sections]), (SELECT COUNT(*) FROM [organization].[Sections])),
    (N'SectionTeams', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[SectionGroups]), (SELECT COUNT(*) FROM [organization].[SectionTeams])),
    (N'TicketBank', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[TaskBank]), (SELECT COUNT(*) FROM [workflows].[TicketBank])),
    (N'Tickets', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[Tasks]), (SELECT COUNT(*) FROM [ticket].[Tickets])),
    (N'TicketActivities', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[TaskActivities]), (SELECT COUNT(*) FROM [ticket].[TicketActivities])),
    (N'TicketWorkTimes', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[TaskWorkTimes]), (SELECT COUNT(*) FROM [ticket].[TicketWorkTimes])),
    (N'Comments', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[Comments]), (SELECT COUNT(*) FROM [ticket].[Comments])),
    (N'Subjects', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[Subjects]), (SELECT COUNT(*) FROM [curriculum].[Subjects])),
    (N'LearningObjectives', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[LearningObjectives]), (SELECT COUNT(*) FROM [curriculum].[LearningObjectives])),
    (N'Notifications', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[Notifications]), (SELECT COUNT(*) FROM [notifications].[Notifications])),
    (N'LeaveRequests', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[LeaveRequests]), (SELECT COUNT(*) FROM [hr].[LeaveRequests])),
    (N'Permissions', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[Permissions]), (SELECT COUNT(*) FROM [hr].[Permissions])),
    (N'Opinions', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[Opinions]), (SELECT COUNT(*) FROM [hr].[Opinions])),
    (N'UserChanges', (SELECT COUNT(*) FROM [$(SourceDb)].[dbo].[UserChanges]), (SELECT COUNT(*) FROM [identity].[UserChanges]));

SELECT
    [Entity],
    [SourceCount],
    [TargetCount],
    CASE WHEN [SourceCount] = [TargetCount] THEN N'OK' ELSE N'MISMATCH' END AS [Status]
FROM @Validation
ORDER BY [Entity];

PRINT N'=== Legacy import: subject status breakdown (non-archived) ===';

SELECT
    [Status],
    CASE [Status]
        WHEN 0 THEN N'Active'
        WHEN 1 THEN N'Closed'
        WHEN 2 THEN N'Hold'
        WHEN 3 THEN N'Reopened'
        ELSE N'Unknown'
    END AS [StatusLabel],
    COUNT(*) AS [Count]
FROM [curriculum].[Subjects]
WHERE [Archived] = 0
GROUP BY [Status]
ORDER BY [Status];

IF EXISTS (SELECT 1 FROM @Validation WHERE [SourceCount] <> [TargetCount])
BEGIN
    RAISERROR(N'Legacy import validation failed: one or more row counts did not match.', 16, 1);
END
ELSE
BEGIN
    PRINT N'Legacy import completed successfully. All validated row counts match.';
END;
GO
