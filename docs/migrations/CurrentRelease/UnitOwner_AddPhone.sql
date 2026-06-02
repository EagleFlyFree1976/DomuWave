-- ============================================================================
-- UnitOwner: aggiunta colonna Phone (numero di telefono del proprietario/condomino)
-- Allineata alla convenzione esistente (UnitTenant.Phone NVARCHAR(20) NULL).
-- ============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('UnitOwner') AND name = 'Phone'
)
BEGIN
    ALTER TABLE UnitOwner ADD Phone NVARCHAR(20) NULL;
END
GO
