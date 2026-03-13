-- Migration: aggiunge TotalBalance e TotalRoundingAdjustment ad AccountBalance,
-- e RoundingAdjustment a ExpenseAllocation.

-- AccountBalance: saldo totale persistito e arrotondamenti aggregati
ALTER TABLE AccountBalance ADD TotalBalance            DECIMAL(18,4) NOT NULL DEFAULT 0;
ALTER TABLE AccountBalance ADD TotalRoundingAdjustment DECIMAL(18,4) NOT NULL DEFAULT 0;

-- ExpenseAllocation: arrotondamento applicato a questa singola riga di ripartizione
ALTER TABLE ExpenseAllocation ADD RoundingAdjustment DECIMAL(18,4) NOT NULL DEFAULT 0;
