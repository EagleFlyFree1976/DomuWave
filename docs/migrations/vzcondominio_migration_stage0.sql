-- ============================================================
-- Migration: VZCondominio -> DomuWave (Stage 0 Foundation)
-- Obiettivo:
-- 1) Preparare area di staging locale in DomuWave
-- 2) Definire mapping legacy->target per migrazione idempotente
-- 3) Abilitare controlli qualità dati prima del caricamento finale
--
-- NOTE:
-- - Script NON modifica tabelle di dominio esistenti.
-- - Script idempotente: può essere rilanciato senza effetti collaterali.
-- - Tutti i campi data sono DATETIME2 in linea con convenzioni DomuWave.
-- ============================================================

SET NOCOUNT ON;

-- ------------------------------------------------------------
-- 0) Schema di staging
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'stg')
    EXEC('CREATE SCHEMA stg AUTHORIZATION dbo');
GO

-- ------------------------------------------------------------
-- 1) Tracking esecuzioni migrazione
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.MigrationRun_VZ', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MigrationRun_VZ (
        Id                  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RunCode             NVARCHAR(100) NOT NULL,
        StartedAt           DATETIME2 NOT NULL,
        EndedAt             DATETIME2 NULL,
        Status              NVARCHAR(30) NOT NULL, -- Started, Completed, Failed
        SourceDatabase      NVARCHAR(128) NOT NULL DEFAULT 'vzcondominio',
        TargetDatabase      NVARCHAR(128) NOT NULL DEFAULT 'DomuWave',
        Notes               NVARCHAR(1000) NULL,
        CreatedBy           NVARCHAR(100) NOT NULL DEFAULT SUSER_SNAME()
    );

    CREATE UNIQUE INDEX UQ_MigrationRun_VZ_RunCode ON dbo.MigrationRun_VZ (RunCode);
END
GO

