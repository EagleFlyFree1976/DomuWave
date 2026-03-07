-- ============================================================
-- Converte automaticamente tutte le colonne datetime → datetime2
-- su tutte le tabelle del database corrente
-- ============================================================

SET NOCOUNT ON;

DECLARE @sql        NVARCHAR(MAX) = N'';
DECLARE @tableName  NVARCHAR(256);
DECLARE @columnName NVARCHAR(256);
DECLARE @isNullable BIT;

-- Cursore su tutte le colonne datetime (esclude datetime2, smalldatetime, datetimeoffset)
DECLARE cur CURSOR FAST_FORWARD FOR
    SELECT
        t.name  AS TableName,
        c.name  AS ColumnName,
        c.is_nullable
    FROM sys.columns c
    JOIN sys.tables  t ON t.object_id = c.object_id
    JOIN sys.types   tp ON tp.user_type_id = c.user_type_id
    WHERE tp.name = 'datetime'          -- solo le colonne di tipo datetime
      AND t.is_ms_shipped = 0           -- escludi tabelle di sistema
    ORDER BY t.name, c.name;

OPEN cur;
FETCH NEXT FROM cur INTO @tableName, @columnName, @isNullable;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Correggi eventuali valori fuori range prima di alterare
    SET @sql = N'UPDATE [' + @tableName + N'] SET [' + @columnName + N'] = ''1900-01-01'' WHERE [' + @columnName + N'] < ''1753-01-01'';';
    EXEC sp_executesql @sql;

    -- Altera la colonna a datetime2
    SET @sql = N'ALTER TABLE [' + @tableName + N'] ALTER COLUMN [' + @columnName + N'] datetime2 ' +
               CASE WHEN @isNullable = 1 THEN N'NULL' ELSE N'NOT NULL' END + N';';
    EXEC sp_executesql @sql;

    PRINT 'Converted: [' + @tableName + '].[' + @columnName + ']';

    FETCH NEXT FROM cur INTO @tableName, @columnName, @isNullable;
END;

CLOSE cur;
DEALLOCATE cur;

PRINT 'Done.';
