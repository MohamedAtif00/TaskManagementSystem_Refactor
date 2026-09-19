-- Team leader lives on organization.Teams only. Drop legacy identity.Users.TeamleaderId.
-- No-op on fresh databases created from updated 003.

IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_identity_Users_Users_TeamleaderId'
      AND parent_object_id = OBJECT_ID(N'identity.Users'))
BEGIN
    ALTER TABLE [identity].[Users]
        DROP CONSTRAINT [FK_identity_Users_Users_TeamleaderId];
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_identity_Users_TeamleaderId'
      AND object_id = OBJECT_ID(N'identity.Users'))
BEGIN
    DROP INDEX [IX_identity_Users_TeamleaderId] ON [identity].[Users];
END
GO

IF COL_LENGTH(N'identity.Users', N'TeamleaderId') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[Users] DROP COLUMN [TeamleaderId];
END
GO
