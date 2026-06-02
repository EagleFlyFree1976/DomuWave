-- ============================================================================
-- Expense: aggiunta flag Send770 ("Invia 770")
-- Indica se la spesa deve essere inclusa nella certificazione/modello 770.
-- ============================================================================
-- La tabella Expense ha indici filtrati/computed: serve QUOTED_IDENTIFIER ON.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Expense') AND name = 'Send770'
)
BEGIN
    ALTER TABLE Expense ADD Send770 BIT NOT NULL DEFAULT 0;
END
GO
