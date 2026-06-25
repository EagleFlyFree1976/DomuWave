-- ============================================================================
-- ChartOfAccounts: aggiunta flag IsLiquidity
-- Per i conti di tipo Patrimoniale (Type = 3) distingue:
--   IsLiquidity = 1 -> disponibilità liquide (Banca c/c, Cassa contanti)  -> ATTIVITÀ
--   IsLiquidity = 0 -> fondi (Riserva, Posti auto, ...)                   -> PASSIVITÀ/FONDI
-- Necessario per il prospetto "Situazione Patrimoniale" del bilancio condominiale.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('ChartOfAccounts') AND name = 'IsLiquidity'
)
BEGIN
    ALTER TABLE ChartOfAccounts ADD IsLiquidity BIT NOT NULL
        CONSTRAINT DF_ChartOfAccounts_IsLiquidity DEFAULT 0;
END
GO
