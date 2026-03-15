-- ============================================================
-- Migration: Caricamento staging stg.VZ_*
-- Esegui questo script su: DomuWave (database target)
-- Fonte dati:   vzcondominio (stesso server SQL)
--
-- Prerequisiti:
-- 1) Eseguire prima: vzcondominio_migration_stage0.sql
-- 2) L'utente deve avere SELECT sui db vzcondominio e DomuWave
--
-- Idempotente: TRUNCATE + INSERT ad ogni run, inteso come refresh
-- completo dello staging. I dati produzione (Map_VZ_*, Condominium, ...)
-- NON vengono toccati da questo script.
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- ------------------------------------------------------------
-- 1) VZ_Condominium
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_Condominium;

INSERT INTO stg.VZ_Condominium (VZId, Name, TaxCode, IbanRaw, Notes)
SELECT
    id,
    ISNULL(nome, CONCAT('Condominio-', id)),
    NULLIF(LTRIM(RTRIM(codiceFiscale)), ''),
    NULLIF(LTRIM(RTRIM(iban)), ''),
    NULLIF(LTRIM(RTRIM(note)), '')
FROM vzcondominio.Domus.VZCondominio;

-- ------------------------------------------------------------
-- 2) VZ_Building (VZEdificio)
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_Building;

INSERT INTO stg.VZ_Building (VZId, VZCondominiumId, Name, Street, City, Province, PostalCode, Country, Floors, Notes)
SELECT
    id,
    idCondominio,
    ISNULL(nome, CONCAT('Edificio-', id)),
    NULLIF(LTRIM(RTRIM(indirizzo)), ''),
    NULLIF(LTRIM(RTRIM(citta)), ''),
    NULLIF(LTRIM(RTRIM(provincia)), ''),
    NULLIF(LTRIM(RTRIM(codicePostale)), ''),
    NULLIF(LTRIM(RTRIM(nazione)), 'IT'),
    piani,
    NULLIF(LTRIM(RTRIM(note)), '')
FROM vzcondominio.Domus.VZEdificio;

-- ------------------------------------------------------------
-- 3) VZ_Staircase (VZScala)
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_Staircase;

INSERT INTO stg.VZ_Staircase (VZId, VZBuildingId, Name, StreetNumber)
SELECT
    id,
    idEdificio,
    ISNULL(NULLIF(LTRIM(RTRIM(nome)), ''), CONCAT('Scala-', id)),
    NULLIF(LTRIM(RTRIM(nCivico)), '')
FROM vzcondominio.Domus.VZScala;

-- ------------------------------------------------------------
-- 4) VZ_Unit (VZImmobile)
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_Unit;

INSERT INTO stg.VZ_Unit (
    VZId, VZBuildingId, VZStaircaseId, Description, FloorNumber,
    CadastralSheet, CadastralParcel, CadastralSub,
    MillesimalGeneral, MillesimalStaircase, MillesimalLift, Notes
)
SELECT
    id,
    idEdificio,
    idScala,
    ISNULL(NULLIF(LTRIM(RTRIM(descrizione)), ''), CONCAT('Unita-', id)),
    piano,
    foglio,
    mappale,
    subalterno,
    CAST(millesimi AS DECIMAL(18,4)),
    CAST(millesimiScala AS DECIMAL(18,4)),
    CAST(millesimiAscensore AS DECIMAL(18,4)),
    NULLIF(LTRIM(RTRIM(note)), '')
FROM vzcondominio.Domus.VZImmobile;

-- ------------------------------------------------------------
-- 5) VZ_Person (VZPersona)
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_Person;

