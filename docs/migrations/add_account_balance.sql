-- Migration: crea la tabella AccountBalance per la gestione dei saldi per esercizio
-- Regole:
--   • I record vengono creati automaticamente all'apertura di un esercizio (Draft → Open)
--   • OpeningBalance è modificabile manualmente solo per il primo esercizio del condominio
--     e solo finché l'esercizio non è chiuso/bloccato
--   • ClosingBalance viene calcolato e salvato alla chiusura dell'esercizio
--   • All'apertura dell'esercizio successivo, ClosingBalance diventa OpeningBalance del nuovo

-- hilo entry per il generatore di ID
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'AccountBalance', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'AccountBalance');

CREATE TABLE AccountBalance (
    Id                   INT            NOT NULL,
    TenantId             UNIQUEIDENTIFIER NOT NULL,
    FiscalYearId         INT            NOT NULL,
    ChartOfAccountsId    INT            NOT NULL,
    OpeningBalance       DECIMAL(18,4)  NOT NULL DEFAULT 0,
    ClosingBalance       DECIMAL(18,4)  NOT NULL DEFAULT 0,
    CreatedById          INT            NOT NULL,
    CreatedByFullName    NVARCHAR(200)  NULL,
    LastUpdatedById      INT            NULL,
    LastUpdatedByFullName NVARCHAR(200) NULL,
    IsDeleted            BIT            NOT NULL DEFAULT 0,
    CreationDate         DATETIME2      NOT NULL,
    LastUpdateDate       DATETIME2      NULL,
    CONSTRAINT PK_AccountBalance PRIMARY KEY (Id),
    CONSTRAINT FK_AccountBalance_FiscalYear      FOREIGN KEY (FiscalYearId)      REFERENCES FiscalYear(Id),
    CONSTRAINT FK_AccountBalance_ChartOfAccounts FOREIGN KEY (ChartOfAccountsId) REFERENCES ChartOfAccounts(Id)
);

CREATE INDEX IX_AccountBalance_FiscalYear ON AccountBalance (FiscalYearId);
CREATE UNIQUE INDEX UX_AccountBalance_FY_Account ON AccountBalance (FiscalYearId, ChartOfAccountsId)
    WHERE IsDeleted = 0;
