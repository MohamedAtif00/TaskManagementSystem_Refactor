-- Source of truth for identity.Roles
-- Deployed via Scripts/Migrations/014_identity_Rbac.sql

IF OBJECT_ID(N'identity.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE [identity].[Roles]
    (
        [Id]          INT            NOT NULL,
        [Name]        NVARCHAR(100)  NOT NULL,
        [Description] NVARCHAR(500)  NULL,
        [IsSystem]    BIT            NOT NULL CONSTRAINT [DF_identity_Roles_IsSystem] DEFAULT (0),
        CONSTRAINT [PK_identity_Roles] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO
