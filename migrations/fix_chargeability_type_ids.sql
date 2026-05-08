-- Aggiorna gli id di ChargeabilityTypeLookup da 0-based a 1-based
-- e aggiorna i FK nelle tabelle che la referenziano

-- 1. Rimuovi FK temporaneamente
ALTER TABLE ChartOfAccounts DROP CONSTRAINT FK_ChartOfAccounts_ChargeabilityTypeLookup;
ALTER TABLE Expense DROP CONSTRAINT FK_Expense_ChargeabilityTypeLookup;

-- 2. Aggiorna i valori nelle tabelle figlie (dal valore più alto al più basso per evitare conflitti)
UPDATE ChartOfAccounts SET ChargeabilityType = 3 WHERE ChargeabilityType = 2;
UPDATE ChartOfAccounts SET ChargeabilityType = 2 WHERE ChargeabilityType = 1;
UPDATE ChartOfAccounts SET ChargeabilityType = 1 WHERE ChargeabilityType = 0;

UPDATE Expense SET ChargeabilityType = 3 WHERE ChargeabilityType = 2;
UPDATE Expense SET ChargeabilityType = 2 WHERE ChargeabilityType = 1;
UPDATE Expense SET ChargeabilityType = 1 WHERE ChargeabilityType = 0;

-- 3. Aggiorna la lookup (dal valore più alto al più basso)
UPDATE ChargeabilityTypeLookup SET Id = 3 WHERE Id = 2;
UPDATE ChargeabilityTypeLookup SET Id = 2 WHERE Id = 1;
UPDATE ChargeabilityTypeLookup SET Id = 1 WHERE Id = 0;

-- 4. Ricrea i FK
ALTER TABLE ChartOfAccounts ADD CONSTRAINT FK_ChartOfAccounts_ChargeabilityTypeLookup
    FOREIGN KEY (ChargeabilityType) REFERENCES ChargeabilityTypeLookup(Id);
ALTER TABLE Expense ADD CONSTRAINT FK_Expense_ChargeabilityTypeLookup
    FOREIGN KEY (ChargeabilityType) REFERENCES ChargeabilityTypeLookup(Id);
