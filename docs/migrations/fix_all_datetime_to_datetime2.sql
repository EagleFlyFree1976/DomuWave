-- ============================================================
-- Migration: converti tutte le colonne CreationDate / LastUpdateDate
-- da datetime a datetime2 in tutte le tabelle TraceEntity
-- ============================================================

-- Correggi eventuali valori fuori range prima di alterare
DECLARE @tables TABLE (TableName NVARCHAR(100));
INSERT INTO @tables VALUES
    ('Budget'), ('BudgetItem'),
    ('ChartOfAccounts'), ('ChartOfAccountsCategory'), ('ChartOfAccountsCategoryTemplate'),
    ('Communication'), ('CommunicationRead'),
    ('Condominium'), ('CondominiumAddress'), ('CondominiumCadastralData'),
    ('CondominiumFee'), ('CondominiumInstallment'),
    ('Document'), ('DocumentAccess'),
    ('Expense'), ('ExpenseAllocation'),
    ('FiscalYear'),
    ('Message'),
    ('MillesimalTable'),
    ('Payment'),
    ('Receipt'),
    ('RealEstateUnit'),
    ('Supplier'), ('SupplierContract'),
    ('Tenant'),
    ('UnitMillesimal'), ('UnitOwner'), ('UnitTenant'),
    ('UserTenant');

-- Correggi valori DateTime.MinValue (0001-01-01)
UPDATE Budget                       SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE BudgetItem                   SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE ChartOfAccounts              SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE ChartOfAccountsCategory      SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE ChartOfAccountsCategoryTemplate SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE Communication                SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE CommunicationRead            SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE Condominium                  SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE CondominiumAddress           SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE CondominiumCadastralData     SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE CondominiumFee               SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE CondominiumInstallment       SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE Document                     SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE DocumentAccess               SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE Expense                      SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE ExpenseAllocation            SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE FiscalYear                   SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE Message                      SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE MillesimalTable              SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE Payment                      SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE Receipt                      SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE RealEstateUnit               SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE Supplier                     SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE SupplierContract             SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE Tenant                       SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE UnitMillesimal               SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE UnitOwner                    SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE UnitTenant                   SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';
UPDATE UserTenant                   SET CreationDate = '1900-01-01' WHERE CreationDate < '1753-01-01';

-- Altera le colonne a datetime2
ALTER TABLE Budget                       ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE Budget                       ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE BudgetItem                   ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE BudgetItem                   ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE ChartOfAccounts              ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE ChartOfAccounts              ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE ChartOfAccountsCategory      ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE ChartOfAccountsCategory      ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE ChartOfAccountsCategoryTemplate ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE ChartOfAccountsCategoryTemplate ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE Communication                ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE Communication                ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE CommunicationRead            ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE CommunicationRead            ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE Condominium                  ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE Condominium                  ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE CondominiumAddress           ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE CondominiumAddress           ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE CondominiumCadastralData     ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE CondominiumCadastralData     ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE CondominiumFee               ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE CondominiumFee               ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE CondominiumInstallment       ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE CondominiumInstallment       ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE Document                     ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE Document                     ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE DocumentAccess               ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE DocumentAccess               ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE Expense                      ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE Expense                      ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE ExpenseAllocation            ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE ExpenseAllocation            ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE FiscalYear                   ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE FiscalYear                   ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE Message                      ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE Message                      ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE MillesimalTable              ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE MillesimalTable              ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE Payment                      ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE Payment                      ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE Receipt                      ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE Receipt                      ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE RealEstateUnit               ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE RealEstateUnit               ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE Supplier                     ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE Supplier                     ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE SupplierContract             ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE SupplierContract             ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE Tenant                       ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE Tenant                       ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE UnitMillesimal               ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE UnitMillesimal               ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE UnitOwner                    ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE UnitOwner                    ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE UnitTenant                   ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE UnitTenant                   ALTER COLUMN LastUpdateDate datetime2 NULL;
ALTER TABLE UserTenant                   ALTER COLUMN CreationDate datetime2 NOT NULL;
ALTER TABLE UserTenant                   ALTER COLUMN LastUpdateDate datetime2 NULL;