INSERT INTO stg.VZ_Person (
    VZId, FirstName, LastName, CompanyName, TaxCode, VatNumber,
    Phone, Mobile, Email, Pec, Address, City, Province, PostalCode, Country, Notes
)
SELECT
    id,
    NULLIF(LTRIM(RTRIM(nome)), ''),
    NULLIF(LTRIM(RTRIM(cognome)), ''),
    NULLIF(LTRIM(RTRIM(ragioneSociale)), ''),
    NULLIF(LTRIM(RTRIM(codiceFiscale)), ''),
    NULLIF(LTRIM(RTRIM(partitaIva)), ''),
    NULLIF(LTRIM(RTRIM(telefono)), ''),
    NULLIF(LTRIM(RTRIM(cellulare)), ''),
    NULLIF(LTRIM(RTRIM(email)), ''),
    NULLIF(LTRIM(RTRIM(pec)), ''),
    NULLIF(LTRIM(RTRIM(indirizzo)), ''),
    NULLIF(LTRIM(RTRIM(citta)), ''),
    NULLIF(LTRIM(RTRIM(provincia)), ''),
    NULLIF(LTRIM(RTRIM(codicePostale)), ''),
    ISNULL(NULLIF(LTRIM(RTRIM(nazione)), ''), 'IT'),
    NULLIF(LTRIM(RTRIM(note)), '')
FROM vzcondominio.Anagrafiche.VZPersona;

-- ------------------------------------------------------------
-- 6) VZ_Supplier (VZFornitore)
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_Supplier;

INSERT INTO stg.VZ_Supplier (
    VZId, FirstName, LastName, CompanyName, TaxCode, VatNumber,
    Phone, Mobile, Email, Pec, Address, City, Province, PostalCode, Country, Notes
)
SELECT
    id,
    NULLIF(LTRIM(RTRIM(nome)), ''),
    NULLIF(LTRIM(RTRIM(cognome)), ''),
    NULLIF(LTRIM(RTRIM(ragioneSociale)), ''),
    NULLIF(LTRIM(RTRIM(codiceFiscale)), ''),
    NULLIF(LTRIM(RTRIM(partitaIva)), ''),
    NULLIF(LTRIM(RTRIM(telefono)), ''),
    NULLIF(LTRIM(RTRIM(cellulare)), ''),
    NULLIF(LTRIM(RTRIM(email)), ''),
    NULLIF(LTRIM(RTRIM(pec)), ''),
    NULLIF(LTRIM(RTRIM(indirizzo)), ''),
    NULLIF(LTRIM(RTRIM(citta)), ''),
    NULLIF(LTRIM(RTRIM(provincia)), ''),
    NULLIF(LTRIM(RTRIM(codicePostale)), ''),
    ISNULL(NULLIF(LTRIM(RTRIM(nazione)), ''), 'IT'),
    NULLIF(LTRIM(RTRIM(note)), '')
FROM vzcondominio.Anagrafiche.VZFornitore;

-- ------------------------------------------------------------
-- 7) VZ_FiscalYear (VZEsercizio)
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_FiscalYear;

INSERT INTO stg.VZ_FiscalYear (
    VZId, VZCondominiumId, Description, StartDate, EndDate, IsExtraordinary, IsClosed
)
SELECT
    id,
    idCondominio,
    ISNULL(NULLIF(LTRIM(RTRIM(descrizione)), ''), CONCAT('Esercizio-', id)),
    CAST(dataInizio AS DATETIME2),
    CAST(dataFine AS DATETIME2),
    ISNULL(straordinario, 0),
    ISNULL(chiuso, 0)
FROM vzcondominio.Gestione.VZEsercizio;

-- ------------------------------------------------------------
-- 8) VZ_Invoice (VZFattura) — senza la colonna base64 per performance
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_Invoice;

