-- ============================================================================
-- TenantDisplaySettings — Branding: logo del tenant
-- Aggiunge le colonne per il logo (salvato come BLOB) e rinomina la voce di menu
-- "Impostazioni visualizzazione" in "Impostazioni" con URL /impostazioni.
-- Idempotente.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF COL_LENGTH('TenantDisplaySettings', 'LogoContent') IS NULL
    ALTER TABLE TenantDisplaySettings ADD LogoContent VARBINARY(MAX) NULL;
GO

IF COL_LENGTH('TenantDisplaySettings', 'LogoContentType') IS NULL
    ALTER TABLE TenantDisplaySettings ADD LogoContentType NVARCHAR(100) NULL;
GO

IF COL_LENGTH('TenantDisplaySettings', 'LogoFileName') IS NULL
    ALTER TABLE TenantDisplaySettings ADD LogoFileName NVARCHAR(260) NULL;
GO

IF COL_LENGTH('TenantDisplaySettings', 'LogoUpdatedDate') IS NULL
    ALTER TABLE TenantDisplaySettings ADD LogoUpdatedDate DATETIME2 NULL;
GO

-- ============================================================================
-- La pagina ora è una "Impostazioni" generica (branding + contabilità):
-- rinomina la voce di menu e aggiorna l'URL a /impostazioni.
-- Idempotente: aggiorna solo se la vecchia Action è ancora presente.
-- ============================================================================
UPDATE base_menues
   SET Description = 'Impostazioni',
       Icon        = 'pi-cog',
       Action      = '/impostazioni'
 WHERE Action = '/impostazioni-visualizzazione';
GO
