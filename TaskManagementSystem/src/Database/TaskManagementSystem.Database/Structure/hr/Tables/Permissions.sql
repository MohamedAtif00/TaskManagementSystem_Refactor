-- Source of truth for hr.Permissions
-- Deployed via Scripts/Migrations/009_hr_Tables.sql

IF OBJECT_ID(N'hr.Permissions', N'U') IS NULL
BEGIN
    CREATE TABLE [hr].[Permissions]
    (
        [Id]             INT           NOT NULL IDENTITY(1, 1),
        [Type]           NVARCHAR(MAX) NOT NULL,
        [Status]         NVARCHAR(MAX) NOT NULL,
        [PermissionDate] DATETIME2     NOT NULL,
        [FromTime]       TIME          NOT NULL,
        [ToTime]         TIME          NOT NULL,
        [Reason]         NVARCHAR(MAX) NULL,
        [CreatedAt]      DATETIME2     NOT NULL,
        [UpdatedAt]      DATETIME2     NULL,
        [UserId]         INT           NOT NULL,
        [TeamleaderId]   INT           NULL,
        [SectionheadId]  INT           NULL,
        CONSTRAINT [PK_hr_Permissions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_hr_Permissions_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_hr_Permissions_UserId]
        ON [hr].[Permissions] ([UserId] ASC);
END
GO
