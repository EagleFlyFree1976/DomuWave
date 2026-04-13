-- ============================================================
-- Seed: Condominio "Residenza Il Giardino" — dati demo
-- 9 appartamenti (2 scale), esercizio 2025 aperto con dati realistici
--
-- Scenario: condominio anni '90, 2 scale (A e B), 4-5 piani,
-- ascensore, portineria part-time, un moroso (D'Angelo)
--
-- Prerequisiti:
--   - Esiste almeno un Tenant nel DB
--   - Tabelle lookup popolate (ExpenseTypeLookup, BudgetStatusLookup, ecc.)
--   - Piano dei conti (ChartOfAccounts) già creato per il tenant
--     (se non esiste, BudgetItem e Expense non vengono inseriti)
--
-- NHibernate hilo (max_lo = 10):
--   ID = next_hi * (max_lo + 1) + lo   (lo = 0..10)
--   Ogni blocco: prenota next_hi corrente, incrementa di 1.
--   Alla fine: UPDATE hibernate_unique_key per avanzare i contatori.
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
BEGIN TRANSACTION;

DECLARE @Now          DATETIME2        = SYSUTCDATETIME();
DECLARE @UserId       INT              = 1;
DECLARE @UserName     NVARCHAR(200)    = N'Sistema Demo';
DECLARE @MaxLo        INT              = 10;

-- Prende il primo tenant attivo
DECLARE @TenantId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Tenant WHERE IsDeleted = 0 ORDER BY Id);
IF @TenantId IS NULL
    RAISERROR('Nessun Tenant trovato. Creare prima un Tenant.', 16, 1);

-- ============================================================
-- HILO: lettura contatori correnti
-- ============================================================
DECLARE @HiCondominium          INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'Condominium');
DECLARE @HiCondominiumAddress   INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'CondominiumAddress');
DECLARE @HiRealEstateUnit       INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'RealEstateUnit');
DECLARE @HiMillesimalTable      INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'MillesimalTable');
DECLARE @HiUnitMillesimal       INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'UnitMillesimal');
DECLARE @HiSupplier             INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'Supplier');
DECLARE @HiFiscalYear           INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'FiscalYear');
DECLARE @HiBudget               INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'Budget');
DECLARE @HiBudgetItem           INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'BudgetItem');
DECLARE @HiCondInst             INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'CondominiumInstallment');
DECLARE @HiCondFee              INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'CondominiumFee');
DECLARE @HiExpense              INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'Expense');
DECLARE @HiUnitOwner            INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'UnitOwner');
DECLARE @HiUnitOpeningBalance   INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'UnitOpeningBalance');

-- ============================================================
-- IDs — Condominium & Address
-- ============================================================
DECLARE @CondId  INT = @HiCondominium        * (@MaxLo + 1);  SET @HiCondominium        += 1;
DECLARE @AddrId  INT = @HiCondominiumAddress * (@MaxLo + 1);  SET @HiCondominiumAddress += 1;

-- ============================================================
-- IDs — Units (9 appartamenti)
--   Scala A: 5 piani (A01..A05)
--   Scala B: 4 piani (B01..B04)
-- ============================================================
DECLARE @UA01 INT = @HiRealEstateUnit * (@MaxLo + 1) + 0;
DECLARE @UA02 INT = @HiRealEstateUnit * (@MaxLo + 1) + 1;
DECLARE @UA03 INT = @HiRealEstateUnit * (@MaxLo + 1) + 2;
DECLARE @UA04 INT = @HiRealEstateUnit * (@MaxLo + 1) + 3;
DECLARE @UA05 INT = @HiRealEstateUnit * (@MaxLo + 1) + 4;
DECLARE @UB01 INT = @HiRealEstateUnit * (@MaxLo + 1) + 5;
DECLARE @UB02 INT = @HiRealEstateUnit * (@MaxLo + 1) + 6;
DECLARE @UB03 INT = @HiRealEstateUnit * (@MaxLo + 1) + 7;
DECLARE @UB04 INT = @HiRealEstateUnit * (@MaxLo + 1) + 8;
SET @HiRealEstateUnit += 1;

-- ============================================================
-- IDs — UnitOwner (uno per appartamento, 9 totali)
-- ============================================================
DECLARE @OWA01 INT = @HiUnitOwner * (@MaxLo + 1) + 0;
DECLARE @OWA02 INT = @HiUnitOwner * (@MaxLo + 1) + 1;
DECLARE @OWA03 INT = @HiUnitOwner * (@MaxLo + 1) + 2;
DECLARE @OWA04 INT = @HiUnitOwner * (@MaxLo + 1) + 3;
DECLARE @OWA05 INT = @HiUnitOwner * (@MaxLo + 1) + 4;
DECLARE @OWB01 INT = @HiUnitOwner * (@MaxLo + 1) + 5;
DECLARE @OWB02 INT = @HiUnitOwner * (@MaxLo + 1) + 6;
DECLARE @OWB03 INT = @HiUnitOwner * (@MaxLo + 1) + 7;
DECLARE @OWB04 INT = @HiUnitOwner * (@MaxLo + 1) + 8;
SET @HiUnitOwner += 1;

