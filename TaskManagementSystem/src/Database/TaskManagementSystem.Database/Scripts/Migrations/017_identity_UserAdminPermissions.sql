IF NOT EXISTS (SELECT 1 FROM [identity].[Permissions] WHERE [Code] = N'identity.users.view')
BEGIN
    INSERT INTO [identity].[Permissions] ([Code], [Name], [Description], [IsSystem])
    VALUES
        (N'identity.users.view',   N'View users',   N'List and view user profiles', 1),
        (N'identity.users.manage', N'Manage users', N'Create, update, and archive users', 1);
END
GO

DECLARE @OwnerRoleId INT = 4;

INSERT INTO [identity].[RolePermissions] ([RoleId], [PermissionId])
SELECT @OwnerRoleId, p.[Id]
FROM [identity].[Permissions] p
WHERE p.[Code] IN (N'identity.users.view', N'identity.users.manage')
  AND NOT EXISTS (
      SELECT 1
      FROM [identity].[RolePermissions] rp
      WHERE rp.[RoleId] = @OwnerRoleId
        AND rp.[PermissionId] = p.[Id]);
GO
