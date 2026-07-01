-- ============================================================================
-- TenantDisplaySettings: logo configurabile per tenant.
--   LogoContent     -> contenuto binario del logo (VARBINARY(MAX)).
--   LogoContentType -> content-type MIME (es. image/png).
--   LogoFileName    -> nome file originale caricato.
--   LogoUpdatedDate -> data ultimo aggiornamento (cache-busting lato client).
--   Il logo appare in sidebar (dopo login) e nei report esportati (Excel/PDF).
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('TenantDisplaySettings') AND name = 'LogoContent'
)
BEGIN
    ALTER TABLE TenantDisplaySettings ADD LogoContent VARBINARY(MAX) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('TenantDisplaySettings') AND name = 'LogoContentType'
)
BEGIN
    ALTER TABLE TenantDisplaySettings ADD LogoContentType NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('TenantDisplaySettings') AND name = 'LogoFileName'
)
BEGIN
    ALTER TABLE TenantDisplaySettings ADD LogoFileName NVARCHAR(260) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('TenantDisplaySettings') AND name = 'LogoUpdatedDate'
)
BEGIN
    ALTER TABLE TenantDisplaySettings ADD LogoUpdatedDate DATETIME2 NULL;
END
GO