-- ============================================================
-- IDs — MillesimalTable + UnitMillesimal (9 righe)
-- ============================================================
DECLARE @MtId INT = @HiMillesimalTable * (@MaxLo + 1);        SET @HiMillesimalTable += 1;

DECLARE @MMA01 INT = @HiUnitMillesimal * (@MaxLo + 1) + 0;
DECLARE @MMA02 INT = @HiUnitMillesimal * (@MaxLo + 1) + 1;
DECLARE @MMA03 INT = @HiUnitMillesimal * (@MaxLo + 1) + 2;
DECLARE @MMA04 INT = @HiUnitMillesimal * (@MaxLo + 1) + 3;
DECLARE @MMA05 INT = @HiUnitMillesimal * (@MaxLo + 1) + 4;
DECLARE @MMB01 INT = @HiUnitMillesimal * (@MaxLo + 1) + 5;
DECLARE @MMB02 INT = @HiUnitMillesimal * (@MaxLo + 1) + 6;
DECLARE @MMB03 INT = @HiUnitMillesimal * (@MaxLo + 1) + 7;
DECLARE @MMB04 INT = @HiUnitMillesimal * (@MaxLo + 1) + 8;
SET @HiUnitMillesimal += 1;

-- ============================================================
-- IDs — Suppliers (4)
-- ============================================================
DECLARE @Sup1 INT = @HiSupplier * (@MaxLo + 1) + 0;  -- manutenzione generale
DECLARE @Sup2 INT = @HiSupplier * (@MaxLo + 1) + 1;  -- pulizie
DECLARE @Sup3 INT = @HiSupplier * (@MaxLo + 1) + 2;  -- amministratore
DECLARE @Sup4 INT = @HiSupplier * (@MaxLo + 1) + 3;  -- manutenzione ascensore
SET @HiSupplier += 1;

-- ============================================================
-- IDs — FiscalYear 2025 (Open)
-- ============================================================
DECLARE @FyId INT = @HiFiscalYear * (@MaxLo + 1);             SET @HiFiscalYear += 1;

-- ============================================================
-- IDs — Budget Preventivo (Approved)
-- ============================================================
DECLARE @BudPrev INT = @HiBudget * (@MaxLo + 1) + 0;
SET @HiBudget += 1;

-- ============================================================
-- IDs — BudgetItem Preventivo (7 voci)
-- ============================================================
DECLARE @BIP1 INT = @HiBudgetItem * (@MaxLo + 1) + 0;
DECLARE @BIP2 INT = @HiBudgetItem * (@MaxLo + 1) + 1;
DECLARE @BIP3 INT = @HiBudgetItem * (@MaxLo + 1) + 2;
DECLARE @BIP4 INT = @HiBudgetItem * (@MaxLo + 1) + 3;
DECLARE @BIP5 INT = @HiBudgetItem * (@MaxLo + 1) + 4;
DECLARE @BIP6 INT = @HiBudgetItem * (@MaxLo + 1) + 5;
DECLARE @BIP7 INT = @HiBudgetItem * (@MaxLo + 1) + 6;
SET @HiBudgetItem += 1;

-- ============================================================
-- IDs — CondominiumInstallment (3 rate: Q1+Q2 pagate, Q3 emessa)
-- ============================================================
DECLARE @CI1 INT = @HiCondInst * (@MaxLo + 1) + 0;
DECLARE @CI2 INT = @HiCondInst * (@MaxLo + 1) + 1;
DECLARE @CI3 INT = @HiCondInst * (@MaxLo + 1) + 2;
DECLARE @CI4 INT = @HiCondInst * (@MaxLo + 1) + 3;
SET @HiCondInst += 1;

-- ============================================================
-- IDs — Expenses (5 spese)
-- ============================================================
DECLARE @EXP1 BIGINT = CAST(@HiExpense AS BIGINT) * (@MaxLo + 1) + 0;
DECLARE @EXP2 BIGINT = CAST(@HiExpense AS BIGINT) * (@MaxLo + 1) + 1;
DECLARE @EXP3 BIGINT = CAST(@HiExpense AS BIGINT) * (@MaxLo + 1) + 2;
DECLARE @EXP4 BIGINT = CAST(@HiExpense AS BIGINT) * (@MaxLo + 1) + 3;
DECLARE @EXP5 BIGINT = CAST(@HiExpense AS BIGINT) * (@MaxLo + 1) + 4;
SET @HiExpense += 1;

-- ============================================================
-- Lookup dei conti dal piano dei conti del tenant
-- (usati dopo la creazione del condominio — vengono ricercati per tipo)
-- ============================================================
-- Recuperiamo i conti DOPO la creazione del condominio (sotto).
-- Li usiamo per BudgetItem e Expense. Se non esistono → NULL e skip.
DECLARE @AccManutenzione    INT = NULL;
DECLARE @AccAscensore       INT = NULL;
DECLARE @AccPulizie         INT = NULL;
DECLARE @AccAmministrazione INT = NULL;
DECLARE @AccAssicurazione   INT = NULL;
DECLARE @AccUtenze          INT = NULL;
DECLARE @AccFondoRiserva    INT = NULL;

-- ============================================================
-- INSERIMENTI
-- ============================================================

