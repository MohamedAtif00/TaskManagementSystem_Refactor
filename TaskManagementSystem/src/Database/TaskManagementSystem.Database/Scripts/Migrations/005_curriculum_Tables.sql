IF OBJECT_ID(N'curriculum.AcademicYears', N'U') IS NULL
BEGIN
    CREATE TABLE [curriculum].[AcademicYears]
    (
        [Id]          INT           NOT NULL IDENTITY(1, 1),
        [Name]        NVARCHAR(MAX) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [Archived]    BIT           NOT NULL,
        CONSTRAINT [PK_curriculum_AcademicYears] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

IF OBJECT_ID(N'curriculum.CurriculumProjects', N'U') IS NULL
BEGIN
    CREATE TABLE [curriculum].[CurriculumProjects]
    (
        [Id]          INT           NOT NULL IDENTITY(1, 1),
        [Name]        NVARCHAR(MAX) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [Archived]    BIT           NOT NULL,
        [YearId]      INT           NOT NULL,
        CONSTRAINT [PK_curriculum_CurriculumProjects] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_curriculum_CurriculumProjects_AcademicYears_YearId]
            FOREIGN KEY ([YearId]) REFERENCES [curriculum].[AcademicYears] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_curriculum_CurriculumProjects_YearId]
        ON [curriculum].[CurriculumProjects] ([YearId] ASC);
END
GO

IF OBJECT_ID(N'curriculum.CurriculumTerms', N'U') IS NULL
BEGIN
    CREATE TABLE [curriculum].[CurriculumTerms]
    (
        [Id]        INT           NOT NULL IDENTITY(1, 1),
        [Name]      NVARCHAR(MAX) NOT NULL,
        [StartDate] DATETIME2     NULL,
        [EndDate]   DATETIME2     NULL,
        [Archived]  BIT           NOT NULL,
        [ProjectId] INT           NOT NULL,
        CONSTRAINT [PK_curriculum_CurriculumTerms] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_curriculum_CurriculumTerms_CurriculumProjects_ProjectId]
            FOREIGN KEY ([ProjectId]) REFERENCES [curriculum].[CurriculumProjects] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_curriculum_CurriculumTerms_ProjectId]
        ON [curriculum].[CurriculumTerms] ([ProjectId] ASC);
END
GO

IF OBJECT_ID(N'curriculum.SubjectGroups', N'U') IS NULL
BEGIN
    CREATE TABLE [curriculum].[SubjectGroups]
    (
        [Id]       INT           NOT NULL IDENTITY(1, 1),
        [Name]     NVARCHAR(MAX) NOT NULL,
        [Archived] BIT           NOT NULL,
        [TermId]   INT           NOT NULL,
        CONSTRAINT [PK_curriculum_SubjectGroups] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_curriculum_SubjectGroups_CurriculumTerms_TermId]
            FOREIGN KEY ([TermId]) REFERENCES [curriculum].[CurriculumTerms] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_curriculum_SubjectGroups_TermId]
        ON [curriculum].[SubjectGroups] ([TermId] ASC);
END
GO

IF OBJECT_ID(N'curriculum.Subjects', N'U') IS NULL
BEGIN
    CREATE TABLE [curriculum].[Subjects]
    (
        [Id]                 INT           NOT NULL IDENTITY(1, 1),
        [Name]               NVARCHAR(MAX) NOT NULL,
        [Description]        NVARCHAR(MAX) NOT NULL,
        [Status]             INT           NOT NULL CONSTRAINT [DF_curriculum_Subjects_Status] DEFAULT (0),
        [Archived]           BIT           NOT NULL,
        [ArchivedWithFolder] BIT           NOT NULL,
        [SubjectGroupId]     INT           NOT NULL,
        CONSTRAINT [PK_curriculum_Subjects] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_curriculum_Subjects_SubjectGroups_SubjectGroupId]
            FOREIGN KEY ([SubjectGroupId]) REFERENCES [curriculum].[SubjectGroups] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_curriculum_Subjects_SubjectGroupId]
        ON [curriculum].[Subjects] ([SubjectGroupId] ASC);
