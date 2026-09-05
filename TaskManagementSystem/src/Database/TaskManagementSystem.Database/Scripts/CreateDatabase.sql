IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'TaskManagementSystem')
BEGIN
    CREATE DATABASE [TaskManagementSystem];
END
GO