-- ── Condominium ──────────────────────────────────────────────
INSERT INTO Condominium (Id, TenantId, Name, Code, TaxCode, NumberOfUnits, NumberOfStaircases,
    TotalMillesimal, InstallmentFrequency, InstallmentDueDay, IsActive,
    CreatedById, CreatedByFullName, IsDeleted, CreationDate)
VALUES (@CondId, @TenantId, N'Residenza Il Giardino', N'GRD-001', N'80134590157',
    9, 2, 1000.0000, 4, 10, 1,
    @UserId, @UserName, 0, @Now);

-- ── CondominiumAddress ───────────────────────────────────────
-- Colonne reali: Street, StreetNumber, PostalCode, City, Province, Country
INSERT INTO CondominiumAddress (Id, TenantId, CondominiumId, Street, StreetNumber, PostalCode, City, Province, Country,
    CreatedById, CreatedByFullName, IsDeleted, CreationDate)
VALUES (@AddrId, @TenantId, @CondId, N'Via del Giardino Botanico', N'7', N'10126', N'Torino', N'TO', N'IT',
    @UserId, @UserName, 0, @Now);

-- ── Recupera conti del piano dei conti creati per questo condominio ──
-- (il piano dei conti viene creato dall'applicazione al momento della creazione del condominio
--  oppure tramite il template; qui lo leggiamo se già esiste)
SELECT @AccManutenzione    = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0 AND Code LIKE '5.1%';
SELECT @AccAscensore       = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0 AND Code LIKE '5.2%';
SELECT @AccPulizie         = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0 AND Code LIKE '5.3%';
SELECT @AccAmministrazione = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0 AND Code LIKE '5.4%';
SELECT @AccAssicurazione   = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0 AND Code LIKE '5.5%';
SELECT @AccUtenze          = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0 AND Code LIKE '5.6%';
SELECT @AccFondoRiserva    = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0 AND Code LIKE '5.7%';

-- Fallback: se i conti per codice non esistono, prende i primi disponibili per tipo
IF @AccManutenzione    IS NULL SELECT @AccManutenzione    = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0;
IF @AccAscensore       IS NULL SET @AccAscensore       = @AccManutenzione;
IF @AccPulizie         IS NULL SET @AccPulizie         = @AccManutenzione;
IF @AccAmministrazione IS NULL SET @AccAmministrazione = @AccManutenzione;
IF @AccAssicurazione   IS NULL SET @AccAssicurazione   = @AccManutenzione;
IF @AccUtenze          IS NULL SET @AccUtenze          = @AccManutenzione;
IF @AccFondoRiserva    IS NULL SET @AccFondoRiserva    = @AccManutenzione;

-- ── RealEstateUnit ───────────────────────────────────────────
-- Colonne reali: Staircase, Floor, InternalNumber, AreaSqm, Rooms,
--   UnitType (NOT NULL), OccupancyStatus (NOT NULL), NumeroAbitanti (NOT NULL),
--   DisplayName, IsActive
INSERT INTO RealEstateUnit (Id, TenantId, CondominiumId,
    Staircase, Floor, InternalNumber, AreaSqm, Rooms,
    UnitType, OccupancyStatus, NumeroAbitanti, DisplayName, IsActive,
    IsDeleted, CreatedById, CreatedByFullName, CreationDate)
VALUES
(@UA01, @TenantId, @CondId, N'A', 1, N'A01',  92.00, 3, N'Residenziale', N'Occupato',  2, N'Esposito',   1, 0, @UserId, @UserName, @Now),
(@UA02, @TenantId, @CondId, N'A', 2, N'A02', 118.00, 4, N'Residenziale', N'Occupato',  3, N'Ferrero',    1, 0, @UserId, @UserName, @Now),
(@UA03, @TenantId, @CondId, N'A', 3, N'A03',  88.00, 3, N'Residenziale', N'Occupato',  2, N'D''Angelo',  1, 0, @UserId, @UserName, @Now),
(@UA04, @TenantId, @CondId, N'A', 4, N'A04',  72.00, 2, N'Residenziale', N'Occupato',  1, N'Martinelli', 1, 0, @UserId, @UserName, @Now),
(@UA05, @TenantId, @CondId, N'A', 5, N'A05',  95.00, 3, N'Residenziale', N'Occupato',  2, N'Santoro',    1, 0, @UserId, @UserName, @Now),
(@UB01, @TenantId, @CondId, N'B', 1, N'B01', 125.00, 4, N'Residenziale', N'Occupato',  4, N'De Luca',    1, 0, @UserId, @UserName, @Now),
(@UB02, @TenantId, @CondId, N'B', 2, N'B02',  85.00, 3, N'Residenziale', N'Occupato',  2, N'Genovese',   1, 0, @UserId, @UserName, @Now),
(@UB03, @TenantId, @CondId, N'B', 3, N'B03', 115.00, 4, N'Residenziale', N'Occupato',  3, N'Amato',      1, 0, @UserId, @UserName, @Now),
(@UB04, @TenantId, @CondId, N'B', 4, N'B04',  68.00, 2, N'Residenziale', N'Occupato',  1, N'Vitale',     1, 0, @UserId, @UserName, @Now);
-- Totale superficie: 92+118+88+72+95+125+85+115+68 = 858 mq

