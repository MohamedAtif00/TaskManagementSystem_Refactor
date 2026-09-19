-- Team leader is owned by organization.Teams (source of truth).

IF COL_LENGTH(N'organization.Teams', N'TeamleaderId') IS NULL
BEGIN
    ALTER TABLE [organization].[Teams]
        ADD [TeamleaderId] INT NULL;
END
GO

UPDATE t
SET t.[TeamleaderId] = leaders.[LeaderId]
FROM [organization].[Teams] AS t
INNER JOIN (
    SELECT u.[TeamId], MIN(u.[Id]) AS [LeaderId]
    FROM [identity].[Users] AS u
    WHERE u.[Role] = 2
      AND u.[Archived] = 0
      AND u.[TeamId] IS NOT NULL
    GROUP BY u.[TeamId]
) AS leaders ON leaders.[TeamId] = t.[Id]
WHERE t.[TeamleaderId] IS NULL;
GO

IF COL_LENGTH(N'identity.Users', N'TeamleaderId') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
        UPDATE t
        SET t.[TeamleaderId] = u.[TeamleaderId]
        FROM [organization].[Teams] AS t
        INNER JOIN (
            SELECT u.[TeamId], MIN(u.[TeamleaderId]) AS [TeamleaderId]
            FROM [identity].[Users] AS u
            WHERE u.[TeamleaderId] IS NOT NULL
              AND u.[Archived] = 0
              AND u.[TeamId] IS NOT NULL
            GROUP BY u.[TeamId]
        ) AS u ON u.[TeamId] = t.[Id]
        WHERE t.[TeamleaderId] IS NULL;';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_organization_Teams_Users_TeamleaderId')
BEGIN
    ALTER TABLE [organization].[Teams]
        ADD CONSTRAINT [FK_organization_Teams_Users_TeamleaderId]
            FOREIGN KEY ([TeamleaderId]) REFERENCES [identity].[Users] ([Id]);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_organization_Teams_TeamleaderId'
      AND object_id = OBJECT_ID(N'organization.Teams'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_organization_Teams_TeamleaderId]
        ON [organization].[Teams] ([TeamleaderId] ASC);
END
GO
