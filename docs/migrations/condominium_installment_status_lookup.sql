-- ============================================================
-- Migration: CondominiumInstallment.Status string → int FK to CondominiumInstallmentStatusLookup
-- ============================================================

-- 1. Crea la tabella di lookup
CREATE TABLE CondominiumInstallmentStatusLookup (
    Id   INT          NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_CondominiumInstallmentStatusLookup PRIMARY KEY (Id)
);

-- 2. Inserisci i valori fissi
INSERT INTO CondominiumInstallmentStatusLookup (Id, Name) VALUES
    (1, 'Draft'),
    (2, 'Open'),
    (3, 'Paid'),
    (4, 'Overdue'),
    (5, 'Cancelled');

-- 3. Aggiungi la colonna StatusId (nullable temporaneamente)
ALTER TABLE CondominiumInstallment ADD StatusId INT NULL;

-- 4. Migra i dati dalla colonna Status (stringa) a StatusId (intero)
UPDATE CondominiumInstallment SET StatusId = 1 WHERE Status = 'Draft';
UPDATE CondominiumInstallment SET StatusId = 2 WHERE Status = 'Open';
UPDATE CondominiumInstallment SET StatusId = 3 WHERE Status = 'Paid';
UPDATE CondominiumInstallment SET StatusId = 4 WHERE Status = 'Overdue';
UPDATE CondominiumInstallment SET StatusId = 5 WHERE Status = 'Cancelled';

-- Tutti i record senza status mappato → Draft
UPDATE CondominiumInstallment SET StatusId = 1 WHERE StatusId IS NULL;

-- 5. Rendi NOT NULL e aggiungi FK
ALTER TABLE CondominiumInstallment ALTER COLUMN StatusId INT NOT NULL;

ALTER TABLE CondominiumInstallment
    ADD CONSTRAINT FK_CondominiumInstallment_Status
    FOREIGN KEY (StatusId) REFERENCES CondominiumInstallmentStatusLookup(Id);

-- 6. Rimuovi la vecchia colonna Status (stringa)
ALTER TABLE CondominiumInstallment DROP COLUMN Status;