END
GO

IF OBJECT_ID(N'curriculum.Units', N'U') IS NULL
BEGIN
    CREATE TABLE [curriculum].[Units]
    (
        [Id]        INT           NOT NULL IDENTITY(1, 1),
        [Name]      NVARCHAR(MAX) NOT NULL,
        [Archived]  BIT           NOT NULL,
        [SubjectId] INT           NOT NULL,
        CONSTRAINT [PK_curriculum_Units] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_curriculum_Units_Subjects_SubjectId]
            FOREIGN KEY ([SubjectId]) REFERENCES [curriculum].[Subjects] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_curriculum_Units_SubjectId]
        ON [curriculum].[Units] ([SubjectId] ASC);
END
GO

IF OBJECT_ID(N'curriculum.Lessons', N'U') IS NULL
BEGIN
    CREATE TABLE [curriculum].[Lessons]
    (
        [Id]       INT           NOT NULL IDENTITY(1, 1),
        [Name]     NVARCHAR(MAX) NOT NULL,
        [Archived] BIT           NOT NULL,
        [UnitId]   INT           NOT NULL,
        CONSTRAINT [PK_curriculum_Lessons] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_curriculum_Lessons_Units_UnitId]
            FOREIGN KEY ([UnitId]) REFERENCES [curriculum].[Units] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_curriculum_Lessons_UnitId]
        ON [curriculum].[Lessons] ([UnitId] ASC);
END
GO

IF OBJECT_ID(N'curriculum.LearningObjectives', N'U') IS NULL
BEGIN
    CREATE TABLE [curriculum].[LearningObjectives]
    (
        [Id]         INT           NOT NULL IDENTITY(1, 1),
        [Name]       NVARCHAR(MAX) NOT NULL,
        [Tag]        NVARCHAR(MAX) NOT NULL,
        [Template]   NVARCHAR(MAX) NOT NULL,
        [Environment] NVARCHAR(MAX) NOT NULL,
        [CreateAt]   DATETIME2     NOT NULL,
        [StartedAt]  DATETIME2     NULL,
        [DoneAt]     DATETIME2     NULL,
        [Archived]   BIT           NOT NULL,
        [LessonId]   INT           NOT NULL,
        [SchemaId]   INT           NOT NULL,
        CONSTRAINT [PK_curriculum_LearningObjectives] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_curriculum_LearningObjectives_Lessons_LessonId]
            FOREIGN KEY ([LessonId]) REFERENCES [curriculum].[Lessons] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_curriculum_LearningObjectives_Schemas_SchemaId]
            FOREIGN KEY ([SchemaId]) REFERENCES [workflows].[Schemas] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_curriculum_LearningObjectives_LessonId]
        ON [curriculum].[LearningObjectives] ([LessonId] ASC);

    CREATE NONCLUSTERED INDEX [IX_curriculum_LearningObjectives_SchemaId]
        ON [curriculum].[LearningObjectives] ([SchemaId] ASC);
END
GO

IF OBJECT_ID(N'curriculum.SubjectUser', N'U') IS NULL
BEGIN
    CREATE TABLE [curriculum].[SubjectUser]
    (
        [SubjectsId] INT NOT NULL,
        [UsersId]    INT NOT NULL,
        CONSTRAINT [PK_curriculum_SubjectUser] PRIMARY KEY CLUSTERED ([SubjectsId] ASC, [UsersId] ASC),
        CONSTRAINT [FK_curriculum_SubjectUser_Subjects_SubjectsId]
            FOREIGN KEY ([SubjectsId]) REFERENCES [curriculum].[Subjects] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_curriculum_SubjectUser_Users_UsersId]
            FOREIGN KEY ([UsersId]) REFERENCES [identity].[Users] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_curriculum_SubjectUser_UsersId]
        ON [curriculum].[SubjectUser] ([UsersId] ASC);
END
GO
