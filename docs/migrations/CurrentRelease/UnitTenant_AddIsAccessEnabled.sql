SET QUOTED_IDENTIFIER ON;
GO

-- ============================================================================
-- UnitTenant: aggiunge il flag IsAccessEnabled (accesso alla piattaforma).
-- Allinea UnitTenant a UnitOwner, che ha già questo campo.
-- Default 1 (true): ogni occupante creato ha accesso abilitato salvo diversa scelta.
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.UnitTenant') AND name = 'IsAccessEnabled'
)
BEGIN
    ALTER TABLE dbo.UnitTenant
        ADD IsAccessEnabled BIT NOT NULL CONSTRAINT DF_UnitTenant_IsAccessEnabled DEFAULT 1;
END
GO