-- ── UnitOwner ────────────────────────────────────────────────
-- Colonne reali: UnitId (FK), OwnerType (= Name fisico), LastName, FirstName,
--   OwnershipQuota, StartDate, IsResident, IsAccessEnabled, IsActive, UserId
-- NB: NON esiste CondominiumId in UnitOwner
DECLARE @StartOwnership DATE = '2020-01-01';

INSERT INTO UnitOwner (Id, TenantId, UnitId,
    OwnerType, LastName, FirstName,
    OwnershipQuota, StartDate, IsResident, IsActive, IsAccessEnabled, UserId,
    IsDeleted, CreatedById, CreatedByFullName, CreationDate)
VALUES
(@OWA01, @TenantId, @UA01, N'Proprietario', N'Esposito',   N'Carla',     1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWA02, @TenantId, @UA02, N'Proprietario', N'Ferrero',    N'Alberto',   1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWA03, @TenantId, @UA03, N'Proprietario', N'D''Angelo',  N'Michele',   1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWA04, @TenantId, @UA04, N'Proprietario', N'Martinelli', N'Giovanna',  1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWA05, @TenantId, @UA05, N'Proprietario', N'Santoro',    N'Filippo',   1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWB01, @TenantId, @UB01, N'Proprietario', N'De Luca',    N'Sergio',    1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWB02, @TenantId, @UB02, N'Proprietario', N'Genovese',   N'Patrizia',  1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWB03, @TenantId, @UB03, N'Proprietario', N'Amato',      N'Vincenzo',  1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWB04, @TenantId, @UB04, N'Proprietario', N'Vitale',     N'Rossella',  1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now);

-- ── MillesimalTable ──────────────────────────────────────────
-- Colonne reali: Name, Code, TotalMillesimal, IsActive, IsEnabled, IsDraft
INSERT INTO MillesimalTable (Id, TenantId, CondominiumId, Name, Code,
    TotalMillesimal, IsActive, IsEnabled, IsDraft,
    IsDeleted, CreatedById, CreatedByFullName, CreationDate)
VALUES (@MtId, @TenantId, @CondId, N'Tabella Millesimale Generale', N'TMG',
    1000.0000, 1, 1, 0,
    0, @UserId, @UserName, @Now);

-- ── UnitMillesimal ───────────────────────────────────────────
-- Millesimi proporzionali alla superficie (858 mq → 1000 millesimi)
INSERT INTO UnitMillesimal (Id, TenantId, MillesimalTableId, UnitId, Millesimal,
    IsDeleted, CreatedById, CreatedByFullName, CreationDate)
VALUES
(@MMA01, @TenantId, @MtId, @UA01, 107.2300, 0, @UserId, @UserName, @Now),  --  92 mq
(@MMA02, @TenantId, @MtId, @UA02, 137.5300, 0, @UserId, @UserName, @Now),  -- 118 mq
(@MMA03, @TenantId, @MtId, @UA03, 102.5600, 0, @UserId, @UserName, @Now),  --  88 mq
(@MMA04, @TenantId, @MtId, @UA04,  83.9200, 0, @UserId, @UserName, @Now),  --  72 mq
(@MMA05, @TenantId, @MtId, @UA05, 110.6900, 0, @UserId, @UserName, @Now),  --  95 mq
(@MMB01, @TenantId, @MtId, @UB01, 145.6900, 0, @UserId, @UserName, @Now),  -- 125 mq
(@MMB02, @TenantId, @MtId, @UB02,  99.0700, 0, @UserId, @UserName, @Now),  --  85 mq
(@MMB03, @TenantId, @MtId, @UB03, 134.0800, 0, @UserId, @UserName, @Now),  -- 115 mq
(@MMB04, @TenantId, @MtId, @UB04,  79.2300, 0, @UserId, @UserName, @Now);  --  68 mq
-- Totale: 107.23+137.53+102.56+83.92+110.69+145.69+99.07+134.08+79.23 = 1000.00

-- ── Suppliers ────────────────────────────────────────────────
-- Colonne reali: CompanyName (= Name fisico), VatNumber, IsActive
INSERT INTO Supplier (Id, TenantId, CompanyName, VatNumber, IsActive,
    IsDeleted, CreatedById, CreatedByFullName, CreationDate)
VALUES
(@Sup1, @TenantId, N'Edilmast Srl',                 N'IT02341870013', 1, 0, @UserId, @UserName, @Now),
(@Sup2, @TenantId, N'Verde & Pulito Coop',           N'IT05678920011', 1, 0, @UserId, @UserName, @Now),
(@Sup3, @TenantId, N'Studio Bianchi & Associati',    N'IT11234560016', 1, 0, @UserId, @UserName, @Now),
(@Sup4, @TenantId, N'Otis Ascensori Srl',            N'IT04512370014', 1, 0, @UserId, @UserName, @Now);

-- ── FiscalYear 2025 (Open) ───────────────────────────────────
INSERT INTO FiscalYear (FiscalYearId, TenantId, CondominiumId, Code, Description,
    StartDate, EndDate, StatusId, IsActive, ClosedDate,
    CreatedById, CreatedByFullName, IsDeleted, CreationDate)
VALUES (@FyId, @TenantId, @CondId, N'2025', N'Esercizio 2025',
    '2025-01-01', '2025-12-31',
    2, 1, NULL,
    @UserId, @UserName, 0, @Now);

