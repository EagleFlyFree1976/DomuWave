-- ============================================================================
-- Condominium: saldo iniziale di cassa a livello di condominio.
--   InitialBalance -> disponibilità liquide di partenza del condominio.
--                     Confluisce come avanzo/disponibilità iniziale nei prospetti
--                     di cassa (Flussi di cassa, Situazione patrimoniale) SOLO per
--                     il primo esercizio del condominio. NOT NULL DEFAULT 0.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Condominium') AND name = 'InitialBalance'
)
BEGIN
    ALTER TABLE Condominium ADD InitialBalance DECIMAL(18,4) NOT NULL
        CONSTRAINT DF_Condominium_InitialBalance DEFAULT 0;
END
GO
