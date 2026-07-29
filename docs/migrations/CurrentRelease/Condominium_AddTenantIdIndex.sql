-- ============================================================================
-- Condominium: indice mancante su TenantId.
--   Ogni endpoint che elenca i condomìni del tenant corrente (GetAll,
--   GetByTenantIdAsync, GetActive, GetWithUpcomingAssembly, ecc.) filtra
--   su Tenant.Id senza un indice dedicato, forzando una scansione della
--   tabella. Aggiunto per ridurre il tempo di risposta di /api/condominiums
--   e derivati.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('Condominium') AND name = 'IX_Condominium_TenantId'
)
BEGIN
    CREATE INDEX IX_Condominium_TenantId ON Condominium (TenantId) WHERE IsDeleted = 0;
END
GO