INSERT INTO stg.VZ_Invoice (
    VZId, Number, Protocol, IssueDate, DueDate, PaymentDate,
    TaxableAmount, VatAmount, WithholdingAmount, PensionFundAmount,
    StampDutyAmount, TotalAmount,
    Notes, VZSupplierId, IsVatExempt, TaxCode, NonTaxableAmount
)
SELECT
    id,
    ISNULL(NULLIF(LTRIM(RTRIM(numero)), ''), CONCAT('N-', id)),
    NULLIF(LTRIM(RTRIM(protocollo)), ''),
    data,
    dataScadenza,
    dataPagamento,
    CAST(ISNULL(imponibile, 0) AS DECIMAL(18,4)),
    CAST(ISNULL(iva, 0) AS DECIMAL(18,4)),
    CAST(ISNULL(ritenuta, 0) AS DECIMAL(18,4)),
    CAST(ISNULL(cassa, 0) AS DECIMAL(18,4)),
    CAST(ISNULL(bollo, 0) AS DECIMAL(18,4)),
    CAST(ISNULL(totale, 0) AS DECIMAL(18,4)),
    NULLIF(LTRIM(RTRIM(note)), ''),
    idFornitore,
    ISNULL(esenteIva, 0),
    NULLIF(LTRIM(RTRIM(codiceTributo)), ''),
    CAST(ISNULL(nonSoggetti, 0) AS DECIMAL(18,4))
FROM vzcondominio.Gestione.VZFattura;

-- Carica i documenti base64 separatamente (pesanti ~21MB)
-- Se vuoi includerli esegui dopo:
--   UPDATE stg.VZ_Invoice SET Base64Document = src.base64
--   FROM stg.VZ_Invoice i
--   INNER JOIN vzcondominio.Gestione.VZFattura src ON src.id = i.VZId
--   WHERE src.base64 IS NOT NULL;

-- ------------------------------------------------------------
-- 9) VZ_Expense (VZSpesa)
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_Expense;

INSERT INTO stg.VZ_Expense (
    VZId, VZBuildingId, VZStaircaseId, VZUnitId, VZPersonId,
    VZMacroCategoryId, VZCategoryId, VZFiscalYearId,
    ExpenseDate, Description, Amount, Notes, VZInvoiceId, Is770
)
SELECT
    id,
    idEdificio,
    idScala,
    idImmobile,
    idPersona,
    idMacroCategoria,
    idCategoria,
    idEsercizio,
    dataSpesa,
    ISNULL(NULLIF(LTRIM(RTRIM(descrizione)), ''), CONCAT('Spesa-', id)),
    CAST(ISNULL(importo, 0) AS DECIMAL(18,4)),
    NULLIF(LTRIM(RTRIM(note)), ''),
    idFattura,
    ISNULL(mod770, 0)
FROM vzcondominio.Gestione.VZSpesa;

-- ------------------------------------------------------------
-- 10) VZ_Payment (VZPagamento)
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_Payment;

INSERT INTO stg.VZ_Payment (
    VZId, VZPersonId, VZFiscalYearId, Description, PaymentDate, Amount, Channel
)
SELECT
    id,
    idPersona,
    idEsercizio,
    ISNULL(NULLIF(LTRIM(RTRIM(descrizione)), ''), CONCAT('Pagamento-', id)),
    data,
    CAST(ISNULL(importo, 0) AS DECIMAL(18,4)),
    NULLIF(LTRIM(RTRIM(canale)), '')
FROM vzcondominio.Gestione.VZPagamento;

-- ------------------------------------------------------------
-- 11) VZ_Budget (VZPreventivo)
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_Budget;

INSERT INTO stg.VZ_Budget (
    VZFiscalYearId, VZMacroCategoryId, VZCategoryId, VZBuildingId, VZStaircaseId, Amount
)
SELECT
    idEsercizio,
    idMacroCategoria,
    idCategoria,
    idEdificio,
    idScala,
    CAST(ISNULL(importo, 0) AS DECIMAL(18,4))
FROM vzcondominio.Gestione.VZPreventivo;

