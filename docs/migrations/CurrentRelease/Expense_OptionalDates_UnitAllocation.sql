-- ============================================================================
-- Expense:
--   1) DocumentDate e RegistrationDate diventano opzionali (NULL)
--   2) MillesimalTableId diventa opzionale (NULL)
--   3) Nuova colonna UnitId: imputazione della spesa a un singolo immobile
--      (alternativa esclusiva alla tabella millesimale)
-- ============================================================================
-- La tabella Expense ha indici filtrati/computed: serve QUOTED_IDENTIFIER ON.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- 1) Date opzionali
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

-- 2) Tabella millesimale opzionale
ALTER TABLE Expense ALTER COLUMN MillesimalTableId INT NULL;
GO

-- 3) Colonna UnitId + FK verso RealEstateUnit
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
