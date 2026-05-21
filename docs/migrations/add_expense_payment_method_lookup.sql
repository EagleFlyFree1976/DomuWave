-- Lookup tabella metodi di pagamento spese
CREATE TABLE ExpensePaymentMethodLookup (
    Id   INT          NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);
GO
INSERT INTO ExpensePaymentMethodLookup (Id, Name) VALUES
    (1,  'Contanti'),
    (2,  'Carta di credito / debito (POS)'),
    (3,  'Assegno bancario'),
    (4,  'Assegno circolare'),
    (5,  'Bonifico bancario immediato'),
    (6,  'Bonifico bancario 30 gg'),
    (7,  'Bonifico bancario 60 gg'),
    (8,  'Bonifico bancario 90 gg'),
    (9,  'RID / SDD - Addebito diretto SEPA'),
    (10, 'RiBa - Ricevuta Bancaria'),
    (11, 'MAV'),
    (12, 'Fattura fine mese'),
    (13, 'Fattura fine mese + 30 gg'),
    (14, 'PayPal'),
    (15, 'PagoPA');
GO
-- Aggiunge colonna FK su Expense (nullable: il metodo può non essere specificato)
ALTER TABLE Expense ADD PaymentMethodId INT NULL;

ALTER TABLE Expense ADD CONSTRAINT FK_Expense_ExpensePaymentMethodLookup
    FOREIGN KEY (PaymentMethodId) REFERENCES ExpensePaymentMethodLookup(Id);
GO
-- Migrazione dati esistenti: mappa i valori stringa precedenti agli id lookup
UPDATE Expense SET PaymentMethodId = 1  WHERE PaymentMethod = 'Cash';
UPDATE Expense SET PaymentMethodId = 2  WHERE PaymentMethod = 'CreditCard';
UPDATE Expense SET PaymentMethodId = 3  WHERE PaymentMethod = 'Check';
UPDATE Expense SET PaymentMethodId = 5  WHERE PaymentMethod = 'BankTransfer';
GO
-- Rimuove la vecchia colonna stringa
ALTER TABLE Expense DROP COLUMN PaymentMethod;
GO