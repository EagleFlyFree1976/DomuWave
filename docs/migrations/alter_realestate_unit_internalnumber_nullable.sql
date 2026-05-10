DROP INDEX IF EXISTS IX_RealEstateUnit_Unique ON RealEstateUnit;

-- 2. Altera la colonna a nullable
ALTER TABLE RealEstateUnit ALTER COLUMN InternalNumber NVARCHAR(40) NULL;