DECLARE @DbName sysname = DB_NAME();

DECLARE @Sql nvarchar(max) =
    N'ALTER DATABASE [' + @DbName + N'] SET ALLOW_SNAPSHOT_ISOLATION ON; ' +
    N'ALTER DATABASE [' + @DbName + N'] SET READ_COMMITTED_SNAPSHOT ON;';

EXEC (@Sql);
GO
