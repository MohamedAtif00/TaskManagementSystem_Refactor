-- Source of truth for identity.Permissions (ACL catalog; not HR leave permissions)
-- Deployed via Scripts/Migrations/014_identity_Rbac.sql

IF OBJECT_ID(N'identity.Permissions', N'U') IS NULL
BEGIN
    CREATE TABLE [identity].[Permissions]
    (
        [Id]          INT            NOT NULL IDENTITY(1, 1),
        [Code]        NVARCHAR(200)  NOT NULL,
        [Name]        NVARCHAR(200)  NOT NULL,
        [Description] NVARCHAR(500)  NULL,
        [IsSystem]    BIT            NOT NULL CONSTRAINT [DF_identity_Permissions_IsSystem] DEFAULT (0),
        CONSTRAINT [PK_identity_Permissions] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO
