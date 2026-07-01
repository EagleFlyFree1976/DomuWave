-- ############################################################################
-- CURRENT RELEASE — Script di migrazione unificato
-- Raccoglie tutte le migrazioni della release corrente in un unico file.
-- Tutti gli step sono idempotenti e possono essere rieseguiti in sicurezza.
--
-- Contenuto:
--   1) TenantDisplaySettings  — nuova tabella impostazioni visualizzazione
--   2) UnitOwner.Phone        — nuova colonna telefono
--   3) Expense.Send770        — nuovo flag "Invia 770"
--   4) Expense                — date opzionali, tabella millesimale opzionale, UnitId
--   5) Voci di menu           — impostazioni, report spese, bilancio ripartizione
--   6) ConsumptionCharge.IsManual — modifica manuale importi ripartizione consumi
--   7) ChartOfAccounts.IsLiquidity — distingue liquidità vs fondi (Situazione Patrimoniale)
-- ############################################################################

-- Richiesto per la creazione/uso di indici filtrati e computed.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ============================================================================
-- 1) TenantDisplaySettings
-- Impostazioni di visualizzazione per tenant (formattazione valori contabili).
-- Prima opzione configurabile: convenzione del segno per le voci contabili.
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TenantDisplaySettings')
BEGIN
    CREATE TABLE TenantDisplaySettings (
        Id                       INT              NOT NULL PRIMARY KEY,
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        Name                     NVARCHAR(200)    NOT NULL,
        Description              NVARCHAR(MAX)    NULL,

        -- Convenzione segno per le voci contabili:
        --   0 = SoloColore     (magnitudine + colore, comportamento storico)
        --   1 = SegnoEsplicito (uscite con -, entrate con +)
        AccountingSignConvention INT              NOT NULL DEFAULT 0,

        -- Trace / audit
        CreatedById              INT              NOT NULL,
        CreatedByFullName        NVARCHAR(200)    NULL,
        LastUpdatedById          INT              NULL,
        LastUpdatedByFullName    NVARCHAR(200)    NULL,
        IsDeleted                BIT              NOT NULL DEFAULT 0,
        CreationDate             DATETIME2        NOT NULL,
        LastUpdateDate           DATETIME2        NULL,

        CONSTRAINT FK_TenantDisplaySettings_Tenant
            FOREIGN KEY (TenantId) REFERENCES Tenant(Id)
    );

    -- Un solo record di impostazioni per tenant
    CREATE UNIQUE INDEX UQ_TenantDisplaySettings_TenantId
        ON TenantDisplaySettings (TenantId)
        WHERE IsDeleted = 0;
END
GO

-- Sequenza hilo per la generazione degli Id
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'TenantDisplaySettings', 1
WHERE NOT EXISTS (
    SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'TenantDisplaySettings'
);
GO

-- ============================================================================
-- 2) UnitOwner: aggiunta colonna Phone (numero di telefono del proprietario/condomino)
-- Allineata alla convenzione esistente (UnitTenant.Phone NVARCHAR(20) NULL).
-- ============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('UnitOwner') AND name = 'Phone'
)
BEGIN
    ALTER TABLE UnitOwner ADD Phone NVARCHAR(20) NULL;
END
GO

-- ============================================================================
-- 3) Expense: aggiunta flag Send770 ("Invia 770")
-- Indica se la spesa deve essere inclusa nella certificazione/modello 770.
-- ============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Expense') AND name = 'Send770'
)
BEGIN
    ALTER TABLE Expense ADD Send770 BIT NOT NULL DEFAULT 0;
END
GO

-- ============================================================================
-- 4) Expense:
--   a) DocumentDate e RegistrationDate diventano opzionali (NULL)
--   b) MillesimalTableId diventa opzionale (NULL)
--   c) Nuova colonna UnitId: imputazione della spesa a un singolo immobile
--      (alternativa esclusiva alla tabella millesimale)
-- ============================================================================

-- a) Date opzionali
-- DocumentDate è referenziata da indici filtrati: vanno droppati, alterata la colonna, ricreati.
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Expense_DocumentDate' AND object_id=OBJECT_ID('Expense'))
    DROP INDEX IX_Expense_DocumentDate ON Expense;
GO
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Expense_Condominium_Year' AND object_id=OBJECT_ID('Expense'))
    DROP INDEX IX_Expense_Condominium_Year ON Expense;
GO

ALTER TABLE Expense ALTER COLUMN DocumentDate     DATETIME2 NULL;
GO
ALTER TABLE Expense ALTER COLUMN RegistrationDate DATETIME2 NULL;
GO

-- Ricrea gli indici filtrati identici
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Expense_DocumentDate' AND object_id=OBJECT_ID('Expense'))
    CREATE NONCLUSTERED INDEX IX_Expense_DocumentDate ON Expense (DocumentDate) WHERE IsDeleted = 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Expense_Condominium_Year' AND object_id=OBJECT_ID('Expense'))
    CREATE NONCLUSTERED INDEX IX_Expense_Condominium_Year ON Expense (CondominiumId, DocumentDate)
        INCLUDE (NetAmount, ExpenseTypeId) WHERE IsDeleted = 0;
GO

-- b) Tabella millesimale opzionale
ALTER TABLE Expense ALTER COLUMN MillesimalTableId INT NULL;
GO