-- Normalizzazione BuildingId su VZ_Budget:
-- quando nello stesso VZFiscalYearId esiste un solo BuildingId valido (>0),
-- propaga quel valore alle righe con BuildingId=0.
-- Per evitare violazioni PK:
-- 1) somma importi su eventuali righe duplicate
-- 2) elimina la riga BuildingId=0 duplicata
-- 3) aggiorna le righe rimanenti BuildingId=0
;WITH d AS (
    SELECT
        VZFiscalYearId,
        MIN(CASE WHEN VZBuildingId > 0 THEN VZBuildingId END) AS DerivedBuildingId,
        COUNT(DISTINCT CASE WHEN VZBuildingId > 0 THEN VZBuildingId END) AS DistinctNonZeroBuilding
    FROM stg.VZ_Budget
    GROUP BY VZFiscalYearId
), merge_pairs AS (
    SELECT
        z.VZFiscalYearId,
        z.VZMacroCategoryId,
        z.VZCategoryId,
        z.VZStaircaseId,
        z.Amount AS ZeroAmount,
        d.DerivedBuildingId
    FROM stg.VZ_Budget z
    INNER JOIN d
        ON d.VZFiscalYearId = z.VZFiscalYearId
       AND d.DistinctNonZeroBuilding = 1
       AND d.DerivedBuildingId IS NOT NULL
    INNER JOIN stg.VZ_Budget t
        ON t.VZFiscalYearId = z.VZFiscalYearId
       AND t.VZMacroCategoryId = z.VZMacroCategoryId
       AND t.VZCategoryId = z.VZCategoryId
       AND t.VZBuildingId = d.DerivedBuildingId
       AND t.VZStaircaseId = z.VZStaircaseId
    WHERE z.VZBuildingId = 0
)
UPDATE t
   SET t.Amount = t.Amount + m.ZeroAmount
FROM stg.VZ_Budget t
INNER JOIN merge_pairs m
    ON m.VZFiscalYearId = t.VZFiscalYearId
   AND m.VZMacroCategoryId = t.VZMacroCategoryId
   AND m.VZCategoryId = t.VZCategoryId
   AND m.DerivedBuildingId = t.VZBuildingId
   AND m.VZStaircaseId = t.VZStaircaseId;

;WITH d AS (
    SELECT
        VZFiscalYearId,
        MIN(CASE WHEN VZBuildingId > 0 THEN VZBuildingId END) AS DerivedBuildingId,
        COUNT(DISTINCT CASE WHEN VZBuildingId > 0 THEN VZBuildingId END) AS DistinctNonZeroBuilding
    FROM stg.VZ_Budget
    GROUP BY VZFiscalYearId
)
DELETE z
FROM stg.VZ_Budget z
INNER JOIN d
    ON d.VZFiscalYearId = z.VZFiscalYearId
   AND d.DistinctNonZeroBuilding = 1
   AND d.DerivedBuildingId IS NOT NULL
INNER JOIN stg.VZ_Budget t
    ON t.VZFiscalYearId = z.VZFiscalYearId
   AND t.VZMacroCategoryId = z.VZMacroCategoryId
   AND t.VZCategoryId = z.VZCategoryId
   AND t.VZBuildingId = d.DerivedBuildingId
   AND t.VZStaircaseId = z.VZStaircaseId
WHERE z.VZBuildingId = 0;

;WITH d AS (
    SELECT
        VZFiscalYearId,
        MIN(CASE WHEN VZBuildingId > 0 THEN VZBuildingId END) AS DerivedBuildingId,
        COUNT(DISTINCT CASE WHEN VZBuildingId > 0 THEN VZBuildingId END) AS DistinctNonZeroBuilding
    FROM stg.VZ_Budget
    GROUP BY VZFiscalYearId
)
UPDATE b
   SET b.VZBuildingId = d.DerivedBuildingId
FROM stg.VZ_Budget b
INNER JOIN d
    ON d.VZFiscalYearId = b.VZFiscalYearId
WHERE b.VZBuildingId = 0
  AND d.DistinctNonZeroBuilding = 1
  AND d.DerivedBuildingId IS NOT NULL;

-- ------------------------------------------------------------
-- 11b) VZ_ExpenseMacroCategory (VZMacroCategoriaSpesa)
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_ExpenseMacroCategory;

INSERT INTO stg.VZ_ExpenseMacroCategory (VZMacroCategoryId, Name)
SELECT
    id,
    ISNULL(NULLIF(LTRIM(RTRIM(descrizione)), ''), CONCAT('Macro-', id))
