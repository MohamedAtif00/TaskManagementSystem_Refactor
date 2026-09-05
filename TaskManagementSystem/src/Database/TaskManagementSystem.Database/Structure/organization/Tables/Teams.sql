-- Source of truth for organization.Teams
-- Deployed via Scripts/Migrations/002_organization_Tables.sql

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
