-- ============================================================
-- Migration: FiscalYear.Status string → int FK to FiscalYearStatusLookup
-- ============================================================

-- 1. Crea la tabella di lookup
CREATE TABLE FiscalYearStatusLookup (
    Id   INT          NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_FiscalYearStatusLookup PRIMARY KEY (Id)
);

-- 2. Inserisci i valori fissi
INSERT INTO FiscalYearStatusLookup (Id, Name) VALUES
    (1, 'Draft'),
    (2, 'Open'),
    (3, 'Closing'),
    (4, 'Closed'),
    (5, 'Locked');

-- 3. Aggiungi la colonna StatusId (nullable temporaneamente)
ALTER TABLE FiscalYear ADD StatusId INT NULL;

-- 4. Migra i dati dalla colonna Status (stringa) a StatusId (intero)
UPDATE FiscalYear SET StatusId = 1 WHERE Status = 'Draft';
UPDATE FiscalYear SET StatusId = 2 WHERE Status = 'Open';
UPDATE FiscalYear SET StatusId = 3 WHERE Status = 'Closing';
UPDATE FiscalYear SET StatusId = 4 WHERE Status = 'Closed';
UPDATE FiscalYear SET StatusId = 5 WHERE Status = 'Locked';

-- Tutti i record senza status mappato → Draft
UPDATE FiscalYear SET StatusId = 1 WHERE StatusId IS NULL;

-- 5. Rendi NOT NULL e aggiungi FK
ALTER TABLE FiscalYear ALTER COLUMN StatusId INT NOT NULL;

ALTER TABLE FiscalYear
    ADD CONSTRAINT FK_FiscalYear_Status
    FOREIGN KEY (StatusId) REFERENCES FiscalYearStatusLookup(Id);

-- 6. Rimuovi la vecchia colonna Status (stringa)
ALTER TABLE FiscalYear DROP COLUMN Status;
