-- Aggiunge IsDefault alla tabella MillesimalTable
-- La tabella con Code='DEF' diventa quella di default per ogni condominio

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('MillesimalTable') AND name = 'IsDefault'
)
BEGIN
    ALTER TABLE MillesimalTable ADD IsDefault BIT NOT NULL DEFAULT 0;
END

-- Imposta IsDefault=1 per le tabelle DEF già esistenti
UPDATE MillesimalTable SET IsDefault = 1 WHERE Code = 'DEF' AND IsDefault = 0;

-- Vincolo unicità: un solo default per condominio
-- (usa un indice filtered su IsDefault=1)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UQ_MillesimalTable_DefaultPerCondominium'
      AND object_id = OBJECT_ID('MillesimalTable')
)
BEGIN
    CREATE UNIQUE INDEX UQ_MillesimalTable_DefaultPerCondominium
        ON MillesimalTable (CondominiumId)
        WHERE IsDefault = 1 AND IsDeleted = 0;
END
