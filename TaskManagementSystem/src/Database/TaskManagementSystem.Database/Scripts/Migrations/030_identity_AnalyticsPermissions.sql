-- Seed analytics.read permission and grant to Owner (admin) role.

DECLARE @Permissions TABLE ([Code] NVARCHAR(200), [Name] NVARCHAR(200), [Description] NVARCHAR(500));

INSERT INTO @Permissions ([Code], [Name], [Description]) VALUES
    (N'analytics.read', N'Read analytics', N'View analytics overviews and summaries');

INSERT INTO [identity].[Permissions] ([Code], [Name], [Description], [IsSystem])
SELECT p.[Code], p.[Name], p.[Description], 1
FROM @Permissions AS p
WHERE NOT EXISTS (
    SELECT 1
    FROM [identity].[Permissions] AS existing
    WHERE existing.[Code] = p.[Code]);
GO

DECLARE @OwnerRoleId INT = 4;

INSERT INTO [identity].[RolePermissions] ([RoleId], [PermissionId])
SELECT @OwnerRoleId, p.[Id]
FROM [identity].[Permissions] AS p
WHERE p.[Code] = N'analytics.read'
  AND NOT EXISTS (
      SELECT 1
      FROM [identity].[RolePermissions] AS rp
      WHERE rp.[RoleId] = @OwnerRoleId
        AND rp.[PermissionId] = p.[Id]);
GO

-- ProjectManager and team roles: read analytics alongside other read permissions
DECLARE @ProjectManagerRoleId INT = 0;

INSERT INTO [identity].[RolePermissions] ([RoleId], [PermissionId])
SELECT @ProjectManagerRoleId, p.[Id]
FROM [identity].[Permissions] AS p
WHERE p.[Code] = N'analytics.read'
  AND NOT EXISTS (
      SELECT 1
      FROM [identity].[RolePermissions] AS rp
      WHERE rp.[RoleId] = @ProjectManagerRoleId
        AND rp.[PermissionId] = p.[Id]);
GO

DECLARE @LeaderRoleIds TABLE ([RoleId] INT);
INSERT INTO @LeaderRoleIds VALUES (1), (2), (3);

INSERT INTO [identity].[RolePermissions] ([RoleId], [PermissionId])
SELECT r.[RoleId], p.[Id]
FROM @LeaderRoleIds AS r
CROSS JOIN [identity].[Permissions] AS p
WHERE p.[Code] = N'analytics.read'
  AND NOT EXISTS (
      SELECT 1
      FROM [identity].[RolePermissions] AS rp
      WHERE rp.[RoleId] = r.[RoleId]
        AND rp.[PermissionId] = p.[Id]);
GO
