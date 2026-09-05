IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = N'app' AND t.name = N'AuditLog')
BEGIN
    CREATE TABLE [app].[AuditLog] (
        [Id]             BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [CorrelationId]  NVARCHAR(64)  NOT NULL,
        [UserId]         INT           NULL,
        [ActionName]     NVARCHAR(256) NOT NULL,
        [OccurredOnUtc]  DATETIME2(7)  NOT NULL,
        [Success]        BIT           NOT NULL
    );

    CREATE INDEX IX_AuditLog_OccurredOnUtc ON [app].[AuditLog]([OccurredOnUtc]);
    CREATE INDEX IX_AuditLog_UserId ON [app].[AuditLog]([UserId]) WHERE [UserId] IS NOT NULL;
END
GO
