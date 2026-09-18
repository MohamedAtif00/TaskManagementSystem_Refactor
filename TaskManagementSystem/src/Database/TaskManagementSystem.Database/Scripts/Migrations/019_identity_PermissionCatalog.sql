-- Expand RBAC to a fixed migration-seeded catalog. Removes runtime permission CRUD need.

-- Migrate legacy codes
UPDATE [identity].[Permissions]
SET [Code] = N'identity.users.read',
    [Name] = N'Read users',
    [Description] = N'List and view user profiles'
WHERE [Code] = N'identity.users.view';
GO

-- Remove obsolete permission-definition CRUD permission
DELETE rp
FROM [identity].[RolePermissions] AS rp
INNER JOIN [identity].[Permissions] AS p ON p.[Id] = rp.[PermissionId]
WHERE p.[Code] = N'identity.permissions.manage';
GO

DELETE FROM [identity].[Permissions]
WHERE [Code] = N'identity.permissions.manage';
GO

-- Remove legacy granular identity codes superseded by verb catalog
DELETE rp
FROM [identity].[RolePermissions] AS rp
INNER JOIN [identity].[Permissions] AS p ON p.[Id] = rp.[PermissionId]
WHERE p.[Code] IN (
    N'identity.roles.manage',
    N'identity.users.assign-role',
    N'identity.users.manage');
GO

DELETE FROM [identity].[Permissions]
WHERE [Code] IN (
    N'identity.roles.manage',
    N'identity.users.assign-role',
    N'identity.users.manage');
GO

-- Insert full catalog (IsSystem = 1)
DECLARE @Permissions TABLE ([Code] NVARCHAR(200), [Name] NVARCHAR(200), [Description] NVARCHAR(500));

INSERT INTO @Permissions ([Code], [Name], [Description]) VALUES
    (N'organization.read', N'Read organization', N'View teams and sections'),
    (N'organization.create', N'Create organization', N'Create teams and sections'),
    (N'organization.update', N'Update organization', N'Update teams and sections'),
    (N'organization.delete', N'Archive organization', N'Archive teams and sections'),
    (N'organization.manage', N'Manage organization', N'Full organization access'),

    (N'workflows.read', N'Read workflows', N'View workflow schemas, nodes, steps, and task bank'),
    (N'workflows.create', N'Create workflows', N'Create workflow definitions'),
    (N'workflows.update', N'Update workflows', N'Update workflow definitions'),
    (N'workflows.delete', N'Archive workflows', N'Archive workflow definitions'),
    (N'workflows.manage', N'Manage workflows', N'Full workflow access'),

    (N'curriculum.read', N'Read curriculum', N'View curriculum hierarchy'),
    (N'curriculum.create', N'Create curriculum', N'Create curriculum entities'),
    (N'curriculum.update', N'Update curriculum', N'Update curriculum entities'),
    (N'curriculum.delete', N'Archive curriculum', N'Archive curriculum entities'),
    (N'curriculum.manage', N'Manage curriculum', N'Full curriculum access'),

    (N'sprints.read', N'Read sprints', N'View sprints and linked learning objectives'),
    (N'sprints.create', N'Create sprints', N'Create sprints'),
    (N'sprints.update', N'Update sprints', N'Update sprints and LO links'),
    (N'sprints.delete', N'Archive sprints', N'Archive sprints'),
    (N'sprints.manage', N'Manage sprints', N'Full sprint access'),

    (N'tickets.read', N'Read tickets', N'View tickets, comments, and work time'),
    (N'tickets.create', N'Create tickets', N'Create tickets and comments'),
    (N'tickets.update', N'Update tickets', N'Assign, proceed, complete, and update tickets'),
    (N'tickets.delete', N'Archive tickets', N'Archive or remove ticket data'),
    (N'tickets.manage', N'Manage tickets', N'Full ticket access'),

    (N'notifications.read', N'Read notifications', N'View notification inbox'),
    (N'notifications.update', N'Update notifications', N'Mark notifications read'),
    (N'notifications.manage', N'Manage notifications', N'Full notification inbox access'),

    (N'hr.leave.read', N'Read leave', N'View leave balances, settings, and requests'),
    (N'hr.leave.create', N'Create leave', N'Create and preview leave requests'),
    (N'hr.leave.update', N'Update leave', N'Cancel leave and record opinions'),
    (N'hr.leave.delete', N'Delete leave', N'Remove leave requests where allowed'),
    (N'hr.leave.manage', N'Manage leave', N'Approve leave and full HR leave access'),

    (N'hr.holidays.read', N'Read holidays', N'View public holidays'),
    (N'hr.holidays.create', N'Create holidays', N'Create public holidays'),
    (N'hr.holidays.update', N'Update holidays', N'Update public holidays'),
    (N'hr.holidays.delete', N'Delete holidays', N'Delete public holidays'),
    (N'hr.holidays.manage', N'Manage holidays', N'Full public holiday administration'),

    (N'hr.timeoff.read', N'Read time-off permissions', N'View permission (time-off) requests'),
    (N'hr.timeoff.create', N'Create time-off permissions', N'Create permission requests'),
    (N'hr.timeoff.update', N'Update time-off permissions', N'Cancel and record opinions on permission requests'),
    (N'hr.timeoff.delete', N'Delete time-off permissions', N'Remove permission requests where allowed'),
    (N'hr.timeoff.manage', N'Manage time-off permissions', N'Approve permission requests and full access'),

    (N'hr.workfromhome.read', N'Read work from home', N'View WFH requests'),
    (N'hr.workfromhome.create', N'Create work from home', N'Create WFH requests'),
    (N'hr.workfromhome.update', N'Update work from home', N'Cancel and record opinions on WFH requests'),
    (N'hr.workfromhome.delete', N'Delete work from home', N'Remove WFH requests where allowed'),
    (N'hr.workfromhome.manage', N'Manage work from home', N'Approve WFH requests and full access'),

    (N'hr.forgotclock.read', N'Read forgot clock', N'View forgot clock requests'),
    (N'hr.forgotclock.create', N'Create forgot clock', N'Create forgot clock requests'),
    (N'hr.forgotclock.update', N'Update forgot clock', N'Cancel and record opinions on forgot clock requests'),
    (N'hr.forgotclock.delete', N'Delete forgot clock', N'Remove forgot clock requests where allowed'),
    (N'hr.forgotclock.manage', N'Manage forgot clock', N'Approve forgot clock requests and full access'),

    (N'identity.users.read', N'Read users', N'List and view user profiles'),
    (N'identity.users.create', N'Create users', N'Create users'),
    (N'identity.users.update', N'Update users', N'Update users and assign roles'),
    (N'identity.users.delete', N'Archive users', N'Archive users'),
    (N'identity.users.manage', N'Manage users', N'Full user administration'),

    (N'identity.roles.read', N'Read roles', N'List roles and permissions catalog'),
    (N'identity.roles.create', N'Create roles', N'Create roles'),
    (N'identity.roles.update', N'Update roles', N'Update roles'),
    (N'identity.roles.delete', N'Delete roles', N'Delete roles'),
    (N'identity.roles.manage', N'Manage roles', N'Assign permissions to roles and full role administration');