-- ── Budget Preventivo 2025 (Approved) ───────────────────────
-- Colonne reali: Type, StatusId (FK lookup), TotalIncome, TotalExpenses, Notes (=Description)
-- Totale preventivato: 18.600 €
INSERT INTO Budget (Id, TenantId, CondominiumId, FiscalYearId, Type, StatusId,
    TotalIncome, TotalExpenses, Notes,
    CreatedById, CreatedByFullName, IsDeleted, CreationDate)
VALUES (@BudPrev, @TenantId, @CondId, @FyId, 1, 2,
    18600.00, 18600.00,
    N'Preventivo approvato assemblea 22/01/2025',
    @UserId, @UserName, 0, @Now);

-- ── BudgetItem Preventivo (7 voci) ───────────────────────────
-- Colonne reali: AccountId (NOT NULL FK), Description (= Name fisico), Amount, AmountPaid, Notes
-- Se non esiste un piano dei conti → skip
IF @AccManutenzione IS NOT NULL
BEGIN
    INSERT INTO BudgetItem (Id, TenantId, BudgetId, AccountId, Description, Amount, AmountPaid,
        IsDeleted, CreatedById, CreatedByFullName, CreationDate)
    VALUES
    (@BIP1, @TenantId, @BudPrev, @AccManutenzione,    N'Manutenzione ordinaria parti comuni',  2800.00, 0.00, 0, @UserId, @UserName, @Now),
    (@BIP2, @TenantId, @BudPrev, @AccAscensore,       N'Manutenzione e revisione ascensori',   3600.00, 0.00, 0, @UserId, @UserName, @Now),
    (@BIP3, @TenantId, @BudPrev, @AccPulizie,         N'Servizio pulizie scale e giardino',    3200.00, 0.00, 0, @UserId, @UserName, @Now),
    (@BIP4, @TenantId, @BudPrev, @AccAmministrazione, N'Onorario amministratore',              2100.00, 0.00, 0, @UserId, @UserName, @Now),
    (@BIP5, @TenantId, @BudPrev, @AccAssicurazione,   N'Assicurazione fabbricato',             1700.00, 0.00, 0, @UserId, @UserName, @Now),
    (@BIP6, @TenantId, @BudPrev, @AccUtenze,          N'Utenze (luce scale, acqua comune)',    3200.00, 0.00, 0, @UserId, @UserName, @Now),
    (@BIP7, @TenantId, @BudPrev, @AccFondoRiserva,    N'Fondo riserva straordinaria',          2000.00, 0.00, 0, @UserId, @UserName, @Now);
END
ELSE
    PRINT N'ATTENZIONE: Piano dei conti non trovato — BudgetItem non inseriti. Crea prima il piano dei conti per il condominio.';

-- ── CondominiumInstallment: 4 rate trimestrali ───────────────
-- Colonne reali: Description (= Name fisico), InstallmentNumber, DueDate, TotalAmount, StatusId (FK)
-- Quota annuale = 18.600 € → 4 rate da 4.650 €
-- Q1+Q2 = Paid (3), Q3 = Issued (2), Q4 = Draft (1)
INSERT INTO CondominiumInstallment (Id, TenantId, CondominiumId, BudgetId, FiscalYearId,
    Description, InstallmentNumber, DueDate, TotalAmount, StatusId,
    IsDeleted, CreatedById, CreatedByFullName, CreationDate)
VALUES
(@CI1, @TenantId, @CondId, @BudPrev, @FyId, N'1ª Rata 2025', 1, '2025-03-10', 4650.00, 3, 0, @UserId, @UserName, @Now),
(@CI2, @TenantId, @CondId, @BudPrev, @FyId, N'2ª Rata 2025', 2, '2025-06-10', 4650.00, 3, 0, @UserId, @UserName, @Now),
(@CI3, @TenantId, @CondId, @BudPrev, @FyId, N'3ª Rata 2025', 3, '2025-09-10', 4650.00, 2, 0, @UserId, @UserName, @Now),
(@CI4, @TenantId, @CondId, @BudPrev, @FyId, N'4ª Rata 2025', 4, '2025-12-10', 4650.00, 1, 0, @UserId, @UserName, @Now);

-- ── CondominiumFee: quote per unità ──────────────────────────
-- Quota per unità per rata = 4650 * (millesimi / 1000)
--   A01 Esposito   (107.23): 498.62
--   A02 Ferrero    (137.53): 639.51
--   A03 D'Angelo   (102.56): 476.90  ← MOROSO: Q3 non pagata
--   A04 Martinelli  (83.92): 390.23
--   A05 Santoro    (110.69): 514.71
--   B01 De Luca    (145.69): 677.46
--   B02 Genovese    (99.07): 460.67
--   B03 Amato      (134.08): 623.47
--   B04 Vitale      (79.23): 368.32
-- Solo Q1, Q2, Q3 emesse (Q4 = Draft → nessuna fee)

DECLARE @FeeHi BIGINT = CAST(@HiCondFee AS BIGINT);

IF OBJECT_ID('tempdb..#fees') IS NOT NULL DROP TABLE #fees;
CREATE TABLE #fees (
    Id BIGINT, UnitId INT, InstId INT,
    AmountDue DECIMAL(18,4), AmountPaid DECIMAL(18,4)
);

