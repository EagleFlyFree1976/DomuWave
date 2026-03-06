-- ============================================================
-- Aggiunge CreationDate e LastUpdateDate dove mancano
-- Generato da mapping HBM - solo tabelle con queste proprietà
-- ============================================================

DECLARE @tables TABLE (TableName NVARCHAR(128));
INSERT INTO @tables VALUES
    ('BudgetItem'),
    ('Budget'),
    ('ChartOfAccountsCategory'),
    ('ChartOfAccounts'),
    ('ChartOfAccountsCategoryTemplate'),
    ('CondominiumInstallment'),
    ('CondominiumFee'),
    ('ExpenseAllocation'),
    ('Condominium'),
    ('CondominiumAddress'),
    ('CondominiumCadastralData'),
    ('FiscalYear'),
    ('Message'),
    ('Communication'),
    ('CommunicationRead'),
    ('Payment'),
    ('MillesimalTable'),
    ('Expense'),
    ('Document'),
    ('DocumentAccess'),
    ('Supplier'),
    ('SupplierContract'),
    ('Receipt'),
    ('RealEstateUnit'),
    ('UnitOwner'),
    ('UnitTenant'),
    ('UnitMillesimal'),
    ('UserTenant'),
    ('Tenant');

-- CreationDate
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('BudgetItem') AND name = 'CreationDate')
    ALTER TABLE BudgetItem ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Budget') AND name = 'CreationDate')
    ALTER TABLE Budget ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ChartOfAccountsCategory') AND name = 'CreationDate')
    ALTER TABLE ChartOfAccountsCategory ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ChartOfAccounts') AND name = 'CreationDate')
    ALTER TABLE ChartOfAccounts ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ChartOfAccountsCategoryTemplate') AND name = 'CreationDate')
    ALTER TABLE ChartOfAccountsCategoryTemplate ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CondominiumInstallment') AND name = 'CreationDate')
    ALTER TABLE CondominiumInstallment ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CondominiumFee') AND name = 'CreationDate')
    ALTER TABLE CondominiumFee ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseAllocation') AND name = 'CreationDate')
    ALTER TABLE ExpenseAllocation ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Condominium') AND name = 'CreationDate')
    ALTER TABLE Condominium ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CondominiumAddress') AND name = 'CreationDate')
    ALTER TABLE CondominiumAddress ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CondominiumCadastralData') AND name = 'CreationDate')
    ALTER TABLE CondominiumCadastralData ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FiscalYear') AND name = 'CreationDate')
    ALTER TABLE FiscalYear ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Message') AND name = 'CreationDate')
    ALTER TABLE Message ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Communication') AND name = 'CreationDate')
    ALTER TABLE Communication ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CommunicationRead') AND name = 'CreationDate')
    ALTER TABLE CommunicationRead ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Payment') AND name = 'CreationDate')
    ALTER TABLE Payment ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MillesimalTable') AND name = 'CreationDate')
    ALTER TABLE MillesimalTable ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Expense') AND name = 'CreationDate')
    ALTER TABLE Expense ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Document') AND name = 'CreationDate')
    ALTER TABLE Document ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DocumentAccess') AND name = 'CreationDate')
    ALTER TABLE DocumentAccess ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Supplier') AND name = 'CreationDate')
    ALTER TABLE Supplier ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SupplierContract') AND name = 'CreationDate')
    ALTER TABLE SupplierContract ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Receipt') AND name = 'CreationDate')
    ALTER TABLE Receipt ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RealEstateUnit') AND name = 'CreationDate')
    ALTER TABLE RealEstateUnit ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UnitOwner') AND name = 'CreationDate')
    ALTER TABLE UnitOwner ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UnitTenant') AND name = 'CreationDate')
    ALTER TABLE UnitTenant ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UnitMillesimal') AND name = 'CreationDate')
    ALTER TABLE UnitMillesimal ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserTenant') AND name = 'CreationDate')
    ALTER TABLE UserTenant ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Tenant') AND name = 'CreationDate')
    ALTER TABLE Tenant ADD CreationDate DATETIME NOT NULL DEFAULT GETDATE();

-- LastUpdateDate
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('BudgetItem') AND name = 'LastUpdateDate')
    ALTER TABLE BudgetItem ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Budget') AND name = 'LastUpdateDate')
    ALTER TABLE Budget ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ChartOfAccountsCategory') AND name = 'LastUpdateDate')
    ALTER TABLE ChartOfAccountsCategory ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ChartOfAccounts') AND name = 'LastUpdateDate')
    ALTER TABLE ChartOfAccounts ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ChartOfAccountsCategoryTemplate') AND name = 'LastUpdateDate')
    ALTER TABLE ChartOfAccountsCategoryTemplate ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CondominiumInstallment') AND name = 'LastUpdateDate')
    ALTER TABLE CondominiumInstallment ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CondominiumFee') AND name = 'LastUpdateDate')
    ALTER TABLE CondominiumFee ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseAllocation') AND name = 'LastUpdateDate')
    ALTER TABLE ExpenseAllocation ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Condominium') AND name = 'LastUpdateDate')
    ALTER TABLE Condominium ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CondominiumAddress') AND name = 'LastUpdateDate')
    ALTER TABLE CondominiumAddress ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CondominiumCadastralData') AND name = 'LastUpdateDate')
    ALTER TABLE CondominiumCadastralData ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FiscalYear') AND name = 'LastUpdateDate')
    ALTER TABLE FiscalYear ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Message') AND name = 'LastUpdateDate')
    ALTER TABLE Message ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Communication') AND name = 'LastUpdateDate')
    ALTER TABLE Communication ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CommunicationRead') AND name = 'LastUpdateDate')
    ALTER TABLE CommunicationRead ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Payment') AND name = 'LastUpdateDate')
    ALTER TABLE Payment ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MillesimalTable') AND name = 'LastUpdateDate')
    ALTER TABLE MillesimalTable ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Expense') AND name = 'LastUpdateDate')
    ALTER TABLE Expense ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Document') AND name = 'LastUpdateDate')
    ALTER TABLE Document ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DocumentAccess') AND name = 'LastUpdateDate')
    ALTER TABLE DocumentAccess ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Supplier') AND name = 'LastUpdateDate')
    ALTER TABLE Supplier ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SupplierContract') AND name = 'LastUpdateDate')
    ALTER TABLE SupplierContract ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Receipt') AND name = 'LastUpdateDate')
    ALTER TABLE Receipt ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RealEstateUnit') AND name = 'LastUpdateDate')
    ALTER TABLE RealEstateUnit ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UnitOwner') AND name = 'LastUpdateDate')
    ALTER TABLE UnitOwner ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UnitTenant') AND name = 'LastUpdateDate')
    ALTER TABLE UnitTenant ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UnitMillesimal') AND name = 'LastUpdateDate')
    ALTER TABLE UnitMillesimal ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserTenant') AND name = 'LastUpdateDate')
    ALTER TABLE UserTenant ADD LastUpdateDate DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Tenant') AND name = 'LastUpdateDate')
    ALTER TABLE Tenant ADD LastUpdateDate DATETIME NULL;

PRINT 'Done.';
