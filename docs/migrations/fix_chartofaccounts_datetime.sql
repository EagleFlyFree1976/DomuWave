-- Fix: converti colonne CreationDate/LastUpdateDate di ChartOfAccounts da datetime a datetime2
-- per evitare "out-of-range value" quando il valore è DateTime.MinValue (0001-01-01)

-- Correggi eventuali valori fuori range prima di alterare il tipo
UPDATE ChartOfAccounts
SET CreationDate = '1900-01-01'
WHERE CreationDate < '1753-01-01';

UPDATE ChartOfAccounts
SET LastUpdateDate = NULL
WHERE LastUpdateDate IS NOT NULL AND LastUpdateDate < '1753-01-01';

-- Converti le colonne a datetime2 per supportare il range completo di .NET DateTime
ALTER TABLE ChartOfAccounts ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE ChartOfAccounts ALTER COLUMN LastUpdateDate datetime2 NULL;
