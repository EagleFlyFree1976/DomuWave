-- ============================================================
-- Migration: Crea tabella CondominiumFee
-- Dipende da: CondominiumInstallment, RealEstateUnit, Tenant
-- ============================================================

CREATE TABLE CondominiumFee (
    Id                   BIGINT         NOT NULL,
    TenantId             UNIQUEIDENTIFIER NOT NULL,
    InstallmentId        INT            NOT NULL,
    UnitId               INT            NOT NULL,
    UserId               BIGINT         NOT NULL DEFAULT 0,
    AmountDue            DECIMAL(18,2)  NOT NULL DEFAULT 0,
    AmountPaid           DECIMAL(18,2)  NOT NULL DEFAULT 0,
    Balance              DECIMAL(18,2)  NOT NULL DEFAULT 0,
    PaymentStatus        NVARCHAR(50)   NOT NULL DEFAULT 'ToPay',
    PaymentDate          DATETIME2      NULL,
    PaymentMethod        NVARCHAR(50)   NULL,
    Description          NVARCHAR(500)  NULL,
    CreatedById          INT            NOT NULL DEFAULT 0,
    CreatedByFullName    NVARCHAR(200)  NULL,
    LastUpdatedById      INT            NULL,
    LastUpdatedByFullName NVARCHAR(200) NULL,
    IsDeleted            BIT            NOT NULL DEFAULT 0,
    CreationDate         DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    LastUpdateDate       DATETIME2      NULL,

    CONSTRAINT PK_CondominiumFee PRIMARY KEY (Id),
    CONSTRAINT FK_CondominiumFee_Installment FOREIGN KEY (InstallmentId) REFERENCES CondominiumInstallment(Id),
    CONSTRAINT FK_CondominiumFee_Unit        FOREIGN KEY (UnitId)        REFERENCES RealEstateUnit(Id)
);

-- Indici per le query più comuni
CREATE INDEX IX_CondominiumFee_InstallmentId  ON CondominiumFee (InstallmentId);
CREATE INDEX IX_CondominiumFee_UnitId         ON CondominiumFee (UnitId);
CREATE INDEX IX_CondominiumFee_UserId         ON CondominiumFee (UserId);
CREATE INDEX IX_CondominiumFee_TenantId       ON CondominiumFee (TenantId);
CREATE INDEX IX_CondominiumFee_PaymentStatus  ON CondominiumFee (PaymentStatus) WHERE IsDeleted = 0;

-- Riga di sequenza hilo per NHibernate
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'CondominiumFee', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'CondominiumFee');
