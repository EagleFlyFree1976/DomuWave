-- ============================================================
-- Migration: ChartOfAccountsCategory lookup table
-- Date: 2026-03-06
-- ============================================================

-- 1. Crea la tabella di lookup
CREATE TABLE ChartOfAccountsCategory (
    Id                    INT               NOT NULL,
    TenantId              UNIQUEIDENTIFIER  NOT NULL,
    Name                  NVARCHAR(200)     NOT NULL,
    Description           NVARCHAR(500)     NULL,
    IsActive              BIT               NOT NULL DEFAULT 1,
    IsDeleted             BIT               NOT NULL DEFAULT 0,
    CreatedById           INT               NOT NULL DEFAULT 0,
    CreatedByFullName     NVARCHAR(200)     NULL,
    LastUpdatedById       INT               NULL,
    LastUpdatedByFullName NVARCHAR(200)     NULL,
    CONSTRAINT PK_ChartOfAccountsCategory PRIMARY KEY (Id)
);

-- 2. Aggiungi la chiave esterna alla tabella ChartOfAccounts
ALTER TABLE ChartOfAccounts
    ADD CategoryId INT NULL;

ALTER TABLE ChartOfAccounts
    ADD CONSTRAINT FK_ChartOfAccounts_Category
        FOREIGN KEY (CategoryId) REFERENCES ChartOfAccountsCategory(Id);

-- 3. Rimuovi il vecchio campo stringa Category (opzionale, eseguire dopo aver verificato che nessun codice lo referenzia)
-- ALTER TABLE ChartOfAccounts DROP COLUMN Category;

-- 4. Riga hilo per la nuova entità
INSERT INTO hibernate_unique_key (entity_type, next_hi)
VALUES ('ChartOfAccountsCategory', 1);
