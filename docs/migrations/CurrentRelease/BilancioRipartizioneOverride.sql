-- ============================================================================
-- BilancioRipartizioneOverride
-- Override manuali delle celle del Bilancio di ripartizione (calcolato al volo).
-- Ogni record sostituisce il valore calcolato di una cella, identificata da
-- (FiscalYearId, RealEstateUnitId, RowType, CellType, ColumnRefId).
--   RowType : 0 = Proprietari, 1 = Inquilini
--   CellType: 0 = Consumo, 1 = Millesimale, 2 = Dirette, 3 = Accrediti, 4 = Versato
--   ColumnRefId: ConsumptionTypeId o MillesimalTableId per Consumo/Millesimale; 0 altrimenti
-- ============================================================================
-- Richiesto per la creazione dell'indice filtrato (WHERE IsDeleted = 0).
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BilancioRipartizioneOverride')
BEGIN
    CREATE TABLE BilancioRipartizioneOverride (
        Id                       INT              NOT NULL PRIMARY KEY,
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        FiscalYearId             INT              NOT NULL,
        RealEstateUnitId         INT              NOT NULL,

        RowType                  INT              NOT NULL,
        CellType                 INT              NOT NULL,
        ColumnRefId              INT              NOT NULL DEFAULT 0,
        Amount                   DECIMAL(18,4)    NOT NULL DEFAULT 0,

        -- Trace / audit
        CreatedById              INT              NOT NULL,
        CreatedByFullName        NVARCHAR(200)    NULL,
        LastUpdatedById          INT              NULL,
        LastUpdatedByFullName    NVARCHAR(200)    NULL,
        IsDeleted                BIT              NOT NULL DEFAULT 0,
        CreationDate             DATETIME2        NOT NULL,
        LastUpdateDate           DATETIME2        NULL,

        CONSTRAINT FK_BilancioRipartizioneOverride_FiscalYear
            FOREIGN KEY (FiscalYearId) REFERENCES FiscalYear(FiscalYearId),
        CONSTRAINT FK_BilancioRipartizioneOverride_RealEstateUnit
            FOREIGN KEY (RealEstateUnitId) REFERENCES RealEstateUnit(Id)
    );

    -- Un solo override attivo per cella
    CREATE UNIQUE INDEX UQ_BilancioOverride_Cell
        ON BilancioRipartizioneOverride (FiscalYearId, RealEstateUnitId, RowType, CellType, ColumnRefId)
        WHERE IsDeleted = 0;
END
GO

-- Sequenza hilo per la generazione degli Id
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'BilancioRipartizioneOverride', 1
WHERE NOT EXISTS (
    SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'BilancioRipartizioneOverride'
);
GO