FROM vzcondominio.Gestione.VZMacroCategoriaSpesa;

-- ------------------------------------------------------------
-- 11c) VZ_ExpenseCategory (VZCategoriaSpesa)
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_ExpenseCategory;

INSERT INTO stg.VZ_ExpenseCategory (VZCategoryId, VZMacroCategoryId, Name)
SELECT
    id,
    idMacroCategoria,
    ISNULL(NULLIF(LTRIM(RTRIM(descrizione)), ''), CONCAT('Categoria-', id))
FROM vzcondominio.Gestione.VZCategoriaSpesa;

-- ------------------------------------------------------------
-- 12) VZ_BudgetInstallment (VZRataPreventivo)
-- ------------------------------------------------------------
TRUNCATE TABLE stg.VZ_BudgetInstallment;

INSERT INTO stg.VZ_BudgetInstallment (VZFiscalYearId, DueDate, Description)
SELECT
    idEsercizio,
    data,
    ISNULL(NULLIF(LTRIM(RTRIM(descrizione)), ''), CONCAT('Rata-', FORMAT(data, 'yyyy-MM-dd')))
FROM vzcondominio.Gestione.VZRataPreventivo;

-- ------------------------------------------------------------
-- 13) Popolazione MigrationTenantMap_VZ
-- Prerequisito finale: devi fornire il TenantId reale per ogni condominio.
-- Esegui questa query per vedere i condomini e poi compila la tabella:
-- ------------------------------------------------------------
SELECT c.VZId, c.Name, c.TaxCode
FROM stg.VZ_Condominium c
LEFT JOIN dbo.MigrationTenantMap_VZ m ON m.VZCondominioId = c.VZId
WHERE m.VZCondominioId IS NULL
ORDER BY c.VZId;

-- Poi inserisci una riga per ogni condominio con il TenantId corretto.
-- Esempio per tutti i condomini sullo stesso tenant:
--
--   DECLARE @TenantId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000'; -- <-- sostituisci
--   INSERT INTO dbo.MigrationTenantMap_VZ (VZCondominioId, TenantId, Notes)
--   SELECT VZId, @TenantId, Name
--   FROM stg.VZ_Condominium
--   WHERE VZId NOT IN (SELECT VZCondominioId FROM dbo.MigrationTenantMap_VZ);

-- ------------------------------------------------------------
-- 14) Riepilogo conteggi staging vs sorgente
-- ------------------------------------------------------------
SELECT 'VZ_Condominium'      AS Table_, COUNT(*) AS StgRows FROM stg.VZ_Condominium
UNION ALL SELECT 'VZ_Building',         COUNT(*) FROM stg.VZ_Building
UNION ALL SELECT 'VZ_Staircase',        COUNT(*) FROM stg.VZ_Staircase
UNION ALL SELECT 'VZ_Unit',             COUNT(*) FROM stg.VZ_Unit
UNION ALL SELECT 'VZ_Person',           COUNT(*) FROM stg.VZ_Person
UNION ALL SELECT 'VZ_Supplier',         COUNT(*) FROM stg.VZ_Supplier
UNION ALL SELECT 'VZ_FiscalYear',       COUNT(*) FROM stg.VZ_FiscalYear
UNION ALL SELECT 'VZ_Invoice',          COUNT(*) FROM stg.VZ_Invoice
UNION ALL SELECT 'VZ_Expense',          COUNT(*) FROM stg.VZ_Expense
UNION ALL SELECT 'VZ_Payment',          COUNT(*) FROM stg.VZ_Payment
UNION ALL SELECT 'VZ_Budget',           COUNT(*) FROM stg.VZ_Budget
UNION ALL SELECT 'VZ_ExpenseMacroCategory', COUNT(*) FROM stg.VZ_ExpenseMacroCategory
UNION ALL SELECT 'VZ_ExpenseCategory',  COUNT(*) FROM stg.VZ_ExpenseCategory
UNION ALL SELECT 'VZ_BudgetInstallment',COUNT(*) FROM stg.VZ_BudgetInstallment;
GO
