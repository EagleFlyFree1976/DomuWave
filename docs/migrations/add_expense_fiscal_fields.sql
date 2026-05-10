-- Aggiunge i campi fiscali alla tabella Expense
ALTER TABLE Expense ADD TaxableAmount          DECIMAL(18,4) NOT NULL DEFAULT 0;
ALTER TABLE Expense ADD TaxableAmountVatExempt DECIMAL(18,4) NOT NULL DEFAULT 0;
ALTER TABLE Expense ADD PensionFund            DECIMAL(18,4) NOT NULL DEFAULT 0;
ALTER TABLE Expense ADD WithholdingTax         DECIMAL(18,4) NOT NULL DEFAULT 0;
ALTER TABLE Expense ADD StampDuty              DECIMAL(18,4) NOT NULL DEFAULT 0;
