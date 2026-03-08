-- ============================================================
-- Script: ricrea tabella CondominiumInstallment
-- Include: FiscalYearId (FK), StatusId (FK lookup), Fees cascade
-- Dipende da: Tenant, Condominium, Budget, FiscalYear,
--             CondominiumInstallmentStatusLookup
-- ============================================================

-- ── 0. Rimuovi dipendenze ────────────────────────────────────

-- Prima rimuovi la tabella CondominiumFee (FK → CondominiumInstallment)
IF OBJECT_ID('CondominiumFee', 'U') IS NOT NULL
    DROP TABLE CondominiumFee;

-- Poi rimuovi la tabella principale
IF OBJECT_ID('CondominiumInstallment', 'U') IS NOT NULL
    DROP TABLE CondominiumInstallment;

-- ── 1. Crea (o ricrea) tabella lookup degli stati ───────────

IF OBJECT_ID('CondominiumInstallmentStatusLookup', 'U') IS NULL
BEGIN
    CREATE TABLE CondominiumInstallmentStatusLookup (
        Id   INT          NOT NULL,
        Name NVARCHAR(50) NOT NULL,
        CONSTRAINT PK_CondominiumInstallmentStatusLookup PRIMARY KEY (Id)
    );

    INSERT INTO CondominiumInstallmentStatusLookup (Id, Name) VALUES
        (1, 'Draft'),
        (2, 'Open'),
        (3, 'Paid'),
        (4, 'Overdue'),
        (5, 'Cancelled');
END;

-- ── 2. Crea tabella CondominiumInstallment ───────────────────

CREATE TABLE CondominiumInstallment (
    Id                    INT            NOT NULL,
    TenantId              INT            NOT NULL,
    CondominiumId         INT            NOT NULL,
    BudgetId              INT            NULL,
    FiscalYearId          INT            NULL,
    Description           NVARCHAR(200)  NOT NULL,   -- mappato come Name nel modello
    Notes                 NVARCHAR(500)  NULL,        -- mappato come Description e Notes nel modello
    InstallmentNumber     INT            NOT NULL,
    DueDate               DATETIME       NOT NULL,
    TotalAmount           DECIMAL(18,2)  NOT NULL,
    StatusId              INT            NOT NULL,
    CreatedById           INT            NOT NULL DEFAULT 0,
    CreatedByFullName     NVARCHAR(200)  NULL,
    LastUpdatedById       INT            NULL,
    LastUpdatedByFullName NVARCHAR(200)  NULL,
    IsDeleted             BIT            NOT NULL DEFAULT 0,
    CreationDate          DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    LastUpdateDate        DATETIME2      NULL,

    CONSTRAINT PK_CondominiumInstallment
        PRIMARY KEY (Id),
    CONSTRAINT FK_CondominiumInstallment_Tenant
        FOREIGN KEY (TenantId)     REFERENCES Tenant(Id),
    CONSTRAINT FK_CondominiumInstallment_Condominium
        FOREIGN KEY (CondominiumId) REFERENCES Condominium(Id),
    CONSTRAINT FK_CondominiumInstallment_Budget
        FOREIGN KEY (BudgetId)     REFERENCES Budget(Id),
    CONSTRAINT FK_CondominiumInstallment_FiscalYear
        FOREIGN KEY (FiscalYearId) REFERENCES FiscalYear(Id),
    CONSTRAINT FK_CondominiumInstallment_Status
        FOREIGN KEY (StatusId)     REFERENCES CondominiumInstallmentStatusLookup(Id)
);

-- ── 3. Indici ────────────────────────────────────────────────

CREATE INDEX IX_CondominiumInstallment_CondominiumId
    ON CondominiumInstallment (CondominiumId);

CREATE INDEX IX_CondominiumInstallment_FiscalYearId
    ON CondominiumInstallment (FiscalYearId);

CREATE INDEX IX_CondominiumInstallment_BudgetId
    ON CondominiumInstallment (BudgetId);

CREATE INDEX IX_CondominiumInstallment_StatusId
    ON CondominiumInstallment (StatusId) WHERE IsDeleted = 0;

-- ── 4. Riga hilo per NHibernate ──────────────────────────────

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'CondominiumInstallment', 1
WHERE NOT EXISTS (
    SELECT 1 FROM hibernate_unique_key
    WHERE entity_type = 'CondominiumInstallment'
);

-- ── 5. Ricrea tabella CondominiumFee ─────────────────────────

CREATE TABLE CondominiumFee (
    Id                    BIGINT           NOT NULL,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    InstallmentId         INT              NOT NULL,
    UnitId                INT              NOT NULL,
    UserId                BIGINT           NOT NULL DEFAULT 0,
    AmountDue             DECIMAL(18,2)    NOT NULL DEFAULT 0,
    AmountPaid            DECIMAL(18,2)    NOT NULL DEFAULT 0,
    Balance               DECIMAL(18,2)    NOT NULL DEFAULT 0,
    PaymentStatus         NVARCHAR(50)     NOT NULL DEFAULT 'ToPay',
    PaymentDate           DATETIME2        NULL,
    PaymentMethod         NVARCHAR(50)     NULL,
    Description           NVARCHAR(500)    NULL,
    CreatedById           INT              NOT NULL DEFAULT 0,
    CreatedByFullName     NVARCHAR(200)    NULL,
    LastUpdatedById       INT              NULL,
    LastUpdatedByFullName NVARCHAR(200)    NULL,
    IsDeleted             BIT              NOT NULL DEFAULT 0,
    CreationDate          DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    LastUpdateDate        DATETIME2        NULL,

    CONSTRAINT PK_CondominiumFee
        PRIMARY KEY (Id),
    CONSTRAINT FK_CondominiumFee_Installment
        FOREIGN KEY (InstallmentId) REFERENCES CondominiumInstallment(Id),
    CONSTRAINT FK_CondominiumFee_Unit
        FOREIGN KEY (UnitId)        REFERENCES RealEstateUnit(Id)
);

CREATE INDEX IX_CondominiumFee_InstallmentId
    ON CondominiumFee (InstallmentId);
CREATE INDEX IX_CondominiumFee_UnitId
    ON CondominiumFee (UnitId);
CREATE INDEX IX_CondominiumFee_UserId
    ON CondominiumFee (UserId);
CREATE INDEX IX_CondominiumFee_TenantId
    ON CondominiumFee (TenantId);
CREATE INDEX IX_CondominiumFee_PaymentStatus
    ON CondominiumFee (PaymentStatus) WHERE IsDeleted = 0;

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'CondominiumFee', 1
WHERE NOT EXISTS (
    SELECT 1 FROM hibernate_unique_key
    WHERE entity_type = 'CondominiumFee'
);