-- c) Colonna UnitId + FK verso RealEstateUnit
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Expense') AND name = 'UnitId'
)
BEGIN
    ALTER TABLE Expense ADD UnitId INT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Expense_RealEstateUnit'
)
BEGIN
    ALTER TABLE Expense ADD CONSTRAINT FK_Expense_RealEstateUnit
        FOREIGN KEY (UnitId) REFERENCES RealEstateUnit(Id);
END
GO

-- ============================================================================
-- 5) Voci di menu
-- ============================================================================

-- Voce di menu "Impostazioni visualizzazione"
-- Inserita sotto "Amministrazione" (MenuId = 6), accanto a "Configurazione Email".
-- Idempotente: non duplica se l'Action è già presente.
IF NOT EXISTS (SELECT 1 FROM base_menues WHERE Action = '/impostazioni-visualizzazione')
BEGIN
    DECLARE @NewMenuId INT = (SELECT ISNULL(MAX(MenuId), 0) + 1 FROM base_menues);

    INSERT INTO base_menues (MenuId, ParentMenuId, Icon, Description, Action, AuthorizationCode, PopulateEvent, IsEnabled, OrderKey, Tags)
    VALUES (@NewMenuId, 6, 'pi-sort-alt', 'Impostazioni visualizzazione', '/impostazioni-visualizzazione', NULL, NULL, 1, 22, NULL);
END
GO

-- Voce di menu "Report spese per tabella millesimale"
-- Inserita sotto "Contabilità" (MenuId = 3), dopo "Rendiconto".
-- Idempotente: non duplica se l'Action è già presente.
IF NOT EXISTS (SELECT 1 FROM base_menues WHERE Action = '/report-spese-millesimali')
BEGIN
    DECLARE @NewMenuId INT = (SELECT ISNULL(MAX(MenuId), 0) + 1 FROM base_menues);

    INSERT INTO base_menues (MenuId, ParentMenuId, Icon, Description, Action, AuthorizationCode, PopulateEvent, IsEnabled, OrderKey, Tags)
    VALUES (@NewMenuId, 3, 'pi-file-export', 'Report spese per millesimi', '/report-spese-millesimali', NULL, NULL, 1, 41, NULL);
END
GO

-- Voce di menu "Bilancio di ripartizione"
-- Inserita sotto "Contabilità" (MenuId = 3), dopo "Report spese per millesimi".
-- Idempotente.
IF NOT EXISTS (SELECT 1 FROM base_menues WHERE Action = '/report-bilancio-ripartizione')
BEGIN
    DECLARE @NewMenuId INT = (SELECT ISNULL(MAX(MenuId), 0) + 1 FROM base_menues);

    INSERT INTO base_menues (MenuId, ParentMenuId, Icon, Description, Action, AuthorizationCode, PopulateEvent, IsEnabled, OrderKey, Tags)
    VALUES (@NewMenuId, 3, 'pi-table', 'Bilancio di ripartizione', '/report-bilancio-ripartizione', NULL, NULL, 1, 42, NULL);
END
GO

-- ============================================================================
-- 6) ConsumptionCharge: aggiunta colonna IsManual.
-- Indica che gli importi delle quote sono stati modificati manualmente:
-- il ricalcolo automatico non sovrascrive piu' gli importi (solo consumi/percentuali).
-- ============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('ConsumptionCharge') AND name = 'IsManual'
)
BEGIN
    ALTER TABLE ConsumptionCharge ADD IsManual BIT NOT NULL CONSTRAINT DF_ConsumptionCharge_IsManual DEFAULT 0;
END
GO

-- ============================================================================
-- 7) ChartOfAccounts: aggiunta flag IsLiquidity
-- Per i conti di tipo Patrimoniale (Type = 3) distingue:
--   IsLiquidity = 1 -> disponibilità liquide (Banca c/c, Cassa contanti)  -> ATTIVITÀ
--   IsLiquidity = 0 -> fondi (Riserva, Posti auto, ...)                   -> PASSIVITÀ/FONDI
-- Necessario per il prospetto "Situazione Patrimoniale" del bilancio condominiale.
-- ============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('ChartOfAccounts') AND name = 'IsLiquidity'
)
BEGIN
    ALTER TABLE ChartOfAccounts ADD IsLiquidity BIT NOT NULL
        CONSTRAINT DF_ChartOfAccounts_IsLiquidity DEFAULT 0;
END
GO

-- ============================================================================
-- 8) TenantDisplaySettings: logo configurabile per tenant (sidebar + report).
--   LogoContent (VARBINARY MAX), LogoContentType, LogoFileName, LogoUpdatedDate.
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TenantDisplaySettings') AND name = 'LogoContent')
    ALTER TABLE TenantDisplaySettings ADD LogoContent VARBINARY(MAX) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TenantDisplaySettings') AND name = 'LogoContentType')
    ALTER TABLE TenantDisplaySettings ADD LogoContentType NVARCHAR(100) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TenantDisplaySettings') AND name = 'LogoFileName')
    ALTER TABLE TenantDisplaySettings ADD LogoFileName NVARCHAR(260) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TenantDisplaySettings') AND name = 'LogoUpdatedDate')
    ALTER TABLE TenantDisplaySettings ADD LogoUpdatedDate DATETIME2 NULL;
GO
