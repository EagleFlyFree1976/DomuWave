-- Migration: aggiungi campi dati bancari e amministratore al Condominium
-- Data: 2026-04-17

ALTER TABLE Condominium ADD
    Iban               NVARCHAR(34)  NULL,
    BankAccountHolder  NVARCHAR(200) NULL,
    BankName           NVARCHAR(100) NULL,
    AdministratorName  NVARCHAR(200) NULL,
    AdministratorPhone NVARCHAR(30)  NULL,
    AdministratorEmail NVARCHAR(255) NULL;
