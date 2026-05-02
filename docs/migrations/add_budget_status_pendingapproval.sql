-- Add BudgetStatus 'In approvazione' (PendingApproval = 4)
IF NOT EXISTS (SELECT 1 FROM BudgetStatusLookup WHERE Id = 4)
BEGIN
    INSERT INTO BudgetStatusLookup (Id, Name) VALUES (4, 'In approvazione');
END
