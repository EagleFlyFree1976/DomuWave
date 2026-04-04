-- Migration: denormalizza AccountCode e AccountName su BudgetItem
-- Motivazione: il nome/codice del conto non deve cambiare retroattivamente
-- sui budget già approvati o chiusi quando il piano dei conti viene modificato.
-- I valori vengono fotografati al momento della creazione/ricalcolo della voce.
-- Per le righe esistenti il fallback è letto live dalla join con ChartOfAccounts.

ALTER TABLE BudgetItem ADD AccountCode NVARCHAR(50)  NULL;
ALTER TABLE BudgetItem ADD AccountName NVARCHAR(200) NULL;

-- Popola i valori esistenti dalla join con ChartOfAccounts
-- (solo per le righe non cancellate; le cancellate rimangono NULL)
UPDATE bi
SET    bi.AccountCode = coa.Code,
       bi.AccountName = coa.Name
FROM   BudgetItem bi
JOIN   ChartOfAccounts coa ON coa.Id = bi.AccountId
WHERE  bi.IsDeleted = 0;
