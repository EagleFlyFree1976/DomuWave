-- Rimuove il campo ExpensePayer da UnitTenant
DECLARE @constraint NVARCHAR(200) = (
    SELECT dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE c.object_id = OBJECT_ID('UnitTenant') AND c.name = 'ExpensePayer'
);
IF @constraint IS NOT NULL
    EXEC('ALTER TABLE UnitTenant DROP CONSTRAINT ' + @constraint);

ALTER TABLE UnitTenant DROP COLUMN ExpensePayer;
