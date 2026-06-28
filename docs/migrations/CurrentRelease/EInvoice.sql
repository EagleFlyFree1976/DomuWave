-- ============================================================================
-- Fatture Elettroniche (passive) — download massivo dal Cassetto Fiscale / SdI
-- tramite provider accreditato con API. Configurazione PER CONDOMINIO
-- (stesso modello di Stripe Connect).
--
--   Condominium.EInvoiceProviderId   -> provider SdI usato (FK lookup; NULL = non configurato)
--   Condominium.EInvoiceApiKey       -> chiave/token API del provider (CIFRATA lato app, mai in chiaro)
--   Condominium.EInvoiceVatNumber    -> P.IVA del condominio su cui filtrare le fatture ricevute
--   Condominium.EInvoiceLastSyncDate -> data dell'ultimo download andato a buon fine
--
--   ElectronicInvoice                -> singola fattura passiva scaricata (XML grezzo + dati estratti)
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ============================================================================
-- 1. Lookup: provider SdI
-- ============================================================================
IF OBJECT_ID('EInvoiceProviderLookup') IS NULL
BEGIN
    CREATE TABLE EInvoiceProviderLookup (
        Id   INT          NOT NULL PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL
    );

    INSERT INTO EInvoiceProviderLookup (Id, Name) VALUES
        (0, 'None'),
        (1, 'Acube'),
        (2, 'Aruba'),
        (3, 'FattureInCloud');
END
GO

-- ============================================================================
-- 2. Lookup: stato della fattura scaricata
--    New     = scaricata, non ancora collegata a una spesa
--    Linked  = collegata a una Expense
--    Ignored = scartata manualmente dall'amministratore
-- ============================================================================
IF OBJECT_ID('ElectronicInvoiceStatusLookup') IS NULL
BEGIN
    CREATE TABLE ElectronicInvoiceStatusLookup (
        Id   INT          NOT NULL PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL
    );

    INSERT INTO ElectronicInvoiceStatusLookup (Id, Name) VALUES
        (0, 'New'),
        (1, 'Linked'),
        (2, 'Ignored');
END
GO

-- ============================================================================
-- 3. Colonne di configurazione su Condominium
-- ============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Condominium') AND name = 'EInvoiceProviderId'
)
BEGIN
    ALTER TABLE Condominium ADD EInvoiceProviderId INT NULL;
    ALTER TABLE Condominium ADD CONSTRAINT FK_Condominium_EInvoiceProviderLookup
        FOREIGN KEY (EInvoiceProviderId) REFERENCES EInvoiceProviderLookup(Id);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Condominium') AND name = 'EInvoiceApiKey'
)
BEGIN
    -- Valore cifrato a riposo lato applicazione (DataProtection). Mai in chiaro.
    ALTER TABLE Condominium ADD EInvoiceApiKey NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Condominium') AND name = 'EInvoiceVatNumber'
)
BEGIN
    ALTER TABLE Condominium ADD EInvoiceVatNumber NVARCHAR(16) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Condominium') AND name = 'EInvoiceLastSyncDate'
)
BEGIN
    ALTER TABLE Condominium ADD EInvoiceLastSyncDate DATETIME2 NULL;
END
GO

-- ============================================================================
-- 4. Tabella ElectronicInvoice
-- ============================================================================
IF OBJECT_ID('ElectronicInvoice') IS NULL
BEGIN
    CREATE TABLE ElectronicInvoice (
        Id              INT                NOT NULL PRIMARY KEY,
        TenantId        uniqueidentifier   NOT NULL,
        CondominiumId   INT                NOT NULL,
        SupplierId      INT                NULL,            -- match automatico per P.IVA
        ExpenseId       INT                NULL,            -- collegamento alla spesa generata
        StatusId        INT                NOT NULL DEFAULT 0,

        -- Identificativo univoco dello SdI: usato per la deduplica in fase di download.
        SdiIdentifier   NVARCHAR(50)       NOT NULL,

        -- Dati estratti dal file FatturaPA 1.2.1
        InvoiceNumber   NVARCHAR(50)       NOT NULL,
        InvoiceDate     DATETIME2          NOT NULL,
        SupplierVat     NVARCHAR(16)       NULL,
        SupplierTaxCode NVARCHAR(16)       NULL,
        SupplierName    NVARCHAR(200)      NULL,
        TotalAmount     DECIMAL(18,4)      NOT NULL DEFAULT 0,

        -- File XML grezzo (per conservazione / ricostruzione / download)
        XmlContent      NVARCHAR(MAX)      NOT NULL,

        -- ── Trace / Audit (standard di progetto) ──────────────────────────────
        CreatedById           INT           NOT NULL,
        CreatedByFullName     NVARCHAR(200) NULL,
        LastUpdatedById       INT           NULL,
        LastUpdatedByFullName NVARCHAR(200) NULL,
        IsDeleted             BIT           NOT NULL DEFAULT 0,
        CreationDate          DATETIME2     NOT NULL,
        LastUpdateDate        DATETIME2     NULL,

        CONSTRAINT FK_ElectronicInvoice_Condominium
            FOREIGN KEY (CondominiumId) REFERENCES Condominium(Id),
        CONSTRAINT FK_ElectronicInvoice_ElectronicInvoiceStatusLookup
            FOREIGN KEY (StatusId) REFERENCES ElectronicInvoiceStatusLookup(Id)
    );

    -- Deduplica: una stessa fattura SdI non va importata due volte per lo stesso condominio.
    CREATE UNIQUE INDEX UQ_ElectronicInvoice_Condominium_SdiIdentifier
        ON ElectronicInvoice (CondominiumId, SdiIdentifier)
        WHERE IsDeleted = 0;
END
GO

-- ============================================================================
-- 5. Hilo sequence entry per ElectronicInvoice
-- ============================================================================
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'ElectronicInvoice', 1
WHERE NOT EXISTS (
    SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'ElectronicInvoice'
);
GO

-- ============================================================================
-- 6. Voce di menu "Fatture Elettroniche"
--    Sotto "Amministrazione" (MenuId = 6). Idempotente.
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM base_menues WHERE Action = '/fatture-elettroniche')
BEGIN
    DECLARE @NewMenuId INT = (SELECT ISNULL(MAX(MenuId), 0) + 1 FROM base_menues);

    INSERT INTO base_menues (MenuId, ParentMenuId, Icon, Description, Action, AuthorizationCode, PopulateEvent, IsEnabled, OrderKey, Tags)
    VALUES (@NewMenuId, 6, 'pi-file-import', 'Fatture Elettroniche', '/fatture-elettroniche', NULL, NULL, 1, 24, NULL);
END
GO
