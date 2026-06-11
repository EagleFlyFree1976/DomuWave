-- ============================================================================
-- Bonifica: rimozione delle Expense "riepilogo consumi" spurie.
--
-- Modello corretto della ripartizione consumi ("solo bollette reali"):
--   le bollette sono gia' registrate sul conto del tipo consumo e vengono
--   ripartite PER CONSUMO; NON deve esistere alcuna Expense "riepilogo".
--
-- In passato RegenerateExpenseAllocations creava retroattivamente una Expense
-- riepilogo (GrossAmount = TotalAmount della charge) ripartita a millesimi:
-- questo DUPLICAVA il costo (bollette reali + riepilogo). Quel codice e' stato
-- rimosso; questo script ripulisce i dati gia' generati.
--
-- Identificazione: una Expense e' "riepilogo consumi" se e' referenziata da una
-- ConsumptionCharge (ConsumptionCharge.ExpenseId = Expense.Id).
--
-- Azioni (soft-delete, idempotenti):
--   1) scollega la ConsumptionCharge dalla Expense (ExpenseId = NULL)
--   2) soft-delete delle ExpenseAllocation della Expense riepilogo
--   3) soft-delete della Expense riepilogo
-- ============================================================================
SET NOCOUNT ON;

DECLARE @summaryExpenses TABLE (ExpenseId BIGINT PRIMARY KEY);

INSERT INTO @summaryExpenses (ExpenseId)
SELECT DISTINCT cc.ExpenseId
FROM ConsumptionCharge cc
JOIN Expense e ON e.Id = cc.ExpenseId
WHERE cc.ExpenseId IS NOT NULL
  AND cc.IsDeleted = 0
  AND e.IsDeleted = 0;

-- 1) Scollega le charge dalle Expense riepilogo
UPDATE cc
SET cc.ExpenseId      = NULL,
    cc.LastUpdateDate = SYSUTCDATETIME()
FROM ConsumptionCharge cc
WHERE cc.ExpenseId IN (SELECT ExpenseId FROM @summaryExpenses);

-- 2) Soft-delete delle allocazioni delle Expense riepilogo
UPDATE a
SET a.IsDeleted      = 1,
    a.LastUpdateDate = SYSUTCDATETIME()
FROM ExpenseAllocation a
WHERE a.ExpenseId IN (SELECT ExpenseId FROM @summaryExpenses)
  AND a.IsDeleted = 0;

-- 3) Soft-delete delle Expense riepilogo
UPDATE e
SET e.IsDeleted      = 1,
    e.LastUpdateDate = SYSUTCDATETIME()
FROM Expense e
WHERE e.Id IN (SELECT ExpenseId FROM @summaryExpenses)
  AND e.IsDeleted = 0;

SELECT (SELECT COUNT(*) FROM @summaryExpenses) AS ExpenseRiepilogoRimosse;
GO
