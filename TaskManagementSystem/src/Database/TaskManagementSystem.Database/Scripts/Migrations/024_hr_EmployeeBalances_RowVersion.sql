-- Optimistic concurrency token for hr.EmployeeBalances (EF-managed metadata updates).

IF COL_LENGTH(N'hr.EmployeeBalances', N'RowVersion') IS NULL
BEGIN
    ALTER TABLE [hr].[EmployeeBalances]
        ADD [RowVersion] ROWVERSION NOT NULL;
END
GO
