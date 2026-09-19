-- Source of truth for identity.Users (GroupId removed; TeamId only)
-- Deployed via Scripts/Migrations/003_identity_Tables.sql
-- Team leader lives on organization.Teams.TeamleaderId

IF OBJECT_ID(N'identity.Users', N'U') IS NULL
BEGIN
    CREATE TABLE [identity].[Users]
    (
        [Id]                       INT            NOT NULL IDENTITY(1, 1),
        [Name]                     NVARCHAR(MAX)  NOT NULL,
        [Code]                     NVARCHAR(6)    NOT NULL,
        [HR_code]                  NVARCHAR(MAX)  NOT NULL,
        [Email]                    NVARCHAR(MAX)  NULL,
        [Phone]                    NVARCHAR(MAX)  NULL,
        [Title]                    NVARCHAR(MAX)  NULL,
        [Role]                     INT            NOT NULL CONSTRAINT [DF_identity_Users_Role] DEFAULT (3),
        [AccountType]              INT            NOT NULL,
        [OnBoard]                  BIT            NOT NULL,
        [Archived]                 BIT            NOT NULL,
        [TeamId]                   INT            NULL,
        CONSTRAINT [PK_identity_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO
