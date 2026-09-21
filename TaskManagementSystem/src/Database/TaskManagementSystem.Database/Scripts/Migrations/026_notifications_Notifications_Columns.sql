-- Align notifications.Notifications with EF/AboutMe mapping (UserId, IsRead, Category, ...).
-- 008 only CREATE TABLE IF missing, so a GUID stub (RecipientUserId, no UserId/IsRead) was never repaired.

IF OBJECT_ID(N'notifications.Notifications', N'U') IS NOT NULL
   AND COL_LENGTH(N'notifications.Notifications', N'UserId') IS NULL
BEGIN
    DROP TABLE [notifications].[Notifications];
END
GO

IF OBJECT_ID(N'notifications.Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE [notifications].[Notifications]
    (
        [Id]               INT           NOT NULL IDENTITY(1, 1),
        [Title]            NVARCHAR(MAX) NOT NULL,
        [Message]          NVARCHAR(MAX) NOT NULL,
        [Category]         NVARCHAR(MAX) NOT NULL,
        [Type]             NVARCHAR(MAX) NOT NULL,
        [Status]           NVARCHAR(MAX) NULL,
        [IsRead]           BIT           NOT NULL CONSTRAINT [DF_notifications_Notifications_IsRead] DEFAULT (0),
        [HasActions]       BIT           NOT NULL CONSTRAINT [DF_notifications_Notifications_HasActions] DEFAULT (0),
        [AdditionalData]   NVARCHAR(MAX) NULL,
        [RelatedEntityId]  INT           NULL,
        [CreatedAt]        DATETIME2     NOT NULL,
        [UserId]           INT           NOT NULL,
        CONSTRAINT [PK_notifications_Notifications] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_notifications_Notifications_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_notifications_Notifications_UserId]
        ON [notifications].[Notifications] ([UserId] ASC);
END
GO

IF COL_LENGTH(N'notifications.Notifications', N'Category') IS NULL
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ADD [Category] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_notifications_Notifications_Category] DEFAULT (N'');
END
GO

IF COL_LENGTH(N'notifications.Notifications', N'IsRead') IS NULL
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ADD [IsRead] BIT NOT NULL CONSTRAINT [DF_notifications_Notifications_IsRead] DEFAULT (0);
END
GO

IF COL_LENGTH(N'notifications.Notifications', N'HasActions') IS NULL
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ADD [HasActions] BIT NOT NULL CONSTRAINT [DF_notifications_Notifications_HasActions] DEFAULT (0);
END
GO

IF COL_LENGTH(N'notifications.Notifications', N'AdditionalData') IS NULL
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ADD [AdditionalData] NVARCHAR(MAX) NULL;
END
GO

IF COL_LENGTH(N'notifications.Notifications', N'RelatedEntityId') IS NULL
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ADD [RelatedEntityId] INT NULL;
END
GO

IF COL_LENGTH(N'notifications.Notifications', N'UserId') IS NULL
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ADD [UserId] INT NULL;
END
GO

DELETE n
FROM [notifications].[Notifications] AS n
WHERE n.[UserId] IS NULL;
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'notifications.Notifications')
      AND name = N'UserId'
      AND is_nullable = 1)
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ALTER COLUMN [UserId] INT NOT NULL;
END
GO

IF COL_LENGTH(N'notifications.Notifications', N'RecipientUserId') IS NOT NULL
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ALTER COLUMN [RecipientUserId] UNIQUEIDENTIFIER NULL;
END
GO

IF COL_LENGTH(N'notifications.Notifications', N'Channel') IS NOT NULL
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ALTER COLUMN [Channel] INT NULL;
END
GO

IF COL_LENGTH(N'notifications.Notifications', N'Priority') IS NOT NULL
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ALTER COLUMN [Priority] NVARCHAR(50) NULL;
END
GO

IF COL_LENGTH(N'notifications.Notifications', N'RetryCount') IS NOT NULL
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ALTER COLUMN [RetryCount] INT NULL;
END
GO

IF COL_LENGTH(N'notifications.Notifications', N'IsEmailSent') IS NOT NULL
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ALTER COLUMN [IsEmailSent] BIT NULL;
END
GO

IF COL_LENGTH(N'notifications.Notifications', N'IsInAppSent') IS NOT NULL
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ALTER COLUMN [IsInAppSent] BIT NULL;
END
GO

IF COL_LENGTH(N'notifications.Notifications', N'IsPushSent') IS NOT NULL
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ALTER COLUMN [IsPushSent] BIT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_notifications_Notifications_Users_UserId')
BEGIN
    ALTER TABLE [notifications].[Notifications]
        ADD CONSTRAINT [FK_notifications_Notifications_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_notifications_Notifications_UserId'
      AND object_id = OBJECT_ID(N'notifications.Notifications'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_notifications_Notifications_UserId]
        ON [notifications].[Notifications] ([UserId] ASC);
END
GO
