-- ============================================================================
-- BillingGroupOpeningBalance
-- Bilancio apertura + chiusura per GRUPPO DI FATTURAZIONE per esercizio.
--
-- Analogo a UnitOpeningBalance ma a livello di BillingGroup: quando più unità
-- condividono lo stesso gruppo di fatturazione, il saldo iniziale/finale viene
-- gestito una sola volta sul gruppo (non più spalmato sulle singole unità).
--
-- Regole: identiche a UnitOpeningBalance (vedi add_unit_opening_balance.sql).
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BillingGroupOpeningBalance')
BEGIN
    CREATE TABLE BillingGroupOpeningBalance (
        Id                    INT              NOT NULL PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        BillingGroupId        INT              NOT NULL,
        FiscalYearId          INT              NOT NULL,
        OpeningBalance        DECIMAL(18,4)    NOT NULL DEFAULT 0,
        RateAddebitate        DECIMAL(18,4)    NOT NULL DEFAULT 0,
        RateIncassate         DECIMAL(18,4)    NOT NULL DEFAULT 0,
        QuotaConsuntiva       DECIMAL(18,4)    NOT NULL DEFAULT 0,
        SaldoConguaglio       DECIMAL(18,4)    NOT NULL DEFAULT 0,
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

        CONSTRAINT FK_BillingGroupOpeningBalance_Tenant       FOREIGN KEY (TenantId)       REFERENCES Tenant(Id),
        CONSTRAINT FK_BillingGroupOpeningBalance_BillingGroup FOREIGN KEY (BillingGroupId) REFERENCES BillingGroup(Id),
        CONSTRAINT FK_BillingGroupOpeningBalance_FiscalYear   FOREIGN KEY (FiscalYearId)   REFERENCES FiscalYear(FiscalYearId),

        -- Un solo record per gruppo + esercizio
        CONSTRAINT UQ_BillingGroupOpeningBalance_GroupFiscalYear UNIQUE (BillingGroupId, FiscalYearId)
    );
END
GO

-- Hilo sequence entry
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'BillingGroupOpeningBalance', 1
WHERE NOT EXISTS (
    SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'BillingGroupOpeningBalance'
);
GO