-- A01 Esposito: Q1+Q2+Q3 tutti pagati
INSERT INTO #fees VALUES
(@FeeHi*11+0, @UA01, @CI1, 498.62, 498.62),
(@FeeHi*11+1, @UA01, @CI2, 498.62, 498.62),
(@FeeHi*11+2, @UA01, @CI3, 498.62, 498.62);
SET @FeeHi += 1;

-- A02 Ferrero: Q1+Q2+Q3 tutti pagati
INSERT INTO #fees VALUES
(@FeeHi*11+0, @UA02, @CI1, 639.51, 639.51),
(@FeeHi*11+1, @UA02, @CI2, 639.51, 639.51),
(@FeeHi*11+2, @UA02, @CI3, 639.51, 639.51);
SET @FeeHi += 1;

-- A03 D'Angelo: Q1+Q2 pagati, Q3 non pagata (moroso)
INSERT INTO #fees VALUES
(@FeeHi*11+0, @UA03, @CI1, 476.90, 476.90),
(@FeeHi*11+1, @UA03, @CI2, 476.90, 476.90),
(@FeeHi*11+2, @UA03, @CI3, 476.90,   0.00);  -- non pagata
SET @FeeHi += 1;

-- A04 Martinelli: Q1+Q2+Q3 tutti pagati
INSERT INTO #fees VALUES
(@FeeHi*11+0, @UA04, @CI1, 390.23, 390.23),
(@FeeHi*11+1, @UA04, @CI2, 390.23, 390.23),
(@FeeHi*11+2, @UA04, @CI3, 390.23, 390.23);
SET @FeeHi += 1;

-- A05 Santoro: Q1+Q2+Q3 tutti pagati
INSERT INTO #fees VALUES
(@FeeHi*11+0, @UA05, @CI1, 514.71, 514.71),
(@FeeHi*11+1, @UA05, @CI2, 514.71, 514.71),
(@FeeHi*11+2, @UA05, @CI3, 514.71, 514.71);
SET @FeeHi += 1;

-- B01 De Luca: Q1+Q2+Q3 tutti pagati
INSERT INTO #fees VALUES
(@FeeHi*11+0, @UB01, @CI1, 677.46, 677.46),
(@FeeHi*11+1, @UB01, @CI2, 677.46, 677.46),
(@FeeHi*11+2, @UB01, @CI3, 677.46, 677.46);
SET @FeeHi += 1;

-- B02 Genovese: Q1+Q2+Q3 tutti pagati
INSERT INTO #fees VALUES
(@FeeHi*11+0, @UB02, @CI1, 460.67, 460.67),
(@FeeHi*11+1, @UB02, @CI2, 460.67, 460.67),
(@FeeHi*11+2, @UB02, @CI3, 460.67, 460.67);
SET @FeeHi += 1;

-- B03 Amato: Q1+Q2+Q3 tutti pagati
INSERT INTO #fees VALUES
(@FeeHi*11+0, @UB03, @CI1, 623.47, 623.47),
(@FeeHi*11+1, @UB03, @CI2, 623.47, 623.47),
(@FeeHi*11+2, @UB03, @CI3, 623.47, 623.47);
SET @FeeHi += 1;

-- B04 Vitale: Q1+Q2+Q3 tutti pagati
INSERT INTO #fees VALUES
(@FeeHi*11+0, @UB04, @CI1, 368.32, 368.32),
(@FeeHi*11+1, @UB04, @CI2, 368.32, 368.32),
(@FeeHi*11+2, @UB04, @CI3, 368.32, 368.32);
SET @FeeHi += 1;

INSERT INTO CondominiumFee (Id, TenantId, InstallmentId, UnitId, UserId,
    AmountDue, AmountPaid, Balance, PaymentStatus,
    IsDeleted, CreatedById, CreatedByFullName, CreationDate)
SELECT f.Id, @TenantId, f.InstId, f.UnitId, 0,
    f.AmountDue, f.AmountPaid,
    f.AmountDue - f.AmountPaid,
    CASE WHEN f.AmountPaid = 0         THEN N'ToPay'
         WHEN f.AmountPaid < f.AmountDue THEN N'PartiallyPaid'
         ELSE                               N'Paid' END,
    0, @UserId, @UserName, @Now
FROM #fees f;

