-- ============================================================================
-- TenantDisplaySettings
-- Impostazioni di visualizzazione per tenant (formattazione valori contabili).
-- Prima opzione configurabile: convenzione del segno per le voci contabili.
-- ============================================================================
-- Richiesto per la creazione dell'indice filtrato (WHERE IsDeleted = 0).
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

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
-- Voce di menu "Impostazioni visualizzazione"
-- Inserita sotto "Amministrazione" (MenuId = 6), accanto a "Configurazione Email".
-- Idempotente: non duplica se l'Action è già presente.
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM base_menues WHERE Action = '/impostazioni-visualizzazione')
BEGIN
    DECLARE @NewMenuId INT = (SELECT ISNULL(MAX(MenuId), 0) + 1 FROM base_menues);

    INSERT INTO base_menues (MenuId, ParentMenuId, Icon, Description, Action, AuthorizationCode, PopulateEvent, IsEnabled, OrderKey, Tags)
    VALUES (@NewMenuId, 6, 'pi-sort-alt', 'Impostazioni visualizzazione', '/impostazioni-visualizzazione', NULL, NULL, 1, 22, NULL);
END
GO

