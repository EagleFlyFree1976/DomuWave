-- ============================================================
-- Migration: Budget.Status string → BudgetStatusLookup FK
-- ============================================================

-- 1. Tabella di lookup stati budget
CREATE TABLE BudgetStatusLookup (
    Id   INT          NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);

INSERT INTO BudgetStatusLookup (Id, Name) VALUES (1, 'Draft');
INSERT INTO BudgetStatusLookup (Id, Name) VALUES (2, 'Approved');
INSERT INTO BudgetStatusLookup (Id, Name) VALUES (3, 'Closed');

-- 2. Aggiunge colonna StatusId alla tabella Budget
ALTER TABLE Budget ADD StatusId INT NULL;

-- 3. Migra i dati esistenti
UPDATE Budget
SET StatusId = CASE Status
    WHEN 'Draft'    THEN 1
    WHEN 'Approved' THEN 2
    WHEN 'Closed'   THEN 3
    ELSE 1
END;

-- 4. Rende NOT NULL e aggiunge FK
ALTER TABLE Budget ALTER COLUMN StatusId INT NOT NULL;

ALTER TABLE Budget
    ADD CONSTRAINT FK_Budget_BudgetStatusLookup
    FOREIGN KEY (StatusId) REFERENCES BudgetStatusLookup(Id);

-- 5. Rimuove la vecchia colonna stringa
ALTER TABLE Budget DROP COLUMN Status;
