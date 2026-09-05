IF OBJECT_ID(N'identity.Users', N'U') IS NULL
BEGIN
    CREATE TABLE [identity].[Users]
    (
        [Id]                       INT            NOT NULL IDENTITY(1, 1),
        [Name]                     NVARCHAR(MAX)  NOT NULL,
        [Code]                     NVARCHAR(6)    NOT NULL,
        [HR_code]                  NVARCHAR(MAX)  NOT NULL,
        [Email]                    NVARCHAR(MAX)  NULL,
        [Phone]                    NVARCHAR(MAX)  NULL,
        [Title]                    NVARCHAR(MAX)  NULL,
        [Role]                     INT            NOT NULL CONSTRAINT [DF_identity_Users_Role] DEFAULT (3),
        [AccountType]              INT            NOT NULL,
        [OnBoard]                  BIT            NOT NULL,
        [Archived]                 BIT            NOT NULL,
        [TeamId]                   INT            NULL,
        [TeamleaderId]             INT            NULL,
        [Annual_leave]             INT            NOT NULL,
        [Annual_leave_MAX]         INT            NOT NULL,
        [Emergency_leave]          INT            NOT NULL,
        [Emergency_leave_MAX]      INT            NOT NULL,
        [Sick_leave]               INT            NOT NULL,
        [Permission]               INT            NOT NULL,
        [Permission_MAX]           INT            NOT NULL,
        [WorkFromHome]             INT            NOT NULL,
        [WorkFromHome_MAX]         INT            NOT NULL,
        [FromNextBalanceDaysUsed]  INT            NOT NULL,
        [OldAnnualBalance]         INT            NOT NULL,
        CONSTRAINT [PK_identity_Users] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_identity_Users_Teams_TeamId]
            FOREIGN KEY ([TeamId]) REFERENCES [organization].[Teams] ([Id]),
        CONSTRAINT [FK_identity_Users_Users_TeamleaderId]
            FOREIGN KEY ([TeamleaderId]) REFERENCES [identity].[Users] ([Id])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [IX_identity_Users_Code]
        ON [identity].[Users] ([Code] ASC);

    CREATE NONCLUSTERED INDEX [IX_identity_Users_TeamId]
        ON [identity].[Users] ([TeamId] ASC);

    CREATE NONCLUSTERED INDEX [IX_identity_Users_TeamleaderId]
        ON [identity].[Users] ([TeamleaderId] ASC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_organization_Sections_Users_HeadId'
      AND parent_object_id = OBJECT_ID(N'organization.Sections'))
BEGIN
    ALTER TABLE [organization].[Sections]
        ADD CONSTRAINT [FK_organization_Sections_Users_HeadId]
            FOREIGN KEY ([HeadId]) REFERENCES [identity].[Users] ([Id]);
END
GO

IF OBJECT_ID(N'identity.RefreshTokens', N'U') IS NULL
BEGIN
    CREATE TABLE [identity].[RefreshTokens]
    (
        [Token]   NVARCHAR(450) NOT NULL,
        [Created] DATETIME2     NOT NULL,
        [Expires] DATETIME2     NOT NULL,
        [Used]    BIT           NOT NULL CONSTRAINT [DF_identity_RefreshTokens_Used] DEFAULT (0),
        [UserId]  INT           NOT NULL,
        CONSTRAINT [PK_identity_RefreshTokens] PRIMARY KEY CLUSTERED ([Token] ASC),
        CONSTRAINT [FK_identity_RefreshTokens_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_identity_RefreshTokens_UserId]
        ON [identity].[RefreshTokens] ([UserId] ASC);
END
GO

IF OBJECT_ID(N'identity.UserChanges', N'U') IS NULL
BEGIN
    CREATE TABLE [identity].[UserChanges]
    (
        [Id]                 INT            NOT NULL IDENTITY(1, 1),
        [Action]             NVARCHAR(MAX)  NOT NULL,
        [Changes]            NVARCHAR(MAX)  NOT NULL,
        [ChangedAt]          DATETIME2      NOT NULL,
        [ChangedByUserName]  NVARCHAR(MAX)  NOT NULL,
        [UserId]             INT            NOT NULL,
        [ChangedByUserId]    INT            NOT NULL,
        CONSTRAINT [PK_identity_UserChanges] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_identity_UserChanges_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]),
        CONSTRAINT [FK_identity_UserChanges_Users_ChangedByUserId]
            FOREIGN KEY ([ChangedByUserId]) REFERENCES [identity].[Users] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_identity_UserChanges_UserId]
        ON [identity].[UserChanges] ([UserId] ASC);

    CREATE NONCLUSTERED INDEX [IX_identity_UserChanges_ChangedByUserId]
        ON [identity].[UserChanges] ([ChangedByUserId] ASC);
END
GO
