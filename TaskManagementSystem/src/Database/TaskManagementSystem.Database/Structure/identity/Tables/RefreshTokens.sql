-- Source of truth for identity.RefreshTokens
-- Deployed via Scripts/Migrations/003_identity_Tables.sql
-- Column repair for stub tables: Scripts/Migrations/025_identity_RefreshTokens_Columns.sql

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
