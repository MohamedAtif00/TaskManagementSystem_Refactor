IF OBJECT_ID(N'organization.Teams', N'U') IS NULL
BEGIN
    CREATE TABLE [organization].[Teams]
    (
        [Id]       INT            NOT NULL IDENTITY(1, 1),
        [Name]     NVARCHAR(MAX)  NOT NULL,
        [Archived] BIT            NOT NULL,
        CONSTRAINT [PK_organization_Teams] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

IF OBJECT_ID(N'organization.Sections', N'U') IS NULL
BEGIN
    CREATE TABLE [organization].[Sections]
    (
        [Id]       INT            NOT NULL IDENTITY(1, 1),
        [Name]     NVARCHAR(MAX)  NOT NULL,
        [Archived] BIT            NOT NULL,
        [HeadId]   INT            NOT NULL,
        CONSTRAINT [PK_organization_Sections] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_organization_Sections_HeadId]
        ON [organization].[Sections] ([HeadId] ASC);
END
GO

IF OBJECT_ID(N'organization.SectionTeams', N'U') IS NULL
BEGIN
    CREATE TABLE [organization].[SectionTeams]
    (
        [Id]        INT NOT NULL IDENTITY(1, 1),
        [SectionId] INT NOT NULL,
        [TeamId]    INT NOT NULL,
        CONSTRAINT [PK_organization_SectionTeams] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_organization_SectionTeams_Sections_SectionId]
            FOREIGN KEY ([SectionId]) REFERENCES [organization].[Sections] ([Id]),
        CONSTRAINT [FK_organization_SectionTeams_Teams_TeamId]
            FOREIGN KEY ([TeamId]) REFERENCES [organization].[Teams] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_organization_SectionTeams_SectionId]
        ON [organization].[SectionTeams] ([SectionId] ASC);

    CREATE NONCLUSTERED INDEX [IX_organization_SectionTeams_TeamId]
        ON [organization].[SectionTeams] ([TeamId] ASC);
END
GO
