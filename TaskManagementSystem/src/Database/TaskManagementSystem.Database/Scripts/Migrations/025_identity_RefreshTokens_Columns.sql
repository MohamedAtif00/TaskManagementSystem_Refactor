-- Align identity.RefreshTokens with EF RefreshToken mapping (Created, Expires, Used, UserId).
-- 003 only CREATE TABLE IF missing, so stub tables (CreatedAt/ExpiresAt/UserIdentityId) were never repaired.

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

IF COL_LENGTH(N'identity.RefreshTokens', N'Created') IS NULL
BEGIN
    ALTER TABLE [identity].[RefreshTokens]
        ADD [Created] DATETIME2 NOT NULL CONSTRAINT [DF_identity_RefreshTokens_Created] DEFAULT (SYSUTCDATETIME());
END
GO

IF COL_LENGTH(N'identity.RefreshTokens', N'Expires') IS NULL
BEGIN
    ALTER TABLE [identity].[RefreshTokens]
        ADD [Expires] DATETIME2 NOT NULL CONSTRAINT [DF_identity_RefreshTokens_Expires] DEFAULT (SYSUTCDATETIME());
END
GO

IF COL_LENGTH(N'identity.RefreshTokens', N'Used') IS NULL
BEGIN
    ALTER TABLE [identity].[RefreshTokens]
        ADD [Used] BIT NOT NULL CONSTRAINT [DF_identity_RefreshTokens_Used] DEFAULT (0);
END
GO

IF COL_LENGTH(N'identity.RefreshTokens', N'UserId') IS NULL
BEGIN
    ALTER TABLE [identity].[RefreshTokens]
        ADD [UserId] INT NULL;
END
GO

DELETE rt
FROM [identity].[RefreshTokens] AS rt
WHERE rt.[UserId] IS NULL;
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'identity.RefreshTokens')
      AND name = N'UserId'
      AND is_nullable = 1)
BEGIN
    ALTER TABLE [identity].[RefreshTokens]
        ALTER COLUMN [UserId] INT NOT NULL;
END
GO

IF COL_LENGTH(N'identity.RefreshTokens', N'CreatedAt') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[RefreshTokens]
        ALTER COLUMN [CreatedAt] DATETIME2 NULL;
END
GO

IF COL_LENGTH(N'identity.RefreshTokens', N'ExpiresAt') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[RefreshTokens]
        ALTER COLUMN [ExpiresAt] DATETIME2 NULL;
END
GO

IF COL_LENGTH(N'identity.RefreshTokens', N'CreatedByIp') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[RefreshTokens]
        ALTER COLUMN [CreatedByIp] NVARCHAR(50) NULL;
END
GO

IF COL_LENGTH(N'identity.RefreshTokens', N'UserIdentityId') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[RefreshTokens]
        ALTER COLUMN [UserIdentityId] UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_identity_RefreshTokens_Users_UserId')
BEGIN
    ALTER TABLE [identity].[RefreshTokens]
        ADD CONSTRAINT [FK_identity_RefreshTokens_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_identity_RefreshTokens_UserId'
      AND object_id = OBJECT_ID(N'identity.RefreshTokens'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_identity_RefreshTokens_UserId]
        ON [identity].[RefreshTokens] ([UserId] ASC);
END
GO
