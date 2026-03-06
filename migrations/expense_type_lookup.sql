-- ============================================================
-- Migration: ExpenseType string → integer FK
-- ============================================================

-- 1. Create lookup table
CREATE TABLE ExpenseTypeLookup (
    Id   INT          NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);

INSERT INTO ExpenseTypeLookup (Id, Name) VALUES
    (1, 'Manutenzione'),
    (2, 'Pulizie'),
    (3, 'Sicurezza'),
    (4, 'Utenze'),
    (5, 'Professionale'),
    (6, 'Altro');

-- 2. Add new integer column (nullable first for migration)
ALTER TABLE Expense ADD ExpenseTypeId INT NULL;

-- 3. Migrate existing string values to integer ids
UPDATE Expense SET ExpenseTypeId = 1 WHERE LOWER(ExpenseType) LIKE '%manutenzione%';
UPDATE Expense SET ExpenseTypeId = 2 WHERE LOWER(ExpenseType) LIKE '%pulizie%';
UPDATE Expense SET ExpenseTypeId = 3 WHERE LOWER(ExpenseType) LIKE '%sicurezza%';
UPDATE Expense SET ExpenseTypeId = 4 WHERE LOWER(ExpenseType) LIKE '%utenze%';
UPDATE Expense SET ExpenseTypeId = 5 WHERE LOWER(ExpenseType) LIKE '%professional%';
-- Any remaining unmapped rows → Altro
UPDATE Expense SET ExpenseTypeId = 6 WHERE ExpenseTypeId IS NULL;

-- 4. Set NOT NULL and add FK
ALTER TABLE Expense ALTER COLUMN ExpenseTypeId INT NOT NULL;
ALTER TABLE Expense ADD CONSTRAINT FK_Expense_ExpenseType FOREIGN KEY (ExpenseTypeId) REFERENCES ExpenseTypeLookup(Id);

-- 5. Drop old string column
ALTER TABLE Expense DROP COLUMN ExpenseType;
