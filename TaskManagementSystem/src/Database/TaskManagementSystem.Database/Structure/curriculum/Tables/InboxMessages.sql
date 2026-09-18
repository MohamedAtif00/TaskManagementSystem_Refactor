CREATE TABLE [curriculum].[InboxMessages] (
    [Id]                 UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [IntegrationEventId] UNIQUEIDENTIFIER NOT NULL,
    [ConsumerName]       NVARCHAR(256)    NOT NULL,
    [OccurredOnUtc]      DATETIME2(7)     NOT NULL,
    [Type]               NVARCHAR(512)    NOT NULL,
    [Payload]            NVARCHAR(MAX)    NOT NULL,
    [ProcessedOnUtc]     DATETIME2(7)     NULL,
    [Error]              NVARCHAR(4000)   NULL,
    [Attempts]           INT              NOT NULL CONSTRAINT DF_curriculum_InboxMessages_Attempts DEFAULT (0)
);

CREATE UNIQUE INDEX IX_curriculum_InboxMessages_EventConsumer
    ON [curriculum].[InboxMessages]([IntegrationEventId], [ConsumerName]);