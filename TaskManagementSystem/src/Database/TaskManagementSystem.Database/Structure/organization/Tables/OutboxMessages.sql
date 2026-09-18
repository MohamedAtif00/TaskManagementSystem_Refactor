CREATE TABLE [organization].[OutboxMessages] (
    [Id]             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [OccurredOnUtc]  DATETIME2(7)     NOT NULL,
    [Type]           NVARCHAR(512)    NOT NULL,
    [Payload]        NVARCHAR(MAX)    NOT NULL,
    [ProcessedOnUtc] DATETIME2(7)     NULL,
    [Error]          NVARCHAR(4000)   NULL,
    [Attempts]       INT              NOT NULL CONSTRAINT DF_organization_OutboxMessages_Attempts DEFAULT (0)
);

CREATE INDEX IX_organization_OutboxMessages_Unprocessed
    ON [organization].[OutboxMessages]([ProcessedOnUtc], [OccurredOnUtc])
    WHERE [ProcessedOnUtc] IS NULL;