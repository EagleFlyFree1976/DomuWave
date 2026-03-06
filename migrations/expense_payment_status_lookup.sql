-- ============================================================
-- Migration: Expense.PaymentStatus string → integer FK
-- ============================================================

-- 1. Create lookup table
CREATE TABLE ExpensePaymentStatusLookup (
    Id   INT          NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);

INSERT INTO ExpensePaymentStatusLookup (Id, Name) VALUES
    (1, 'Da pagare'),
    (2, 'Pagata'),
    (3, 'Scaduta');

-- 2. Add new integer column (nullable first for migration)
ALTER TABLE Expense ADD PaymentStatusId INT NULL;

-- 3. Migrate existing string values to integer ids
UPDATE Expense SET PaymentStatusId = 2 WHERE PaymentStatus IN ('Paid', 'paid');
UPDATE Expense SET PaymentStatusId = 3 WHERE PaymentStatus IN ('Overdue', 'overdue');
-- Any remaining (ToPay or unmapped) → Da pagare
UPDATE Expense SET PaymentStatusId = 1 WHERE PaymentStatusId IS NULL;

-- 4. Set NOT NULL and add FK
ALTER TABLE Expense ALTER COLUMN PaymentStatusId INT NOT NULL;
ALTER TABLE Expense ADD CONSTRAINT FK_Expense_PaymentStatus FOREIGN KEY (PaymentStatusId) REFERENCES ExpensePaymentStatusLookup(Id);

-- 5. Drop old string column
ALTER TABLE Expense DROP COLUMN PaymentStatus;
