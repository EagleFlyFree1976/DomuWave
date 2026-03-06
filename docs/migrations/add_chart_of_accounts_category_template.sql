-- ============================================================
-- Migration: ChartOfAccountsCategoryTemplate
-- Date: 2026-03-06
-- Scopo: tabella di template per le categorie del piano dei conti.
--        I template attivi vengono copiati automaticamente come
--        ChartOfAccountsCategory al momento della creazione di un
--        nuovo tenant.
-- ============================================================

-- 1. Crea la tabella template (nessun TenantId: è di sistema)
CREATE TABLE ChartOfAccountsCategoryTemplate (
    Id                    INT           NOT NULL,
    Name                  NVARCHAR(200) NOT NULL,
    Description           NVARCHAR(500) NULL,
    IsActive              BIT           NOT NULL DEFAULT 1,
    IsDeleted             BIT           NOT NULL DEFAULT 0,
    CreatedById           INT           NOT NULL DEFAULT 0,
    CreatedByFullName     NVARCHAR(200) NULL,
    LastUpdatedById       INT           NULL,
    LastUpdatedByFullName NVARCHAR(200) NULL,
    CONSTRAINT PK_ChartOfAccountsCategoryTemplate PRIMARY KEY (Id)
);

-- 2. Riga hilo per la nuova entità
INSERT INTO hibernate_unique_key (entity_type, next_hi)
VALUES ('ChartOfAccountsCategoryTemplate', 1);

-- 3. (Opzionale) Seed di template iniziali
--    Inserire manualmente gli Id usando valori fuori dal range hilo corrente,
--    oppure usare il pannello SuperAdmin dell'applicazione.
--
-- Esempio (commentato — usare l'UI):
-- INSERT INTO ChartOfAccountsCategoryTemplate (Id, Name, Description, IsActive, IsDeleted, CreatedById)
-- VALUES
--   (1, 'Ordinaria',      'Spese di gestione ordinaria',          1, 0, 1),
--   (2, 'Straordinaria',  'Spese di manutenzione straordinaria',  1, 0, 1),
--   (3, 'Utenze',         'Acqua, luce, gas condominiali',        1, 0, 1),
--   (4, 'Pulizie',        'Servizi di pulizia parti comuni',      1, 0, 1),
--   (5, 'Assicurazioni',  'Polizze assicurative',                 1, 0, 1);
