IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'identity')
    EXEC(N'CREATE SCHEMA [identity]');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'organization')
    EXEC(N'CREATE SCHEMA [organization]');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'workflows')
    EXEC(N'CREATE SCHEMA [workflows]');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'curriculum')
    EXEC(N'CREATE SCHEMA [curriculum]');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'ticket')
    EXEC(N'CREATE SCHEMA [ticket]');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'sprints')
    EXEC(N'CREATE SCHEMA [sprints]');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'notifications')
    EXEC(N'CREATE SCHEMA [notifications]');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'hr')
    EXEC(N'CREATE SCHEMA [hr]');
GO
