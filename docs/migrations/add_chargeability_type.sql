-- ============================================================
-- Migration: ChargeabilityType lookup + colonne FK
-- 0 = Proprietario (Owner), 1 = Inquilino (Tenant), 2 = Automatico (Auto)
-- ============================================================

-- 1. Tabella di lookup
CREATE TABLE ChargeabilityTypeLookup (
    Id   INT          NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);

INSERT INTO ChargeabilityTypeLookup (Id, Name) VALUES
    (0, 'Proprietario'),
    (1, 'Inquilino'),
    (2, 'Automatico');

-- 2. Aggiunta colonne con FK
ALTER TABLE ChartOfAccounts ADD ChargeabilityType INT NOT NULL DEFAULT 0;
ALTER TABLE ChartOfAccounts ADD CONSTRAINT FK_ChartOfAccounts_ChargeabilityType
    FOREIGN KEY (ChargeabilityType) REFERENCES ChargeabilityTypeLookup(Id);

ALTER TABLE Expense ADD ChargeabilityType INT NOT NULL DEFAULT 0;
ALTER TABLE Expense ADD CONSTRAINT FK_Expense_ChargeabilityType
    FOREIGN KEY (ChargeabilityType) REFERENCES ChargeabilityTypeLookup(Id);
