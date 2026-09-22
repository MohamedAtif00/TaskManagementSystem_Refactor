-- Sample curriculum tree whose LearningObjectives.Name values use the LO-code grammar
-- (Subject_Grade_Term_Unit_Lesson_LO, optional year / QR_ / _p2).
-- SubjectGroup is the full catalog name (mth→Math, ara→Arabic, sci→Science, eng→English,
-- soc→Social Studies, mul→Multimedia, rel→Religion, ict→ICT, tsk→Tokkatsu).
-- Re-runnable. Does not touch heavy-load SEED_* rows.
-- Run via seed-database.ps1 (Seeds/*.sql) or:
--   DatabaseMigrator --seed <connectionString> <this file>

SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM [workflows].[SchemaTypes] WHERE [Name] = N'LO Code')
BEGIN
    INSERT INTO [workflows].[SchemaTypes] ([Name], [Description])
    VALUES (N'LO Code', N'Default type for sample LO codes');
END

DECLARE @TypeId INT = (SELECT TOP 1 [Id] FROM [workflows].[SchemaTypes] WHERE [Name] = N'LO Code');
IF @TypeId IS NULL
    SET @TypeId = (SELECT TOP 1 [Id] FROM [workflows].[SchemaTypes] ORDER BY [Id]);

IF NOT EXISTS (SELECT 1 FROM [workflows].[Schemas] WHERE [Name] = N'LO Code Schema')
BEGIN
    INSERT INTO [workflows].[Schemas] ([Name], [Description], [Archived], [TypeId])
    VALUES (N'LO Code Schema', N'Schema for seeded LO codes', 0, @TypeId);
END

DECLARE @SchemaId INT = (SELECT TOP 1 [Id] FROM [workflows].[Schemas] WHERE [Name] = N'LO Code Schema' AND [Archived] = 0);
IF @SchemaId IS NULL
    SET @SchemaId = (SELECT TOP 1 [Id] FROM [workflows].[Schemas] WHERE [Archived] = 0 ORDER BY [Id]);

IF @SchemaId IS NULL
BEGIN
    RAISERROR(N'Cannot seed LO codes: no workflow schema exists. Run migrations first.', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM [curriculum].[AcademicYears] WHERE [Name] = N'2026' AND [Archived] = 0)
BEGIN
    INSERT INTO [curriculum].[AcademicYears] ([Name], [Description], [Archived])
    VALUES (N'2026', N'Sample academic year for LO codes', 0);
END

DECLARE @YearId INT = (SELECT TOP 1 [Id] FROM [curriculum].[AcademicYears] WHERE [Name] = N'2026' AND [Archived] = 0);

IF NOT EXISTS (
    SELECT 1
    FROM [curriculum].[CurriculumProjects]
    WHERE [Name] = N'Primary Curriculum' AND [YearId] = @YearId AND [Archived] = 0)
BEGIN
    INSERT INTO [curriculum].[CurriculumProjects] ([Name], [Description], [Archived], [YearId])
    VALUES (N'Primary Curriculum', N'Sample project for LO codes', 0, @YearId);
END

DECLARE @ProjectId INT = (
    SELECT TOP 1 [Id]
    FROM [curriculum].[CurriculumProjects]
    WHERE [Name] = N'Primary Curriculum' AND [YearId] = @YearId AND [Archived] = 0);

-- SubjectGroup is the full catalog name for the 3-letter code (mth→Math, ara→Arabic, ...).
DECLARE @Codes TABLE
(
    [Code]         NVARCHAR(100) NOT NULL PRIMARY KEY,
    [SubjectCode]  NVARCHAR(3)   NOT NULL,
    [SubjectGroup] NVARCHAR(100) NOT NULL,
    [Subject]      NVARCHAR(100) NOT NULL,
    [TermName]     NVARCHAR(100) NOT NULL,
    [UnitName]     NVARCHAR(100) NOT NULL,
    [LessonName]   NVARCHAR(100) NOT NULL
);

INSERT INTO @Codes ([Code], [SubjectCode], [SubjectGroup], [Subject], [TermName], [UnitName], [LessonName])
VALUES
    (N'Mth_5R_1A_01_04_02', N'mth', N'Math', N'Math Grade 5', N'Term 1 (Arabic)', N'Unit 1', N'Lesson 4'),
    (N'Ara_5R_1A_01_01_04', N'ara', N'Arabic', N'Arabic Grade 5', N'Term 1 (Arabic)', N'Unit 1', N'Lesson 1'),
    (N'Sci_5R_1A_04_03_05', N'sci', N'Science', N'Science Grade 5', N'Term 1 (Arabic)', N'Unit 4', N'Lesson 3'),
    (N'Soc_5R_1A_02_03_02', N'soc', N'Social Studies', N'Social Studies Grade 5', N'Term 1 (Arabic)', N'Unit 2', N'Lesson 3'),
    (N'Rel_3R_1A_05_02_01', N'rel', N'Religion', N'Religion Grade 3', N'Term 1 (Arabic)', N'Unit 5', N'Lesson 2'),
    (N'Eng_5R_1E_07_04_04', N'eng', N'English', N'English Grade 5', N'Term 1 (English)', N'Unit 7', N'Lesson 4'),
    (N'Mul_2R_1E_01_03_01', N'mul', N'Multimedia', N'Multimedia Grade 2', N'Term 1 (English)', N'Unit 1', N'Lesson 3'),
    (N'Ict_5R_1A_01_02_01', N'ict', N'ICT', N'ICT Grade 5', N'Term 1 (Arabic)', N'Unit 1', N'Lesson 2'),
    (N'Tsk_1R_1A_01_01_01', N'tsk', N'Tokkatsu', N'Tokkatsu Grade 1', N'Term 1 (Arabic)', N'Unit 1', N'Lesson 1'),
    (N'2026_ara_2r_1a_02_05_03', N'ara', N'Arabic', N'Arabic Grade 2', N'Term 1 (Arabic)', N'Unit 2', N'Lesson 5'),
    (N'2026_eng_4r_1e_02_02_03', N'eng', N'English', N'English Grade 4', N'Term 1 (English)', N'Unit 2', N'Lesson 2'),
    (N'QR_mth_1r_1a_02_02_06', N'mth', N'Math', N'Math Grade 1', N'Term 1 (Arabic)', N'Unit 2', N'Lesson 2'),
    (N'QR_2025_ara_3r_1a_01_01_01', N'ara', N'Arabic', N'Arabic Grade 3', N'Term 1 (Arabic)', N'Unit 1', N'Lesson 1'),
    (N'Soc_4R_1A_01_04_03_p2', N'soc', N'Social Studies', N'Social Studies Grade 4', N'Term 1 (Arabic)', N'Unit 1', N'Lesson 4');

DECLARE @Code NVARCHAR(100);
DECLARE @SubjectCode NVARCHAR(3);
DECLARE @SubjectGroup NVARCHAR(100);
DECLARE @Subject NVARCHAR(100);
DECLARE @TermName NVARCHAR(100);
DECLARE @UnitName NVARCHAR(100);
DECLARE @LessonName NVARCHAR(100);
DECLARE @TermId INT;
DECLARE @GroupId INT;
DECLARE @SubjectId INT;
DECLARE @UnitId INT;
DECLARE @LessonId INT;

DECLARE code_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT [Code], [SubjectCode], [SubjectGroup], [Subject], [TermName], [UnitName], [LessonName]
    FROM @Codes;

OPEN code_cursor;
FETCH NEXT FROM code_cursor INTO @Code, @SubjectCode, @SubjectGroup, @Subject, @TermName, @UnitName, @LessonName;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM [curriculum].[CurriculumTerms]
        WHERE [Name] = @TermName AND [ProjectId] = @ProjectId AND [Archived] = 0)
    BEGIN
        INSERT INTO [curriculum].[CurriculumTerms] ([Name], [StartDate], [EndDate], [Archived], [ProjectId])
        VALUES (@TermName, NULL, NULL, 0, @ProjectId);
    END

    SET @TermId = (
        SELECT TOP 1 [Id]
        FROM [curriculum].[CurriculumTerms]
        WHERE [Name] = @TermName AND [ProjectId] = @ProjectId AND [Archived] = 0);

    IF NOT EXISTS (
        SELECT 1
        FROM [curriculum].[SubjectGroups]
        WHERE [Name] = @SubjectGroup AND [TermId] = @TermId AND [Archived] = 0)
    BEGIN
        INSERT INTO [curriculum].[SubjectGroups] ([Name], [Archived], [TermId])
        VALUES (@SubjectGroup, 0, @TermId);
    END

    SET @GroupId = (
        SELECT TOP 1 [Id]
        FROM [curriculum].[SubjectGroups]
        WHERE [Name] = @SubjectGroup AND [TermId] = @TermId AND [Archived] = 0);

    IF NOT EXISTS (
        SELECT 1
        FROM [curriculum].[Subjects]
        WHERE [Name] = @Subject AND [SubjectGroupId] = @GroupId AND [Archived] = 0)
    BEGIN
        INSERT INTO [curriculum].[Subjects]
            ([Name], [Description], [Status], [Archived], [ArchivedWithFolder], [SubjectGroupId])
        VALUES
            (@Subject, @Subject, 0, 0, 0, @GroupId);
    END

    SET @SubjectId = (
        SELECT TOP 1 [Id]
        FROM [curriculum].[Subjects]
        WHERE [Name] = @Subject AND [SubjectGroupId] = @GroupId AND [Archived] = 0);

    IF NOT EXISTS (
        SELECT 1
        FROM [curriculum].[Units]
        WHERE [Name] = @UnitName AND [SubjectId] = @SubjectId AND [Archived] = 0)
    BEGIN
        INSERT INTO [curriculum].[Units] ([Name], [Archived], [SubjectId])
        VALUES (@UnitName, 0, @SubjectId);
    END

    SET @UnitId = (
        SELECT TOP 1 [Id]
        FROM [curriculum].[Units]
        WHERE [Name] = @UnitName AND [SubjectId] = @SubjectId AND [Archived] = 0);

    IF NOT EXISTS (
        SELECT 1
        FROM [curriculum].[Lessons]
        WHERE [Name] = @LessonName AND [UnitId] = @UnitId AND [Archived] = 0)
    BEGIN
        INSERT INTO [curriculum].[Lessons] ([Name], [Archived], [UnitId])
        VALUES (@LessonName, 0, @UnitId);
    END

    SET @LessonId = (
        SELECT TOP 1 [Id]
        FROM [curriculum].[Lessons]
        WHERE [Name] = @LessonName AND [UnitId] = @UnitId AND [Archived] = 0);

    IF NOT EXISTS (
        SELECT 1
        FROM [curriculum].[LearningObjectives]
        WHERE [Name] = @Code AND [Archived] = 0)
    BEGIN
        INSERT INTO [curriculum].[LearningObjectives]
        (
            [Name], [Tag], [Template], [Environment], [CreateAt],
            [StartedAt], [DoneAt], [Archived], [LessonId], [SchemaId]
        )
        VALUES
        (
            @Code,
            UPPER(@SubjectCode),
            N'default',
            N'Web',
            SYSUTCDATETIME(),
            NULL,
            NULL,
            0,
            @LessonId,
            @SchemaId
        );
    END

    FETCH NEXT FROM code_cursor INTO @Code, @SubjectCode, @SubjectGroup, @Subject, @TermName, @UnitName, @LessonName;
END

CLOSE code_cursor;
DEALLOCATE code_cursor;

DECLARE @UserId INT = (SELECT TOP 1 [Id] FROM [identity].[Users] WHERE [Code] = N'TST001' AND [Archived] = 0);
IF @UserId IS NOT NULL
BEGIN
    INSERT INTO [curriculum].[SubjectUser] ([SubjectsId], [UsersId])
    SELECT s.[Id], @UserId
    FROM [curriculum].[Subjects] AS s
    INNER JOIN [curriculum].[SubjectGroups] AS g ON g.[Id] = s.[SubjectGroupId]
    INNER JOIN [curriculum].[CurriculumTerms] AS t ON t.[Id] = g.[TermId]
    WHERE t.[ProjectId] = @ProjectId
      AND s.[Archived] = 0
      AND NOT EXISTS (
          SELECT 1
          FROM [curriculum].[SubjectUser] AS su
          WHERE su.[SubjectsId] = s.[Id] AND su.[UsersId] = @UserId);
END
