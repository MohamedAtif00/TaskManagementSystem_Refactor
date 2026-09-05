-- Source of truth for identity.Users (GroupId removed; TeamId only)
-- Deployed via Scripts/Migrations/003_identity_Tables.sql

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
        CONSTRAINT [PK_identity_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO
