-- ============================================================
-- Migration: Consumi — letture contatori e ripartizione spese
--
-- Entità:
--   ConsumptionType        — tipo di consumo (acqua, gas, ecc.) per condominio
--   Meter                  — contatore associato a un'unità immobiliare
--   ConsumptionReading     — lettura iniziale/finale per contatore + esercizio
--   ConsumptionCharge      — ripartizione spesa per tipo consumo + esercizio
--   ConsumptionChargeItem  — riga ripartizione per unità (quota calcolata)
--   ConsumptionChargeStatusLookup — stati ripartizione (Draft, Approved)
-- ============================================================

-- ── Lookup stato ripartizione ─────────────────────────────────────────────

CREATE TABLE ConsumptionChargeStatusLookup (
    Id   INT          NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);

INSERT INTO ConsumptionChargeStatusLookup (Id, Name) VALUES
    (1, 'Draft'),
    (2, 'Approved');

-- ── ConsumptionType ───────────────────────────────────────────────────────

CREATE TABLE ConsumptionType (
    Id                    INT              NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    CondominiumId         INT              NOT NULL,
    Name                  NVARCHAR(200)    NOT NULL,
    UnitOfMeasure         NVARCHAR(50)     NOT NULL,
    Notes                 NVARCHAR(1000)   NULL,
    IsActive              BIT              NOT NULL DEFAULT 1,

    CreatedById           INT              NOT NULL,
    CreatedByFullName     NVARCHAR(200)    NULL,
    LastUpdatedById       INT              NULL,
    LastUpdatedByFullName NVARCHAR(200)    NULL,
    IsDeleted             BIT              NOT NULL DEFAULT 0,
    CreationDate          DATETIME2        NOT NULL,
    LastUpdateDate        DATETIME2        NULL,

    CONSTRAINT FK_ConsumptionType_Tenant      FOREIGN KEY (TenantId)      REFERENCES Tenant(Id),
    CONSTRAINT FK_ConsumptionType_Condominium FOREIGN KEY (CondominiumId) REFERENCES Condominium(Id)
);

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'ConsumptionType', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'ConsumptionType');

-- ── Meter ─────────────────────────────────────────────────────────────────

CREATE TABLE Meter (
    Id                    INT              NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    ConsumptionTypeId     INT              NOT NULL,
    RealEstateUnitId      INT              NOT NULL,
    Code                  NVARCHAR(100)    NOT NULL,
    Notes                 NVARCHAR(1000)   NULL,
    IsActive              BIT              NOT NULL DEFAULT 1,

    CreatedById           INT              NOT NULL,
    CreatedByFullName     NVARCHAR(200)    NULL,
    LastUpdatedById       INT              NULL,
    LastUpdatedByFullName NVARCHAR(200)    NULL,
    IsDeleted             BIT              NOT NULL DEFAULT 0,
    CreationDate          DATETIME2        NOT NULL,
    LastUpdateDate        DATETIME2        NULL,

    CONSTRAINT FK_Meter_Tenant          FOREIGN KEY (TenantId)          REFERENCES Tenant(Id),
    CONSTRAINT FK_Meter_ConsumptionType FOREIGN KEY (ConsumptionTypeId) REFERENCES ConsumptionType(Id),
    CONSTRAINT FK_Meter_RealEstateUnit  FOREIGN KEY (RealEstateUnitId)  REFERENCES RealEstateUnit(Id)
);

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'Meter', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'Meter');

-- ── ConsumptionReading ────────────────────────────────────────────────────

CREATE TABLE ConsumptionReading (
    Id                    INT              NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    MeterId               INT              NOT NULL,
    FiscalYearId          INT              NOT NULL,
    InitialDate           DATETIME2        NULL,
    InitialValue          DECIMAL(18,4)    NOT NULL DEFAULT 0,
    FinalDate             DATETIME2        NULL,
    FinalValue            DECIMAL(18,4)    NOT NULL DEFAULT 0,
    Consumption           AS (FinalValue - InitialValue) PERSISTED,
    Notes                 NVARCHAR(1000)   NULL,

    CreatedById           INT              NOT NULL,
    CreatedByFullName     NVARCHAR(200)    NULL,
    LastUpdatedById       INT              NULL,
    LastUpdatedByFullName NVARCHAR(200)    NULL,
    IsDeleted             BIT              NOT NULL DEFAULT 0,
    CreationDate          DATETIME2        NOT NULL,
    LastUpdateDate        DATETIME2        NULL,

    CONSTRAINT FK_ConsumptionReading_Tenant     FOREIGN KEY (TenantId)     REFERENCES Tenant(Id),
    CONSTRAINT FK_ConsumptionReading_Meter      FOREIGN KEY (MeterId)      REFERENCES Meter(Id),
    CONSTRAINT FK_ConsumptionReading_FiscalYear FOREIGN KEY (FiscalYearId) REFERENCES FiscalYear(Id)
);

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'ConsumptionReading', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'ConsumptionReading');

-- ── ConsumptionCharge ─────────────────────────────────────────────────────

CREATE TABLE ConsumptionCharge (
    Id                    INT              NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    ConsumptionTypeId     INT              NOT NULL,
    FiscalYearId          INT              NOT NULL,
    BudgetId              INT              NOT NULL,
    ExpenseId             INT              NULL,
    TotalAmount           DECIMAL(18,4)    NOT NULL DEFAULT 0,
    StatusId              INT              NOT NULL DEFAULT 1,
    Notes                 NVARCHAR(1000)   NULL,

    CreatedById           INT              NOT NULL,
    CreatedByFullName     NVARCHAR(200)    NULL,
    LastUpdatedById       INT              NULL,
    LastUpdatedByFullName NVARCHAR(200)    NULL,
    IsDeleted             BIT              NOT NULL DEFAULT 0,
    CreationDate          DATETIME2        NOT NULL,
    LastUpdateDate        DATETIME2        NULL,

    CONSTRAINT FK_ConsumptionCharge_Tenant          FOREIGN KEY (TenantId)          REFERENCES Tenant(Id),
    CONSTRAINT FK_ConsumptionCharge_ConsumptionType FOREIGN KEY (ConsumptionTypeId) REFERENCES ConsumptionType(Id),
    CONSTRAINT FK_ConsumptionCharge_FiscalYear      FOREIGN KEY (FiscalYearId)      REFERENCES FiscalYear(Id),
    CONSTRAINT FK_ConsumptionCharge_Budget          FOREIGN KEY (BudgetId)          REFERENCES Budget(Id),
    CONSTRAINT FK_ConsumptionCharge_Expense         FOREIGN KEY (ExpenseId)         REFERENCES Expense(Id),
    CONSTRAINT FK_ConsumptionCharge_Status          FOREIGN KEY (StatusId)          REFERENCES ConsumptionChargeStatusLookup(Id)
);

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'ConsumptionCharge', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'ConsumptionCharge');

-- ── ConsumptionChargeItem ─────────────────────────────────────────────────

CREATE TABLE ConsumptionChargeItem (
    Id                    INT              NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    ConsumptionChargeId   INT              NOT NULL,
    RealEstateUnitId      INT              NOT NULL,
    Consumption           DECIMAL(18,4)    NOT NULL DEFAULT 0,
    TotalConsumption      DECIMAL(18,4)    NOT NULL DEFAULT 0,
    Percentage            DECIMAL(10,6)    NOT NULL DEFAULT 0,
    Amount                DECIMAL(18,4)    NOT NULL DEFAULT 0,
    HasWarning            BIT              NOT NULL DEFAULT 0,

    CreatedById           INT              NOT NULL,
    CreatedByFullName     NVARCHAR(200)    NULL,
    LastUpdatedById       INT              NULL,
    LastUpdatedByFullName NVARCHAR(200)    NULL,
    IsDeleted             BIT              NOT NULL DEFAULT 0,
    CreationDate          DATETIME2        NOT NULL,
    LastUpdateDate        DATETIME2        NULL,

    CONSTRAINT FK_ConsumptionChargeItem_Tenant           FOREIGN KEY (TenantId)           REFERENCES Tenant(Id),
    CONSTRAINT FK_ConsumptionChargeItem_Charge           FOREIGN KEY (ConsumptionChargeId) REFERENCES ConsumptionCharge(Id),
    CONSTRAINT FK_ConsumptionChargeItem_RealEstateUnit   FOREIGN KEY (RealEstateUnitId)   REFERENCES RealEstateUnit(Id),

    CONSTRAINT UQ_ConsumptionChargeItem_ChargeUnit UNIQUE (ConsumptionChargeId, RealEstateUnitId)
);

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'ConsumptionChargeItem', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'ConsumptionChargeItem');
