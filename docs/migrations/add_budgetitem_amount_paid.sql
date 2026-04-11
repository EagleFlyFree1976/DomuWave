-- Add AmountPaid to BudgetItem to track accrual vs cash delta in consuntivo budgets.
-- Amount     = competenza (accrual): spese sostenute / quote emesse
-- AmountPaid = cassa (cash):         spese pagate    / quote incassate

ALTER TABLE BudgetItem
    ADD AmountPaid DECIMAL(18,4) NOT NULL DEFAULT 0;
