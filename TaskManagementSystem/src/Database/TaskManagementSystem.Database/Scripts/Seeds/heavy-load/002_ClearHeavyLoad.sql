-- Removes heavy-load seed rows (SEED_* / SD#### users). Safe to re-run.
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @SeedUserIds TABLE ([Id] INT NOT NULL PRIMARY KEY);
INSERT INTO @SeedUserIds ([Id])
SELECT [Id] FROM [identity].[Users] WHERE [Code] LIKE N'SD%';

DECLARE @SeedLoIds TABLE ([Id] INT NOT NULL PRIMARY KEY);
INSERT INTO @SeedLoIds ([Id])
SELECT [Id] FROM [curriculum].[LearningObjectives] WHERE [Name] LIKE N'SEED_%';

DECLARE @SeedTicketIds TABLE ([Id] INT NOT NULL PRIMARY KEY);
INSERT INTO @SeedTicketIds ([Id])
SELECT [Id] FROM [ticket].[Tasks] WHERE [Name] LIKE N'SEED_%';

-- Ticket dependents
DELETE ta
FROM [ticket].[TaskActivities] AS ta
INNER JOIN @SeedTicketIds AS t ON t.[Id] = ta.[TaskId];

DELETE tw
FROM [ticket].[TaskWorkTimes] AS tw
INNER JOIN @SeedTicketIds AS t ON t.[Id] = tw.[TaskId];

DELETE c
FROM [ticket].[Comments] AS c
INNER JOIN @SeedLoIds AS lo ON lo.[Id] = c.[LearningObjectiveId];

DELETE FROM [ticket].[Tasks] WHERE [Name] LIKE N'SEED_%';

-- Sprints
DELETE slo
FROM [sprints].[SprintLearningObjectives] AS slo
INNER JOIN [sprints].[Sprints] AS s ON s.[Id] = slo.[SprintId]
WHERE s.[Name] LIKE N'SEED_%';

DELETE FROM [sprints].[Sprints] WHERE [Name] LIKE N'SEED_%';

-- Notifications
DELETE FROM [notifications].[Notifications] WHERE [Title] LIKE N'SEED_%';

-- HR opinions on seeded requests
DELETE o
FROM [hr].[Opinions] AS o
WHERE o.[LeaveRequestId] IN (SELECT [Id] FROM [hr].[LeaveRequests] WHERE [Reason] = N'SEED_HEAVY_LOAD')
   OR o.[PermissionId] IN (SELECT [Id] FROM [hr].[Permissions] WHERE [Reason] = N'SEED_HEAVY_LOAD')
   OR o.[WorkFromHomeRequestId] IN (SELECT [Id] FROM [hr].[WorkFromHomeRequests] WHERE [NoteForManager] = N'SEED_HEAVY_LOAD')
   OR o.[ForgotClockRequestId] IN (SELECT [Id] FROM [hr].[ForgotClockRequests] WHERE [Reason] = N'SEED_HEAVY_LOAD');

DELETE FROM [hr].[LeaveRequests] WHERE [Reason] = N'SEED_HEAVY_LOAD';
DELETE FROM [hr].[Permissions] WHERE [Reason] = N'SEED_HEAVY_LOAD';
DELETE FROM [hr].[WorkFromHomeRequests] WHERE [NoteForManager] = N'SEED_HEAVY_LOAD';
DELETE FROM [hr].[ForgotClockRequests] WHERE [Reason] = N'SEED_HEAVY_LOAD';
DELETE FROM [hr].[PublicHolidays] WHERE [Name] LIKE N'SEED_%';

-- Curriculum (SubjectUser first)
DELETE su
FROM [curriculum].[SubjectUser] AS su
INNER JOIN [curriculum].[Subjects] AS s ON s.[Id] = su.[SubjectsId]
WHERE s.[Name] LIKE N'SEED_%';

DELETE FROM [curriculum].[LearningObjectives] WHERE [Name] LIKE N'SEED_%';
DELETE FROM [curriculum].[Lessons] WHERE [Name] LIKE N'SEED_%';
DELETE FROM [curriculum].[Units] WHERE [Name] LIKE N'SEED_%';
DELETE FROM [curriculum].[Subjects] WHERE [Name] LIKE N'SEED_%';
DELETE FROM [curriculum].[SubjectGroups] WHERE [Name] LIKE N'SEED_%';
DELETE FROM [curriculum].[CurriculumTerms] WHERE [Name] LIKE N'SEED_%';
DELETE FROM [curriculum].[CurriculumProjects] WHERE [Name] LIKE N'SEED_%';
DELETE FROM [curriculum].[AcademicYears] WHERE [Name] LIKE N'SEED_%';

-- Workflows (steps/nodes cascade from schema delete if configured - delete explicitly)
DELETE st
FROM [workflows].[Steps] AS st
INNER JOIN [workflows].[Nodes] AS n ON n.[Id] = st.[NodeId]
WHERE n.[Name] LIKE N'SEED_%';

DELETE FROM [workflows].[Nodes] WHERE [Name] LIKE N'SEED_%';
DELETE FROM [workflows].[TaskBank] WHERE [Name] LIKE N'SEED_%';
DELETE FROM [workflows].[Schemas] WHERE [Name] LIKE N'SEED_%';

-- Organization + identity
DELETE st
FROM [organization].[SectionTeams] AS st
INNER JOIN [organization].[Sections] AS sec ON sec.[Id] = st.[SectionId]
WHERE sec.[Name] LIKE N'SEED_%';

DELETE FROM [organization].[Sections] WHERE [Name] LIKE N'SEED_%';

DELETE FROM [hr].[EmployeeBalances]
WHERE [UserId] IN (SELECT [Id] FROM @SeedUserIds);

DELETE FROM [identity].[RefreshTokens]
WHERE [UserId] IN (SELECT [Id] FROM @SeedUserIds);

DELETE FROM [identity].[Users] WHERE [Code] LIKE N'SD%';
DELETE FROM [organization].[Teams] WHERE [Name] LIKE N'SEED_%';

PRINT N'Heavy-load seed cleared.';
GO
