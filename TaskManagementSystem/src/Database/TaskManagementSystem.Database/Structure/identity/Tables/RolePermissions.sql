-- Source of truth for identity.RolePermissions
-- Deployed via Scripts/Migrations/014_identity_Rbac.sql

IF OBJECT_ID(N'identity.RolePermissions', N'U') IS NULL
BEGIN
    CREATE TABLE [identity].[RolePermissions]
    (
        [RoleId]       INT NOT NULL,
        [PermissionId] INT NOT NULL,
        CONSTRAINT [PK_identity_RolePermissions] PRIMARY KEY CLUSTERED ([RoleId] ASC, [PermissionId] ASC)
    );
END
GO
