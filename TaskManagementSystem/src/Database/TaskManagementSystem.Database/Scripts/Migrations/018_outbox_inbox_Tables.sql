DECLARE @Schemas TABLE (SchemaName sysname NOT NULL);
INSERT INTO @Schemas (SchemaName)
VALUES
    (N'ticket'),
    (N'notifications'),
    (N'identity'),
    (N'hr'),
    (N'curriculum'),
    (N'sprints'),
    (N'workflows'),
    (N'organization');

DECLARE @SchemaName sysname;
DECLARE schema_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT SchemaName FROM @Schemas;

OPEN schema_cursor;
FETCH NEXT FROM schema_cursor INTO @SchemaName;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.tables t
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE s.name = @SchemaName AND t.name = N'OutboxMessages')
    BEGIN
        EXEC(N'
            CREATE TABLE [' + @SchemaName + N'].[OutboxMessages] (
                [Id]             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                [OccurredOnUtc]  DATETIME2(7)     NOT NULL,
                [Type]           NVARCHAR(512)    NOT NULL,
                [Payload]        NVARCHAR(MAX)    NOT NULL,
                [ProcessedOnUtc] DATETIME2(7)     NULL,
                [Error]          NVARCHAR(4000)   NULL,
                [Attempts]       INT              NOT NULL CONSTRAINT DF_' + @SchemaName + N'_OutboxMessages_Attempts DEFAULT (0)
            );

            CREATE INDEX IX_' + @SchemaName + N'_OutboxMessages_Unprocessed
                ON [' + @SchemaName + N'].[OutboxMessages]([ProcessedOnUtc], [OccurredOnUtc])
                WHERE [ProcessedOnUtc] IS NULL;
        ');
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.tables t
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE s.name = @SchemaName AND t.name = N'InboxMessages')
    BEGIN
        EXEC(N'
            CREATE TABLE [' + @SchemaName + N'].[InboxMessages] (
                [Id]                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                [IntegrationEventId]  UNIQUEIDENTIFIER NOT NULL,
                [ConsumerName]        NVARCHAR(256)    NOT NULL,
                [OccurredOnUtc]       DATETIME2(7)     NOT NULL,
                [Type]                NVARCHAR(512)    NOT NULL,
                [Payload]             NVARCHAR(MAX)    NOT NULL,
                [ProcessedOnUtc]      DATETIME2(7)     NULL,
                [Error]               NVARCHAR(4000)   NULL,
                [Attempts]            INT              NOT NULL CONSTRAINT DF_' + @SchemaName + N'_InboxMessages_Attempts DEFAULT (0)
            );

            CREATE UNIQUE INDEX IX_' + @SchemaName + N'_InboxMessages_EventConsumer
                ON [' + @SchemaName + N'].[InboxMessages]([IntegrationEventId], [ConsumerName]);
        ');
    END

    FETCH NEXT FROM schema_cursor INTO @SchemaName;
END

CLOSE schema_cursor;
DEALLOCATE schema_cursor;
GO
