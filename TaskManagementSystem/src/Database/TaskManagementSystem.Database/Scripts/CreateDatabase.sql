:setvar TargetDb TaskManagementSystem
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'$(TargetDb)')
BEGIN
    CREATE DATABASE [$(TargetDb)];
END
GO
