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

    CREATE UNIQUE NONCLUSTERED INDEX [IX_identity_Roles_Name]
        ON [identity].[Roles] ([Name] ASC);
END
GO

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

    CREATE UNIQUE NONCLUSTERED INDEX [IX_identity_Permissions_Code]
        ON [identity].[Permissions] ([Code] ASC);
END
GO

IF OBJECT_ID(N'identity.RolePermissions', N'U') IS NULL
BEGIN
    CREATE TABLE [identity].[RolePermissions]
    (
        [RoleId]       INT NOT NULL,
        [PermissionId] INT NOT NULL,
        CONSTRAINT [PK_identity_RolePermissions] PRIMARY KEY CLUSTERED ([RoleId] ASC, [PermissionId] ASC),
        CONSTRAINT [FK_identity_RolePermissions_Roles_RoleId]
            FOREIGN KEY ([RoleId]) REFERENCES [identity].[Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_identity_RolePermissions_Permissions_PermissionId]
            FOREIGN KEY ([PermissionId]) REFERENCES [identity].[Permissions] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_identity_RolePermissions_PermissionId]
        ON [identity].[RolePermissions] ([PermissionId] ASC);
END
GO

IF NOT EXISTS (SELECT 1 FROM [identity].[Roles] WHERE [Id] = 0)
BEGIN
    INSERT INTO [identity].[Roles] ([Id], [Name], [Description], [IsSystem])
    VALUES
        (0, N'ProjectManger', N'System role: Project Manager', 1),
        (1, N'SectionHead',   N'System role: Section Head',   1),
        (2, N'TeamLeader',    N'System role: Team Leader',    1),
        (3, N'Member',        N'System role: Member',         1),
        (4, N'Owner',         N'System role: Owner',          1);
END
GO

IF NOT EXISTS (SELECT 1 FROM [identity].[Permissions] WHERE [Code] = N'identity.permissions.manage')
BEGIN
    INSERT INTO [identity].[Permissions] ([Code], [Name], [Description], [IsSystem])
    VALUES
        (N'identity.permissions.manage', N'Manage permissions', N'Create, update, and delete permission definitions', 1),
        (N'identity.roles.manage',       N'Manage roles',       N'Create, update, delete roles and assign permissions', 1),
        (N'identity.users.assign-role',  N'Assign user roles',  N'Assign roles to users', 1);
END
GO

DECLARE @OwnerRoleId INT = 4;

INSERT INTO [identity].[RolePermissions] ([RoleId], [PermissionId])
SELECT @OwnerRoleId, p.[Id]
FROM [identity].[Permissions] p
WHERE p.[Code] IN (
    N'identity.permissions.manage',
    N'identity.roles.manage',
    N'identity.users.assign-role')
  AND NOT EXISTS (
      SELECT 1
      FROM [identity].[RolePermissions] rp
      WHERE rp.[RoleId] = @OwnerRoleId
        AND rp.[PermissionId] = p.[Id]);
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_identity_Users_Roles_Role'
      AND parent_object_id = OBJECT_ID(N'identity.Users'))
BEGIN
    ALTER TABLE [identity].[Users]
        ADD CONSTRAINT [FK_identity_Users_Roles_Role]
            FOREIGN KEY ([Role]) REFERENCES [identity].[Roles] ([Id]);
END
GO