IF OBJECT_ID('dbo.MigrationTenantMap_VZ', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MigrationTenantMap_VZ (
        VZCondominioId      INT NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        CondominiumId       INT NULL,
        IsActive            BIT NOT NULL DEFAULT 1,
        CreatedAt           DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        UpdatedAt           DATETIME2 NULL,
        Notes               NVARCHAR(1000) NULL
    );
END
GO

-- ------------------------------------------------------------
-- 2) Staging raw estratti da VZCondominio
-- ------------------------------------------------------------
IF OBJECT_ID('stg.VZ_Condominium', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_Condominium (
        VZId                INT NOT NULL PRIMARY KEY,
        Name                NVARCHAR(255) NOT NULL,
        TaxCode             NVARCHAR(50) NULL,
        IbanRaw             NVARCHAR(500) NULL,
        Notes               NVARCHAR(MAX) NULL,
        SourceExtractedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow             VARBINARY(32) NULL
    );
END
GO

IF OBJECT_ID('stg.VZ_Building', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_Building (
        VZId                INT NOT NULL PRIMARY KEY,
        VZCondominiumId     INT NULL,
        Name                NVARCHAR(255) NOT NULL,
        Street              NVARCHAR(255) NULL,
        City                NVARCHAR(100) NULL,
        Province            NVARCHAR(50) NULL,
        PostalCode          NVARCHAR(20) NULL,
        Country             NVARCHAR(100) NULL,
        Floors              INT NULL,
        Notes               NVARCHAR(MAX) NULL,
        SourceExtractedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow             VARBINARY(32) NULL
    );

    CREATE INDEX IX_stg_VZ_Building_CondominiumId ON stg.VZ_Building (VZCondominiumId);
END
GO

IF OBJECT_ID('stg.VZ_Staircase', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_Staircase (
        VZId                INT NOT NULL PRIMARY KEY,
        VZBuildingId        INT NULL,
        Name                NVARCHAR(255) NOT NULL,
        StreetNumber        NVARCHAR(50) NULL,
        SourceExtractedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow             VARBINARY(32) NULL
    );

    CREATE INDEX IX_stg_VZ_Staircase_BuildingId ON stg.VZ_Staircase (VZBuildingId);
END
GO

IF OBJECT_ID('stg.VZ_Unit', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_Unit (
        VZId                INT NOT NULL PRIMARY KEY,
        VZBuildingId        INT NOT NULL,
        VZStaircaseId       INT NOT NULL,
        Description         NVARCHAR(255) NOT NULL,
        FloorNumber         INT NULL,
        CadastralSheet      INT NULL,
        CadastralParcel     INT NULL,
        CadastralSub        INT NULL,
        MillesimalGeneral   DECIMAL(18,4) NULL,
        MillesimalStaircase DECIMAL(18,4) NULL,
        MillesimalLift      DECIMAL(18,4) NULL,
        Notes               NVARCHAR(MAX) NULL,
        SourceExtractedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow             VARBINARY(32) NULL
    );

    CREATE INDEX IX_stg_VZ_Unit_BuildingId ON stg.VZ_Unit (VZBuildingId);
    CREATE INDEX IX_stg_VZ_Unit_StaircaseId ON stg.VZ_Unit (VZStaircaseId);
END
GO

IF OBJECT_ID('stg.VZ_Person', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_Person (
        VZId                INT NOT NULL PRIMARY KEY,
        FirstName           NVARCHAR(150) NULL,
        LastName            NVARCHAR(150) NULL,
        CompanyName         NVARCHAR(255) NULL,
        TaxCode             NVARCHAR(50) NULL,
        VatNumber           NVARCHAR(50) NULL,
        Phone               NVARCHAR(50) NULL,
        Mobile              NVARCHAR(50) NULL,
        Email               NVARCHAR(255) NULL,
        Pec                 NVARCHAR(255) NULL,
        Address             NVARCHAR(255) NULL,
        City                NVARCHAR(100) NULL,
        Province            NVARCHAR(50) NULL,
        PostalCode          NVARCHAR(20) NULL,
        Country             NVARCHAR(100) NULL,
        Notes               NVARCHAR(MAX) NULL,
        SourceExtractedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow             VARBINARY(32) NULL
    );
END
GO

IF OBJECT_ID('stg.VZ_Supplier', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_Supplier (
        VZId                INT NOT NULL PRIMARY KEY,
        FirstName           NVARCHAR(150) NULL,
        LastName            NVARCHAR(150) NULL,
        CompanyName         NVARCHAR(255) NULL,
        TaxCode             NVARCHAR(50) NULL,
        VatNumber           NVARCHAR(50) NULL,
        Phone               NVARCHAR(50) NULL,
        Mobile              NVARCHAR(50) NULL,
        Email               NVARCHAR(255) NULL,
        Pec                 NVARCHAR(255) NULL,
        Address             NVARCHAR(255) NULL,
        City                NVARCHAR(100) NULL,
        Province            NVARCHAR(50) NULL,
        PostalCode          NVARCHAR(20) NULL,
        Country             NVARCHAR(100) NULL,
        Notes               NVARCHAR(MAX) NULL,
        SourceExtractedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow             VARBINARY(32) NULL
    );
END
GO

IF OBJECT_ID('stg.VZ_FiscalYear', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_FiscalYear (
        VZId                INT NOT NULL PRIMARY KEY,
        VZCondominiumId     INT NULL,
        Description         NVARCHAR(255) NOT NULL,
        StartDate           DATETIME2 NOT NULL,
        EndDate             DATETIME2 NOT NULL,
        IsExtraordinary     BIT NULL,
        IsClosed            BIT NULL,
        SourceExtractedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow             VARBINARY(32) NULL
    );

    CREATE INDEX IX_stg_VZ_FiscalYear_CondominiumId ON stg.VZ_FiscalYear (VZCondominiumId);
END
GO

IF OBJECT_ID('stg.VZ_Expense', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_Expense (
        VZId                INT NOT NULL PRIMARY KEY,
        VZBuildingId        INT NOT NULL,
        VZStaircaseId       INT NULL,
        VZUnitId            INT NULL,
        VZPersonId          INT NULL,
        VZMacroCategoryId   INT NOT NULL,
        VZCategoryId        INT NOT NULL,
        VZFiscalYearId      INT NOT NULL,
        ExpenseDate         DATE NOT NULL,
        Description         NVARCHAR(500) NOT NULL,
        Amount              DECIMAL(18,4) NOT NULL,
        Notes               NVARCHAR(MAX) NULL,
        VZInvoiceId         INT NULL,
        Is770               BIT NOT NULL,
        SourceExtractedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow             VARBINARY(32) NULL
    );

    CREATE INDEX IX_stg_VZ_Expense_FiscalYearId ON stg.VZ_Expense (VZFiscalYearId);
    CREATE INDEX IX_stg_VZ_Expense_InvoiceId ON stg.VZ_Expense (VZInvoiceId);
END
GO

IF OBJECT_ID('stg.VZ_Invoice', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_Invoice (
        VZId                INT NOT NULL PRIMARY KEY,
        Number              NVARCHAR(100) NOT NULL,
        Protocol            NVARCHAR(100) NULL,
        IssueDate           DATE NOT NULL,
        DueDate             DATE NOT NULL,
        PaymentDate         DATE NULL,
        TaxableAmount       DECIMAL(18,4) NOT NULL,
        VatAmount           DECIMAL(18,4) NULL,
        WithholdingAmount   DECIMAL(18,4) NULL,
        PensionFundAmount   DECIMAL(18,4) NULL,
        StampDutyAmount     DECIMAL(18,4) NULL,
        TotalAmount         DECIMAL(18,4) NOT NULL,
        Base64Document      NVARCHAR(MAX) NULL,
        Notes               NVARCHAR(MAX) NULL,
        VZSupplierId        INT NULL,
        IsVatExempt         BIT NULL,
        TaxCode             NVARCHAR(50) NULL,
        NonTaxableAmount    DECIMAL(18,4) NULL,
        SourceExtractedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow             VARBINARY(32) NULL
    );

    CREATE INDEX IX_stg_VZ_Invoice_SupplierId ON stg.VZ_Invoice (VZSupplierId);
END
GO

IF OBJECT_ID('stg.VZ_Payment', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_Payment (
        VZId                INT NOT NULL PRIMARY KEY,
        VZPersonId          INT NULL,
        VZFiscalYearId      INT NULL,
        Description         NVARCHAR(500) NOT NULL,
        PaymentDate         DATE NOT NULL,
        Amount              DECIMAL(18,4) NOT NULL,
        Channel             NVARCHAR(100) NULL,
        SourceExtractedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow             VARBINARY(32) NULL
    );

    CREATE INDEX IX_stg_VZ_Payment_FiscalYearId ON stg.VZ_Payment (VZFiscalYearId);
END
GO

IF OBJECT_ID('stg.VZ_Budget', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_Budget (
        VZFiscalYearId      INT NOT NULL,
        VZMacroCategoryId   INT NOT NULL,
        VZCategoryId        INT NOT NULL,
        VZBuildingId        INT NOT NULL,
        VZStaircaseId       INT NOT NULL,
        Amount              DECIMAL(18,4) NOT NULL,
        SourceExtractedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow             VARBINARY(32) NULL,
        CONSTRAINT PK_stg_VZ_Budget PRIMARY KEY (VZFiscalYearId, VZMacroCategoryId, VZCategoryId, VZBuildingId, VZStaircaseId)
    );
END
GO

IF OBJECT_ID('stg.VZ_ExpenseMacroCategory', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_ExpenseMacroCategory (
        VZMacroCategoryId    INT NOT NULL PRIMARY KEY,
        Name                 NVARCHAR(255) NOT NULL,
        SourceExtractedAt    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow              VARBINARY(32) NULL
    );
END
GO

IF OBJECT_ID('stg.VZ_ExpenseCategory', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_ExpenseCategory (
        VZCategoryId         INT NOT NULL PRIMARY KEY,
        VZMacroCategoryId    INT NOT NULL,
        Name                 NVARCHAR(255) NOT NULL,
        SourceExtractedAt    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow              VARBINARY(32) NULL
    );

    CREATE INDEX IX_stg_VZ_ExpenseCategory_Macro ON stg.VZ_ExpenseCategory (VZMacroCategoryId);
END
GO

IF OBJECT_ID('stg.VZ_BudgetInstallment', 'U') IS NULL
BEGIN
    CREATE TABLE stg.VZ_BudgetInstallment (
        VZFiscalYearId      INT NOT NULL,
        DueDate             DATE NOT NULL,
        Description         NVARCHAR(255) NOT NULL,
        SourceExtractedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        HashRow             VARBINARY(32) NULL,
        CONSTRAINT PK_stg_VZ_BudgetInstallment PRIMARY KEY (VZFiscalYearId, DueDate, Description)
    );
END
GO

-- ------------------------------------------------------------
-- 3) Mapping legacy -> DomuWave (id target)
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.Map_VZ_Condominium', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Map_VZ_Condominium (
        VZId                INT NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        DomuWaveId          INT NOT NULL,
        MigratedAt          DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        RunCode             NVARCHAR(100) NULL
    );

    CREATE UNIQUE INDEX UQ_Map_VZ_Condominium_Target ON dbo.Map_VZ_Condominium (TenantId, DomuWaveId);
END
GO

IF OBJECT_ID('dbo.Map_VZ_Unit', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Map_VZ_Unit (
        VZId                INT NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        DomuWaveId          INT NOT NULL,
        MigratedAt          DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        RunCode             NVARCHAR(100) NULL
    );

    CREATE UNIQUE INDEX UQ_Map_VZ_Unit_Target ON dbo.Map_VZ_Unit (TenantId, DomuWaveId);
END
GO

IF OBJECT_ID('dbo.Map_VZ_PersonToUser', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Map_VZ_PersonToUser (
        VZId                INT NOT NULL,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        UserId              INT NOT NULL,
        RoleHint            NVARCHAR(30) NULL, -- Owner, Tenant, Generic
        MigratedAt          DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        RunCode             NVARCHAR(100) NULL,
        CONSTRAINT PK_Map_VZ_PersonToUser PRIMARY KEY (VZId, TenantId, UserId)
    );
END
GO

IF OBJECT_ID('dbo.Map_VZ_Supplier', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Map_VZ_Supplier (
        VZId                INT NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        DomuWaveId          INT NOT NULL,
        MigratedAt          DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        RunCode             NVARCHAR(100) NULL
    );

    CREATE UNIQUE INDEX UQ_Map_VZ_Supplier_Target ON dbo.Map_VZ_Supplier (TenantId, DomuWaveId);
END
GO

IF OBJECT_ID('dbo.Map_VZ_FiscalYear', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Map_VZ_FiscalYear (
        VZId                INT NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        DomuWaveId          INT NOT NULL,
        MigratedAt          DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        RunCode             NVARCHAR(100) NULL
    );

    CREATE UNIQUE INDEX UQ_Map_VZ_FiscalYear_Target ON dbo.Map_VZ_FiscalYear (TenantId, DomuWaveId);
END
GO

IF OBJECT_ID('dbo.Map_VZ_Expense', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Map_VZ_Expense (
        VZId                INT NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        DomuWaveId          INT NOT NULL,
        MigratedAt          DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        RunCode             NVARCHAR(100) NULL
    );

    CREATE UNIQUE INDEX UQ_Map_VZ_Expense_Target ON dbo.Map_VZ_Expense (TenantId, DomuWaveId);
END
GO

-- ------------------------------------------------------------
-- 4) Query di controllo qualità (eseguire dopo load staging)
-- ------------------------------------------------------------
-- 4.1 Unità senza condominio/edificio coerente
SELECT u.VZId, u.VZBuildingId
FROM stg.VZ_Unit u
LEFT JOIN stg.VZ_Building b ON b.VZId = u.VZBuildingId
WHERE b.VZId IS NULL;

-- 4.2 Esercizi senza condominio mappato a tenant
SELECT fy.VZId, fy.VZCondominiumId
FROM stg.VZ_FiscalYear fy
LEFT JOIN dbo.MigrationTenantMap_VZ tm ON tm.VZCondominioId = fy.VZCondominiumId AND tm.IsActive = 1
WHERE tm.VZCondominioId IS NULL;

-- 4.3 Spese orfane (anno assente)
SELECT e.VZId, e.VZFiscalYearId
FROM stg.VZ_Expense e
LEFT JOIN stg.VZ_FiscalYear fy ON fy.VZId = e.VZFiscalYearId
WHERE fy.VZId IS NULL;

-- 4.4 Fatture con fornitore assente in staging
SELECT i.VZId, i.VZSupplierId
FROM stg.VZ_Invoice i
LEFT JOIN stg.VZ_Supplier s ON s.VZId = i.VZSupplierId
WHERE i.VZSupplierId IS NOT NULL
  AND s.VZId IS NULL;

-- 4.5 Controllo formale IBAN (lunghezza minima)
SELECT c.VZId, c.IbanRaw
FROM stg.VZ_Condominium c
WHERE c.IbanRaw IS NOT NULL
  AND LEN(REPLACE(c.IbanRaw, ' ', '')) < 15;
GO

-- ------------------------------------------------------------
-- 5) Checklist operativa stage successivo
-- ------------------------------------------------------------
-- [ ] Popolare stg.* da vzcondominio con query cross-database o ETL esterno
-- [ ] Popolare MigrationTenantMap_VZ (VZCondominioId -> TenantId)
-- [ ] Definire utente tecnico migrazione (CreatedById) in DomuWave
-- [ ] Eseguire query di controllo qualità e risolvere anomalie
-- [ ] Procedere con Stage 1 (Master Data): Condominium, Address, Unit, Supplier
-- ------------------------------------------------------------
