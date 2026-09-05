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
