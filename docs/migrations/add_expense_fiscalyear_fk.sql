-- Aggiunge la colonna FiscalYearId alla tabella Expense e la FK verso FiscalYear.
-- La colonna era già mappata nell'HBM (not-null="true") ma mancava nel DB.
-- I record esistenti vengono agganciati all'esercizio fiscale attivo del condominio
-- con la StartDate più recente; se non esiste nessun esercizio il valore rimane NULL
-- (la colonna parte nullable per permettere il backfill, poi diventa NOT NULL).

-- 1. Aggiunge la colonna nullable
ALTER TABLE Expense
    ADD FiscalYearId INT NULL;

-- 2. Backfill: aggancia ogni spesa all'esercizio fiscale del condominio con
--    StartDate più recente (strategia conservativa per dati preesistenti)
UPDATE e
SET    e.FiscalYearId = (
    SELECT TOP 1 fy.FiscalYearId
    FROM   FiscalYear fy
    WHERE  fy.CondominiumId = e.CondominiumId
      AND  fy.IsDeleted     = 0
    ORDER  BY fy.StartDate DESC
)
FROM   Expense e
WHERE  e.IsDeleted = 0;

-- 3. Rende la colonna NOT NULL (dopo il backfill non devono esserci NULL)
ALTER TABLE Expense
    ALTER COLUMN FiscalYearId INT NOT NULL;

-- 4. Aggiunge la FK
ALTER TABLE Expense
    ADD CONSTRAINT FK_Expense_FiscalYear
        FOREIGN KEY (FiscalYearId) REFERENCES FiscalYear(FiscalYearId);
