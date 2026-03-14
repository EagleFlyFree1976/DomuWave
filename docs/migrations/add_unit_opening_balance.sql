-- ============================================================
-- Migration: UnitOpeningBalance
-- Bilancio apertura + chiusura per unità immobiliare per esercizio.
--
-- Regole:
-- - OpeningBalance: modificabile manualmente solo sul primo esercizio.
--   Negli esercizi successivi viene propagato automaticamente dal
--   ClosingBalance dell'esercizio precedente (non modificabile).
-- - TotalMovements: calcolato alla chiusura (AmountDue - AmountPaid).
-- - ClosingBalance: OpeningBalance + TotalMovements (calcolato alla chiusura).
-- ============================================================

CREATE TABLE UnitOpeningBalance (
    Id                    INT              NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    UnitId                INT              NOT NULL,
    FiscalYearId          INT              NOT NULL,
    OpeningBalance        DECIMAL(18,4)    NOT NULL DEFAULT 0,
    TotalMovements        DECIMAL(18,4)    NOT NULL DEFAULT 0,
    ClosingBalance        DECIMAL(18,4)    NOT NULL DEFAULT 0,
    Notes                 NVARCHAR(1000)   NULL,

    -- Trace fields
    CreatedById           INT              NOT NULL,
    CreatedByFullName     NVARCHAR(200)    NULL,
    LastUpdatedById       INT              NULL,
    LastUpdatedByFullName NVARCHAR(200)    NULL,
    IsDeleted             BIT              NOT NULL DEFAULT 0,
    CreationDate          DATETIME2        NOT NULL,
    LastUpdateDate        DATETIME2        NULL,

    CONSTRAINT FK_UnitOpeningBalance_Tenant     FOREIGN KEY (TenantId)     REFERENCES Tenant(Id),
    CONSTRAINT FK_UnitOpeningBalance_Unit       FOREIGN KEY (UnitId)       REFERENCES RealEstateUnit(Id),
    CONSTRAINT FK_UnitOpeningBalance_FiscalYear FOREIGN KEY (FiscalYearId) REFERENCES FiscalYear(Id),

    -- Un solo record per unità + esercizio
    CONSTRAINT UQ_UnitOpeningBalance_UnitFiscalYear UNIQUE (UnitId, FiscalYearId)
);

-- Hilo sequence entry
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'UnitOpeningBalance', 1
WHERE NOT EXISTS (
    SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'UnitOpeningBalance'
);
