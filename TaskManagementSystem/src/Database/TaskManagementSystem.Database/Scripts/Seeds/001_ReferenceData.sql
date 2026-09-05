IF NOT EXISTS (SELECT 1 FROM [workflows].[SchemaTypes])
BEGIN
    INSERT INTO [workflows].[SchemaTypes] ([Name], [Description])
    VALUES
        (N'Default', N'Default workflow schema type');
END
GO
