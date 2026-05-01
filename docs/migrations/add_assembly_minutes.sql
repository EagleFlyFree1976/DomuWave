-- Aggiunge BoardMembers e Minutes alla tabella Assembly

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Assembly') AND name = 'BoardMembers'
)
BEGIN
    ALTER TABLE Assembly ADD BoardMembers NVARCHAR(MAX) NULL;
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Assembly') AND name = 'Minutes'
)
BEGIN
    ALTER TABLE Assembly ADD Minutes NVARCHAR(MAX) NULL;
END
