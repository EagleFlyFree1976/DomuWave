-- ============================================================
-- Migration: UnitOpeningBalance — aggiunta ClosingBalance e TotalMovements
-- Da eseguire solo se la tabella è già stata creata con la colonna Amount
-- (migration precedente add_unit_opening_balance.sql).
-- ============================================================

-- Rinomina Amount → OpeningBalance
EXEC sp_rename 'UnitOpeningBalance.Amount', 'OpeningBalance', 'COLUMN';

-- Aggiunge i nuovi campi
ALTER TABLE UnitOpeningBalance ADD TotalMovements DECIMAL(18,4) NOT NULL DEFAULT 0;
ALTER TABLE UnitOpeningBalance ADD ClosingBalance DECIMAL(18,4) NOT NULL DEFAULT 0;
