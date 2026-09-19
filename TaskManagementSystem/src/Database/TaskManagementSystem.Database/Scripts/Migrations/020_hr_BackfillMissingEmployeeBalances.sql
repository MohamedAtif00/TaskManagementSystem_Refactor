-- Backfill hr.EmployeeBalances for users created after migration 015
-- when the Identity outbox DI bug prevented UserCreatedIntegrationEvent from reaching HR.

INSERT INTO [hr].[EmployeeBalances]
(
    [UserId],
    [TeamId],
    [TeamleaderId],
    [Role],
    [AnnualLeave],
    [AnnualLeaveMax],
    [EmergencyLeave],
    [EmergencyLeaveMax],
    [SickLeave],
    [Permission],
    [PermissionMax],
    [WorkFromHome],
    [WorkFromHomeMax],
    [FromNextBalanceDaysUsed],
    [OldAnnualBalance]
)
SELECT
    u.[Id],
    u.[TeamId],
    NULL,
    u.[Role],
    0,
    30,
    0,
    5,
    0,
    0,
    10,
    0,
    5,
    0,
    0
FROM [identity].[Users] AS u
WHERE NOT EXISTS (
    SELECT 1
    FROM [hr].[EmployeeBalances] AS eb
    WHERE eb.[UserId] = u.[Id]
);
GO