-- ── Expenses (5 spese) ───────────────────────────────────────
-- Colonne reali: AccountId (NOT NULL), MillesimalTableId (NOT NULL),
--   Description (= Name fisico), PaymentStatusId (FK), ExpenseTypeId (FK),
--   ChargeabilityType (FK int = 0=Owner)
-- PaymentStatusId: 1=DaPagare, 2=Pagata
-- ExpenseTypeId valori lookup (da ExpenseTypeLookup): usare quelli presenti nel DB
-- ChargeabilityType: 0
IF @AccManutenzione IS NOT NULL
BEGIN
    -- Recupera gli ID ExpenseType e ChargeabilityType dal DB
    DECLARE @EtManutenzione    INT = (SELECT TOP 1 Id FROM ExpenseTypeLookup WHERE Id = 1);
    DECLARE @EtPulizie         INT = (SELECT TOP 1 Id FROM ExpenseTypeLookup WHERE Id = 2);
    DECLARE @EtAssicurazione   INT = (SELECT TOP 1 Id FROM ExpenseTypeLookup WHERE Id = 6);
    DECLARE @EtUtenze          INT = (SELECT TOP 1 Id FROM ExpenseTypeLookup WHERE Id = 4);
    DECLARE @PayPagata         INT = (SELECT TOP 1 Id FROM ExpensePaymentStatusLookup WHERE Id = 2);
    DECLARE @PayDaPagare       INT = (SELECT TOP 1 Id FROM ExpensePaymentStatusLookup WHERE Id = 1);
    DECLARE @ChargeOwner       INT = (SELECT TOP 1 Id FROM ChargeabilityTypeLookup   WHERE Id = 0);

    -- Se le lookup non hanno quei valori, usa 1 come fallback
    IF @EtManutenzione  IS NULL SET @EtManutenzione  = (SELECT TOP 1 Id FROM ExpenseTypeLookup ORDER BY Id);
    IF @EtPulizie       IS NULL SET @EtPulizie       = @EtManutenzione;
    IF @EtAssicurazione IS NULL SET @EtAssicurazione = @EtManutenzione;
    IF @EtUtenze        IS NULL SET @EtUtenze        = @EtManutenzione;
    IF @PayPagata       IS NULL SET @PayPagata       = (SELECT TOP 1 Id FROM ExpensePaymentStatusLookup ORDER BY Id);
    IF @PayDaPagare     IS NULL SET @PayDaPagare     = @PayPagata;
    IF @ChargeOwner     IS NULL SET @ChargeOwner     = (SELECT TOP 1 Id FROM ChargeabilityTypeLookup ORDER BY Id);

    INSERT INTO Expense (Id, TenantId, CondominiumId, FiscalYearId, AccountId, MillesimalTableId,
        SupplierId, Description, GrossAmount, NetAmount, VatAmount,
        ExpenseTypeId, PaymentStatusId, ChargeabilityType,
        DocumentDate, RegistrationDate,
        IsDeleted, CreatedById, CreatedByFullName, CreationDate)
    VALUES
    (@EXP1, @TenantId, @CondId, @FyId, @AccAscensore,       @MtId,
        @Sup4, N'Contratto manutenzione ascensori 2025',
        3600.00, 2950.82, 649.18, @EtManutenzione, @PayPagata, @ChargeOwner,
        '2025-01-05', '2025-01-07', 0, @UserId, @UserName, @Now),

    (@EXP2, @TenantId, @CondId, @FyId, @AccPulizie,         @MtId,
        @Sup2, N'Servizio pulizie I semestre',
        1600.00, 1311.48, 288.52, @EtPulizie, @PayPagata, @ChargeOwner,
        '2025-06-30', '2025-07-02', 0, @UserId, @UserName, @Now),

    (@EXP3, @TenantId, @CondId, @FyId, @AccAssicurazione,   @MtId,
        NULL, N'Assicurazione fabbricato 2025',
        1700.00, 1700.00, 0.00, @EtAssicurazione, @PayPagata, @ChargeOwner,
        '2025-01-10', '2025-01-12', 0, @UserId, @UserName, @Now),

    (@EXP4, @TenantId, @CondId, @FyId, @AccUtenze,          @MtId,
        NULL, N'Bollette energia elettrica scale I sem.',
        980.00, 803.28, 176.72, @EtUtenze, @PayPagata, @ChargeOwner,
        '2025-06-15', '2025-06-17', 0, @UserId, @UserName, @Now),

    (@EXP5, @TenantId, @CondId, @FyId, @AccManutenzione,    @MtId,
        @Sup1, N'Riparazione perdita idraulica piano 3 scala A',
        420.00, 344.26, 75.74, @EtManutenzione, @PayDaPagare, @ChargeOwner,
        '2025-08-20', '2025-08-22', 0, @UserId, @UserName, @Now);
END
ELSE
    PRINT N'ATTENZIONE: Piano dei conti non trovato — Expense non inserite.';

