-- ============================================================================
-- ConsumptionCharge: aggiunta colonna IsManual.
-- Indica che gli importi delle quote sono stati modificati manualmente:
-- il ricalcolo automatico non sovrascrive piu' gli importi (solo consumi/percentuali).
-- ============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('ConsumptionCharge') AND name = 'IsManual'
)
BEGIN
    ALTER TABLE ConsumptionCharge ADD IsManual BIT NOT NULL CONSTRAINT DF_ConsumptionCharge_IsManual DEFAULT 0;
END
GO