INSERT INTO [identity].[Permissions] ([Code], [Name], [Description], [IsSystem])
SELECT p.[Code], p.[Name], p.[Description], 1
FROM @Permissions AS p
WHERE NOT EXISTS (SELECT 1 FROM [identity].[Permissions] AS existing WHERE existing.[Code] = p.[Code]);
GO

-- Owner: all manage permissions
DECLARE @OwnerRoleId INT = 4;

INSERT INTO [identity].[RolePermissions] ([RoleId], [PermissionId])
SELECT @OwnerRoleId, p.[Id]
FROM [identity].[Permissions] AS p
WHERE p.[Code] LIKE N'%.manage'
  AND NOT EXISTS (
      SELECT 1
      FROM [identity].[RolePermissions] AS rp
      WHERE rp.[RoleId] = @OwnerRoleId AND rp.[PermissionId] = p.[Id]);
GO

-- ProjectManger: HR manage + read on core modules
DECLARE @ProjectManagerRoleId INT = 0;

INSERT INTO [identity].[RolePermissions] ([RoleId], [PermissionId])
SELECT @ProjectManagerRoleId, p.[Id]
FROM [identity].[Permissions] AS p
WHERE p.[Code] IN (
    N'hr.holidays.manage',
    N'hr.leave.manage', N'hr.leave.read',
    N'hr.timeoff.manage', N'hr.timeoff.read',
    N'hr.workfromhome.manage', N'hr.workfromhome.read',
    N'hr.forgotclock.manage', N'hr.forgotclock.read',
    N'organization.read', N'workflows.read', N'curriculum.read',
    N'sprints.read', N'tickets.read', N'notifications.read')
  AND NOT EXISTS (
      SELECT 1 FROM [identity].[RolePermissions] AS rp
      WHERE rp.[RoleId] = @ProjectManagerRoleId AND rp.[PermissionId] = p.[Id]);
GO

-- SectionHead / TeamLeader / Member: read + own HR create/update
DECLARE @LeaderRoleIds TABLE ([RoleId] INT);
INSERT INTO @LeaderRoleIds VALUES (1), (2), (3);

INSERT INTO [identity].[RolePermissions] ([RoleId], [PermissionId])
SELECT r.[RoleId], p.[Id]
FROM @LeaderRoleIds AS r
CROSS JOIN [identity].[Permissions] AS p
WHERE p.[Code] IN (
    N'organization.read', N'workflows.read', N'curriculum.read', N'sprints.read',
    N'tickets.read', N'tickets.create', N'tickets.update',
    N'notifications.read', N'notifications.update',
    N'hr.leave.read', N'hr.leave.create', N'hr.leave.update',
    N'hr.timeoff.read', N'hr.timeoff.create', N'hr.timeoff.update',
    N'hr.workfromhome.read', N'hr.workfromhome.create', N'hr.workfromhome.update',
    N'hr.forgotclock.read', N'hr.forgotclock.create', N'hr.forgotclock.update',
    N'hr.holidays.read')
  AND NOT EXISTS (
      SELECT 1 FROM [identity].[RolePermissions] AS rp
      WHERE rp.[RoleId] = r.[RoleId] AND rp.[PermissionId] = p.[Id]);
GO

-- Optional cleanup of non-system custom permissions
DELETE rp
FROM [identity].[RolePermissions] AS rp
INNER JOIN [identity].[Permissions] AS p ON p.[Id] = rp.[PermissionId]
WHERE p.[IsSystem] = 0;
GO

DELETE FROM [identity].[Permissions] WHERE [IsSystem] = 0;
GO