-- ── UnitOpeningBalance 2025 ───────────────────────────────────
-- Primo esercizio: OpeningBalance = 0 per tutti
-- Situazione al Q3 emesso (D'Angelo moroso su Q3):
--   RateAddebitate = Q1+Q2+Q3 per unità
--   RateIncassate  = Q1+Q2+Q3 per tutti tranne D'Angelo (solo Q1+Q2)
--   TotalMovements = RateAddebitate - RateIncassate
--   ClosingBalance = OpeningBalance + TotalMovements

DECLARE @UobHi INT = @HiUnitOpeningBalance;
DECLARE @UobId0 INT = @UobHi * (@MaxLo+1) + 0;
DECLARE @UobId1 INT = @UobHi * (@MaxLo+1) + 1;
DECLARE @UobId2 INT = @UobHi * (@MaxLo+1) + 2;
DECLARE @UobId3 INT = @UobHi * (@MaxLo+1) + 3;
DECLARE @UobId4 INT = @UobHi * (@MaxLo+1) + 4;
DECLARE @UobId5 INT = @UobHi * (@MaxLo+1) + 5;
DECLARE @UobId6 INT = @UobHi * (@MaxLo+1) + 6;
DECLARE @UobId7 INT = @UobHi * (@MaxLo+1) + 7;
DECLARE @UobId8 INT = @UobHi * (@MaxLo+1) + 8;
SET @HiUnitOpeningBalance += 1;

INSERT INTO UnitOpeningBalance (Id, TenantId, UnitId, FiscalYearId,
    OpeningBalance, RateAddebitate, RateIncassate, QuotaConsuntiva, SaldoConguaglio,
    TotalMovements, ClosingBalance,
    IsDeleted, CreatedById, CreatedByFullName, CreationDate)
VALUES
-- A01 Esposito:    3 × 498.62 = 1495.86, tutti pagati
(@UobId0, @TenantId, @UA01, @FyId, 0.00, 1495.86, 1495.86, 0.00, 0.00,   0.00,   0.00, 0, @UserId, @UserName, @Now),
-- A02 Ferrero:     3 × 639.51 = 1918.53, tutti pagati
(@UobId1, @TenantId, @UA02, @FyId, 0.00, 1918.53, 1918.53, 0.00, 0.00,   0.00,   0.00, 0, @UserId, @UserName, @Now),
-- A03 D'Angelo:    3 × 476.90 = 1430.70, Q1+Q2 pagati (953.80), Q3 non pagata → debito 476.90
(@UobId2, @TenantId, @UA03, @FyId, 0.00, 1430.70,  953.80, 0.00, 0.00, 476.90, 476.90, 0, @UserId, @UserName, @Now),
-- A04 Martinelli:  3 × 390.23 = 1170.69, tutti pagati
(@UobId3, @TenantId, @UA04, @FyId, 0.00, 1170.69, 1170.69, 0.00, 0.00,   0.00,   0.00, 0, @UserId, @UserName, @Now),
-- A05 Santoro:     3 × 514.71 = 1544.13, tutti pagati
(@UobId4, @TenantId, @UA05, @FyId, 0.00, 1544.13, 1544.13, 0.00, 0.00,   0.00,   0.00, 0, @UserId, @UserName, @Now),
-- B01 De Luca:     3 × 677.46 = 2032.38, tutti pagati
(@UobId5, @TenantId, @UB01, @FyId, 0.00, 2032.38, 2032.38, 0.00, 0.00,   0.00,   0.00, 0, @UserId, @UserName, @Now),
-- B02 Genovese:    3 × 460.67 = 1382.01, tutti pagati
(@UobId6, @TenantId, @UB02, @FyId, 0.00, 1382.01, 1382.01, 0.00, 0.00,   0.00,   0.00, 0, @UserId, @UserName, @Now),
-- B03 Amato:       3 × 623.47 = 1870.41, tutti pagati
(@UobId7, @TenantId, @UB03, @FyId, 0.00, 1870.41, 1870.41, 0.00, 0.00,   0.00,   0.00, 0, @UserId, @UserName, @Now),
-- B04 Vitale:      3 × 368.32 = 1104.96, tutti pagati
(@UobId8, @TenantId, @UB04, @FyId, 0.00, 1104.96, 1104.96, 0.00, 0.00,   0.00,   0.00, 0, @UserId, @UserName, @Now);

-- ============================================================
-- HILO: avanzamento contatori
-- ============================================================
UPDATE hibernate_unique_key SET next_hi = @HiCondominium        WHERE entity_type = 'Condominium';
UPDATE hibernate_unique_key SET next_hi = @HiCondominiumAddress WHERE entity_type = 'CondominiumAddress';
UPDATE hibernate_unique_key SET next_hi = @HiRealEstateUnit     WHERE entity_type = 'RealEstateUnit';
UPDATE hibernate_unique_key SET next_hi = @HiMillesimalTable    WHERE entity_type = 'MillesimalTable';
UPDATE hibernate_unique_key SET next_hi = @HiUnitMillesimal     WHERE entity_type = 'UnitMillesimal';
UPDATE hibernate_unique_key SET next_hi = @HiSupplier           WHERE entity_type = 'Supplier';
UPDATE hibernate_unique_key SET next_hi = @HiFiscalYear         WHERE entity_type = 'FiscalYear';
UPDATE hibernate_unique_key SET next_hi = @HiBudget             WHERE entity_type = 'Budget';
UPDATE hibernate_unique_key SET next_hi = @HiBudgetItem         WHERE entity_type = 'BudgetItem';
UPDATE hibernate_unique_key SET next_hi = @HiCondInst           WHERE entity_type = 'CondominiumInstallment';
UPDATE hibernate_unique_key SET next_hi = CAST(@FeeHi AS INT)   WHERE entity_type = 'CondominiumFee';
UPDATE hibernate_unique_key SET next_hi = @HiExpense            WHERE entity_type = 'Expense';
UPDATE hibernate_unique_key SET next_hi = @HiUnitOwner          WHERE entity_type = 'UnitOwner';
UPDATE hibernate_unique_key SET next_hi = @HiUnitOpeningBalance WHERE entity_type = 'UnitOpeningBalance';

COMMIT TRANSACTION;
PRINT N'Seed "Residenza Il Giardino" completato con successo.';
PRINT N'CondominiumId: ' + CAST(@CondId AS NVARCHAR);
PRINT N'FiscalYearId:  ' + CAST(@FyId  AS NVARCHAR);
PRINT N'MillesimalTableId: ' + CAST(@MtId AS NVARCHAR);

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@Msg, 16, 1);
END CATCH;
