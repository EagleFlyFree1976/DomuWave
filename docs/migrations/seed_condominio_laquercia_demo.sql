-- ============================================================
-- Seed: Condominio "La Quercia" — dati demo
-- 9 appartamenti + 3 box, esercizio 2024 chiuso con dati realistici
--
-- Prerequisiti:
--   - Esiste almeno un Tenant nel DB
--   - Tabelle lookup popolate (ExpenseTypeLookup, BudgetStatusLookup, ecc.)
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

DECLARE @TenantId UNIQUEIDENTIFIER = 'E60CE426-B371-444A-9D44-B40500664F9F';

-- ============================================================
-- HILO: assicura che tutte le entity_type esistano
-- ============================================================
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT v.et, 1 FROM (VALUES
    ('Condominium'),('CondominiumAddress'),('RealEstateUnit'),
    ('MillesimalTable'),('UnitMillesimal'),('Supplier'),
    ('FiscalYear'),('Budget'),('BudgetItem'),('CondominiumInstallment'),
    ('CondominiumFee'),('Expense'),('UnitOwner'),('UnitOpeningBalance'),('AccountBalance')
) v(et)
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = v.et);

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
DECLARE @HiAccountBalance       INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'AccountBalance');

-- ============================================================
-- IDs — Condominium & Address
-- ============================================================
DECLARE @CondId  INT = @HiCondominium        * (@MaxLo + 1);  SET @HiCondominium        += 1;
DECLARE @AddrId  INT = @HiCondominiumAddress * (@MaxLo + 1);  SET @HiCondominiumAddress += 1;

-- ============================================================
-- IDs — Units (9 apt + 3 box = 12)
-- ============================================================
DECLARE @UA01 INT = @HiRealEstateUnit * (@MaxLo + 1) + 0;
DECLARE @UA02 INT = @HiRealEstateUnit * (@MaxLo + 1) + 1;
DECLARE @UA03 INT = @HiRealEstateUnit * (@MaxLo + 1) + 2;
DECLARE @UB01 INT = @HiRealEstateUnit * (@MaxLo + 1) + 3;
DECLARE @UB02 INT = @HiRealEstateUnit * (@MaxLo + 1) + 4;
DECLARE @UB03 INT = @HiRealEstateUnit * (@MaxLo + 1) + 5;
DECLARE @UC01 INT = @HiRealEstateUnit * (@MaxLo + 1) + 6;
DECLARE @UC02 INT = @HiRealEstateUnit * (@MaxLo + 1) + 7;
DECLARE @UC03 INT = @HiRealEstateUnit * (@MaxLo + 1) + 8;
DECLARE @UBOX1 INT = @HiRealEstateUnit * (@MaxLo + 1) + 9;
DECLARE @UBOX2 INT = @HiRealEstateUnit * (@MaxLo + 1) + 10;  -- lo=10 ok (max_lo=10)
SET @HiRealEstateUnit += 2;  -- serve 1 blocco extra per UBOX3
DECLARE @UBOX3 INT = @HiRealEstateUnit * (@MaxLo + 1);        SET @HiRealEstateUnit += 1;

-- ============================================================
-- IDs — UnitOwner (uno per apt, 12 totali)
-- ============================================================
DECLARE @OWA01 INT = @HiUnitOwner * (@MaxLo + 1) + 0;
DECLARE @OWA02 INT = @HiUnitOwner * (@MaxLo + 1) + 1;
DECLARE @OWA03 INT = @HiUnitOwner * (@MaxLo + 1) + 2;
DECLARE @OWB01 INT = @HiUnitOwner * (@MaxLo + 1) + 3;
DECLARE @OWB02 INT = @HiUnitOwner * (@MaxLo + 1) + 4;
DECLARE @OWB03 INT = @HiUnitOwner * (@MaxLo + 1) + 5;
DECLARE @OWC01 INT = @HiUnitOwner * (@MaxLo + 1) + 6;
DECLARE @OWC02 INT = @HiUnitOwner * (@MaxLo + 1) + 7;
DECLARE @OWC03 INT = @HiUnitOwner * (@MaxLo + 1) + 8;
DECLARE @OWBX1 INT = @HiUnitOwner * (@MaxLo + 1) + 9;
DECLARE @OWBX2 INT = @HiUnitOwner * (@MaxLo + 1) + 10;
SET @HiUnitOwner += 2;
DECLARE @OWBX3 INT = @HiUnitOwner * (@MaxLo + 1);             SET @HiUnitOwner += 1;

-- ============================================================
-- IDs — MillesimalTable + UnitMillesimal (12 righe)
-- ============================================================
DECLARE @MtId INT = @HiMillesimalTable * (@MaxLo + 1);        SET @HiMillesimalTable += 1;

DECLARE @MMA01 INT = @HiUnitMillesimal * (@MaxLo + 1) + 0;
DECLARE @MMA02 INT = @HiUnitMillesimal * (@MaxLo + 1) + 1;
DECLARE @MMA03 INT = @HiUnitMillesimal * (@MaxLo + 1) + 2;
DECLARE @MMB01 INT = @HiUnitMillesimal * (@MaxLo + 1) + 3;
DECLARE @MMB02 INT = @HiUnitMillesimal * (@MaxLo + 1) + 4;
DECLARE @MMB03 INT = @HiUnitMillesimal * (@MaxLo + 1) + 5;
DECLARE @MMC01 INT = @HiUnitMillesimal * (@MaxLo + 1) + 6;
DECLARE @MMC02 INT = @HiUnitMillesimal * (@MaxLo + 1) + 7;
DECLARE @MMC03 INT = @HiUnitMillesimal * (@MaxLo + 1) + 8;
DECLARE @MMBX1 INT = @HiUnitMillesimal * (@MaxLo + 1) + 9;
DECLARE @MMBX2 INT = @HiUnitMillesimal * (@MaxLo + 1) + 10;
SET @HiUnitMillesimal += 2;
DECLARE @MMBX3 INT = @HiUnitMillesimal * (@MaxLo + 1);        SET @HiUnitMillesimal += 1;

-- ============================================================
-- IDs — Suppliers (3)
-- ============================================================
DECLARE @Sup1 INT = @HiSupplier * (@MaxLo + 1) + 0;  -- manutenzione
DECLARE @Sup2 INT = @HiSupplier * (@MaxLo + 1) + 1;  -- pulizie
DECLARE @Sup3 INT = @HiSupplier * (@MaxLo + 1) + 2;  -- amministratore
SET @HiSupplier += 1;

-- ============================================================
-- IDs — FiscalYear 2024
-- ============================================================
DECLARE @FyId INT = @HiFiscalYear * (@MaxLo + 1);             SET @HiFiscalYear += 1;

-- ============================================================
-- IDs — Budget Preventivo + Consuntivo
-- ============================================================
DECLARE @BudPrev INT = @HiBudget * (@MaxLo + 1) + 0;
DECLARE @BudCons INT = @HiBudget * (@MaxLo + 1) + 1;
SET @HiBudget += 1;

-- ============================================================
-- IDs — BudgetItem Preventivo (6 voci) + Consuntivo (6 voci)
-- ============================================================
DECLARE @BIP1 INT = @HiBudgetItem * (@MaxLo + 1) + 0;
DECLARE @BIP2 INT = @HiBudgetItem * (@MaxLo + 1) + 1;
DECLARE @BIP3 INT = @HiBudgetItem * (@MaxLo + 1) + 2;
DECLARE @BIP4 INT = @HiBudgetItem * (@MaxLo + 1) + 3;
DECLARE @BIP5 INT = @HiBudgetItem * (@MaxLo + 1) + 4;
DECLARE @BIP6 INT = @HiBudgetItem * (@MaxLo + 1) + 5;
DECLARE @BIC1 INT = @HiBudgetItem * (@MaxLo + 1) + 6;
DECLARE @BIC2 INT = @HiBudgetItem * (@MaxLo + 1) + 7;
DECLARE @BIC3 INT = @HiBudgetItem * (@MaxLo + 1) + 8;
DECLARE @BIC4 INT = @HiBudgetItem * (@MaxLo + 1) + 9;
DECLARE @BIC5 INT = @HiBudgetItem * (@MaxLo + 1) + 10;
SET @HiBudgetItem += 2;
DECLARE @BIC6 INT = @HiBudgetItem * (@MaxLo + 1);             SET @HiBudgetItem += 1;

-- ============================================================
-- IDs — CondominiumInstallment (4 rate trimestrali)
-- ============================================================
DECLARE @CI1 INT = @HiCondInst * (@MaxLo + 1) + 0;
DECLARE @CI2 INT = @HiCondInst * (@MaxLo + 1) + 1;
DECLARE @CI3 INT = @HiCondInst * (@MaxLo + 1) + 2;
DECLARE @CI4 INT = @HiCondInst * (@MaxLo + 1) + 3;
SET @HiCondInst += 1;

-- ============================================================
-- IDs — Expenses (6 spese reali)
-- ============================================================
DECLARE @EXP1 INT = @HiExpense * (@MaxLo + 1) + 0;
DECLARE @EXP2 INT = @HiExpense * (@MaxLo + 1) + 1;
DECLARE @EXP3 INT = @HiExpense * (@MaxLo + 1) + 2;
DECLARE @EXP4 INT = @HiExpense * (@MaxLo + 1) + 3;
DECLARE @EXP5 INT = @HiExpense * (@MaxLo + 1) + 4;
DECLARE @EXP6 INT = @HiExpense * (@MaxLo + 1) + 5;
SET @HiExpense += 1;

-- ============================================================
-- INSERIMENTI
-- ============================================================

-- ── Condominium ──────────────────────────────────────────────
INSERT INTO Condominium (Id, TenantId, Name, Code, TaxCode, NumberOfUnits, NumberOfStaircases,
    TotalMillesimal, InstallmentFrequency, InstallmentDueDay, IsActive,
    CreatedById, CreatedByFullName, IsDeleted, CreationDate)
VALUES (@CondId, @TenantId, N'Condominio La Quercia', N'LQ-001', N'91054320153',
    12, 1, 1000.0000, N'Quarterly', 15, 1,
    @UserId, @UserName, 0, @Now);

-- ── CondominiumAddress ───────────────────────────────────────
INSERT INTO CondominiumAddress (Id, TenantId, CondominiumId, Street, StreetNumber, PostalCode, City, Province, Country,
    CreatedById, CreatedByFullName, IsDeleted, CreationDate)
VALUES (@AddrId, @TenantId, @CondId, N'Via delle Querce', N'14', N'20141', N'Milano', N'MI', N'IT',
    @UserId, @UserName, 0, @Now);

-- ── RealEstateUnit ───────────────────────────────────────────
-- Colonne reali: Staircase, Floor, InternalNumber, AreaSqm, Rooms,
--   UnitType (NOT NULL), OccupancyStatus (NOT NULL), NumeroAbitanti (NOT NULL),
--   DisplayName, IsActive
-- Scala A: 3 appartamenti (piano 1-2-3)
INSERT INTO RealEstateUnit (Id, TenantId, CondominiumId,
    Staircase, Floor, InternalNumber, AreaSqm, Rooms,
    UnitType, OccupancyStatus, NumeroAbitanti, DisplayName, IsActive,
    IsDeleted, CreatedById, CreatedByFullName, CreationDate)
VALUES
(@UA01,  @TenantId, @CondId, N'A', 1, N'A01',   85.00, 3, N'Residenziale', N'Occupato', 2, N'Ferrari',   1, 0, @UserId, @UserName, @Now),
(@UA02,  @TenantId, @CondId, N'A', 2, N'A02',  110.00, 4, N'Residenziale', N'Occupato', 3, N'Marchetti', 1, 0, @UserId, @UserName, @Now),
(@UA03,  @TenantId, @CondId, N'A', 3, N'A03',   90.00, 3, N'Residenziale', N'Occupato', 2, N'Colombo',   1, 0, @UserId, @UserName, @Now),
-- Scala B: 3 appartamenti
(@UB01,  @TenantId, @CondId, N'B', 1, N'B01',   65.00, 2, N'Residenziale', N'Occupato', 1, N'Galli',     1, 0, @UserId, @UserName, @Now),
(@UB02,  @TenantId, @CondId, N'B', 2, N'B02',  120.00, 4, N'Residenziale', N'Occupato', 3, N'Fontana',   1, 0, @UserId, @UserName, @Now),
(@UB03,  @TenantId, @CondId, N'B', 3, N'B03',   95.00, 3, N'Residenziale', N'Occupato', 2, N'Rizzo',     1, 0, @UserId, @UserName, @Now),
-- Scala C: 3 appartamenti
(@UC01,  @TenantId, @CondId, N'C', 1, N'C01',   80.00, 3, N'Residenziale', N'Occupato', 2, N'Barbieri',  1, 0, @UserId, @UserName, @Now),
(@UC02,  @TenantId, @CondId, N'C', 2, N'C02',   60.00, 2, N'Residenziale', N'Occupato', 1, N'Conti',     1, 0, @UserId, @UserName, @Now),
(@UC03,  @TenantId, @CondId, N'C', 3, N'C03',  130.00, 4, N'Residenziale', N'Occupato', 3, N'Greco',     1, 0, @UserId, @UserName, @Now),
-- Box auto
(@UBOX1, @TenantId, @CondId, N'A', 0, N'BOX-A1', 14.00, 0, N'Box',         N'Occupato', 0, N'Ferrari',   1, 0, @UserId, @UserName, @Now),
(@UBOX2, @TenantId, @CondId, N'B', 0, N'BOX-B1', 14.00, 0, N'Box',         N'Occupato', 0, N'Fontana',   1, 0, @UserId, @UserName, @Now),
(@UBOX3, @TenantId, @CondId, N'C', 0, N'BOX-C1', 14.00, 0, N'Box',         N'Occupato', 0, N'Greco',     1, 0, @UserId, @UserName, @Now);

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
(@OWA01, @TenantId, @UA01,  N'Proprietario', N'Ferrari',   N'Marco',    1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWA02, @TenantId, @UA02,  N'Proprietario', N'Marchetti', N'Laura',    1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWA03, @TenantId, @UA03,  N'Proprietario', N'Colombo',   N'Giuseppe', 1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWB01, @TenantId, @UB01,  N'Proprietario', N'Galli',     N'Sofia',    1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWB02, @TenantId, @UB02,  N'Proprietario', N'Fontana',   N'Roberto',  1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWB03, @TenantId, @UB03,  N'Proprietario', N'Rizzo',     N'Anna',     1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWC01, @TenantId, @UC01,  N'Proprietario', N'Barbieri',  N'Luca',     1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWC02, @TenantId, @UC02,  N'Proprietario', N'Conti',     N'Elena',    1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWC03, @TenantId, @UC03,  N'Proprietario', N'Greco',     N'Paolo',    1.0000, @StartOwnership, 1, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWBX1, @TenantId, @UBOX1, N'Proprietario', N'Ferrari',   N'Marco',    1.0000, @StartOwnership, 0, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWBX2, @TenantId, @UBOX2, N'Proprietario', N'Fontana',   N'Roberto',  1.0000, @StartOwnership, 0, 1, 1, 0, 0, @UserId, @UserName, @Now),
(@OWBX3, @TenantId, @UBOX3, N'Proprietario', N'Greco',     N'Paolo',    1.0000, @StartOwnership, 0, 1, 1, 0, 0, @UserId, @UserName, @Now);

-- ── MillesimalTable ──────────────────────────────────────────
-- Colonne reali: Name, Code, TotalMillesimal, IsActive, IsEnabled, IsDraft
INSERT INTO MillesimalTable (Id, TenantId, CondominiumId, Name, Code,
    TotalMillesimal, IsActive, IsEnabled, IsDraft,
    IsDeleted, CreatedById, CreatedByFullName, CreationDate)
VALUES (@MtId, @TenantId, @CondId, N'Tabella Millesimale Generale', N'TMG',
    1000.0000, 1, 1, 0,
    0, @UserId, @UserName, @Now);

-- ── UnitMillesimal ───────────────────────────────────────────
-- Totale = 1000 millesimi, proporzionali alla superficie
INSERT INTO UnitMillesimal (Id, TenantId, MillesimalTableId, UnitId, Millesimal, IsDeleted, CreatedById, CreatedByFullName, CreationDate)
VALUES
(@MMA01, @TenantId, @MtId, @UA01,  98.7500, 0, @UserId, @UserName, @Now),  -- 85 mq
(@MMA02, @TenantId, @MtId, @UA02, 127.7500, 0, @UserId, @UserName, @Now),  -- 110 mq
(@MMA03, @TenantId, @MtId, @UA03, 104.5000, 0, @UserId, @UserName, @Now),  -- 90 mq
(@MMB01, @TenantId, @MtId, @UB01,  75.4500, 0, @UserId, @UserName, @Now),  -- 65 mq
(@MMB02, @TenantId, @MtId, @UB02, 139.3000, 0, @UserId, @UserName, @Now),  -- 120 mq
(@MMB03, @TenantId, @MtId, @UB03, 110.3000, 0, @UserId, @UserName, @Now),  -- 95 mq
(@MMC01, @TenantId, @MtId, @UC01,  92.9000, 0, @UserId, @UserName, @Now),  -- 80 mq
(@MMC02, @TenantId, @MtId, @UC02,  69.7000, 0, @UserId, @UserName, @Now),  -- 60 mq
(@MMC03, @TenantId, @MtId, @UC03, 150.9500, 0, @UserId, @UserName, @Now),  -- 130 mq
(@MMBX1, @TenantId, @MtId, @UBOX1, 10.3500, 0, @UserId, @UserName, @Now),
(@MMBX2, @TenantId, @MtId, @UBOX2, 10.3500, 0, @UserId, @UserName, @Now),
(@MMBX3, @TenantId, @MtId, @UBOX3, 10.3500, 0, @UserId, @UserName, @Now);
-- Totale: 98.75+127.75+104.50+75.45+139.30+110.30+92.90+69.70+150.95+10.35+10.35+10.35 = 1000.65 ≈ 1000

-- ── Suppliers ────────────────────────────────────────────────
-- Colonna fisica: CompanyName (= Name nel mapping NHibernate)
INSERT INTO Supplier (Id, TenantId, CompanyName, VatNumber, IsActive,
    IsDeleted, CreatedById, CreatedByFullName, CreationDate)
VALUES
(@Sup1, @TenantId, N'Tecno Impianti Srl',   N'IT03456780152', 1, 0, @UserId, @UserName, @Now),
(@Sup2, @TenantId, N'CleanPro Srl',         N'IT07891230154', 1, 0, @UserId, @UserName, @Now),
(@Sup3, @TenantId, N'Studio Amm. Ferretti', N'IT09876540151', 1, 0, @UserId, @UserName, @Now);

-- ── FiscalYear 2024 (Closed) ─────────────────────────────────
INSERT INTO FiscalYear (FiscalYearId, TenantId, CondominiumId, Code, Description, StartDate, EndDate,
    StatusId, IsActive, ClosedDate,
    CreatedBy, CreatedByFullName, IsDeleted, CreationDate)
VALUES (@FyId, @TenantId, @CondId, N'2024', N'Esercizio 2024',
    '2024-01-01', '2024-12-31',
    4,  -- Closed
    0, '2025-02-10',
    @UserId, @UserName, 0, @Now);

-- ── Recupera conti del piano dei conti ────────────────────────
DECLARE @AccManutenzione  INT = NULL;
DECLARE @AccPulizie       INT = NULL;
DECLARE @AccAmministrazione INT = NULL;
DECLARE @AccAssicurazione INT = NULL;
DECLARE @AccUtenze        INT = NULL;
DECLARE @AccFondoRiserva  INT = NULL;

SELECT @AccManutenzione    = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0 AND Code LIKE '5.1%';
SELECT @AccPulizie         = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0 AND Code LIKE '5.3%';
SELECT @AccAmministrazione = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0 AND Code LIKE '5.4%';
SELECT @AccAssicurazione   = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0 AND Code LIKE '5.5%';
SELECT @AccUtenze          = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0 AND Code LIKE '5.6%';
SELECT @AccFondoRiserva    = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0 AND Code LIKE '5.7%';

IF @AccManutenzione    IS NULL SELECT @AccManutenzione    = MIN(Id) FROM ChartOfAccounts WHERE TenantId = @TenantId AND CondominiumId = @CondId AND IsDeleted = 0;
IF @AccPulizie         IS NULL SET @AccPulizie         = @AccManutenzione;
IF @AccAmministrazione IS NULL SET @AccAmministrazione = @AccManutenzione;
IF @AccAssicurazione   IS NULL SET @AccAssicurazione   = @AccManutenzione;
IF @AccUtenze          IS NULL SET @AccUtenze          = @AccManutenzione;
IF @AccFondoRiserva    IS NULL SET @AccFondoRiserva    = @AccManutenzione;

-- ── Budget Preventivo 2024 (Approved) ───────────────────────
-- Totale preventivato: 12.400 €
-- Colonne reali: TotalIncome, TotalExpenses, Notes (= Description fisico)
INSERT INTO Budget (Id, TenantId, CondominiumId, FiscalYearId, Type, StatusId,
    TotalIncome, TotalExpenses, Notes,
    CreatedById, CreatedByFullName, IsDeleted, CreationDate)
VALUES (@BudPrev, @TenantId, @CondId, @FyId, 1, 2,
    12400.00, 12400.00, N'Preventivo approvato assemblea 15/01/2024',
    @UserId, @UserName, 0, @Now);

-- BudgetItem: colonne reali AccountId (NOT NULL), Description (= Name fisico)
IF @AccManutenzione IS NOT NULL
BEGIN
    INSERT INTO BudgetItem (Id, TenantId, BudgetId, AccountId, Description, Amount, AmountPaid,
        IsDeleted, CreatedById, CreatedByFullName, CreationDate)
    VALUES
    (@BIP1, @TenantId, @BudPrev, @AccManutenzione,    N'Manutenzione ordinaria parti comuni',  3200.00, 3200.00, 0, @UserId, @UserName, @Now),
    (@BIP2, @TenantId, @BudPrev, @AccPulizie,         N'Servizio pulizie scale e aree comuni', 2400.00, 2400.00, 0, @UserId, @UserName, @Now),
    (@BIP3, @TenantId, @BudPrev, @AccAmministrazione, N'Onorario amministratore',              1800.00, 1800.00, 0, @UserId, @UserName, @Now),
    (@BIP4, @TenantId, @BudPrev, @AccAssicurazione,   N'Assicurazione fabbricato',             1400.00, 1400.00, 0, @UserId, @UserName, @Now),
    (@BIP5, @TenantId, @BudPrev, @AccUtenze,          N'Utenze (luce scale, acqua)',           2200.00, 2200.00, 0, @UserId, @UserName, @Now),
    (@BIP6, @TenantId, @BudPrev, @AccFondoRiserva,    N'Fondo riserva',                        1400.00, 1400.00, 0, @UserId, @UserName, @Now);
END

-- ── Budget Consuntivo 2024 (Approved) ───────────────────────
-- Totale consuntivo: 12.850 €
INSERT INTO Budget (Id, TenantId, CondominiumId, FiscalYearId, Type, StatusId,
    TotalIncome, TotalExpenses, Notes,
    CreatedById, CreatedByFullName, IsDeleted, CreationDate)
VALUES (@BudCons, @TenantId, @CondId, @FyId, 2, 2,
    12850.00, 12850.00, N'Consuntivo approvato assemblea 20/01/2025',
    @UserId, @UserName, 0, @Now);

IF @AccManutenzione IS NOT NULL
BEGIN
    INSERT INTO BudgetItem (Id, TenantId, BudgetId, AccountId, Description, Amount, AmountPaid, Notes,
        IsDeleted, CreatedById, CreatedByFullName, CreationDate)
    VALUES
    (@BIC1, @TenantId, @BudCons, @AccManutenzione,    N'Manutenzione ordinaria parti comuni',  3450.00, 3450.00, N'Intervento extra cancello automatico', 0, @UserId, @UserName, @Now),
    (@BIC2, @TenantId, @BudCons, @AccPulizie,         N'Servizio pulizie scale e aree comuni', 2400.00, 2400.00, NULL, 0, @UserId, @UserName, @Now),
    (@BIC3, @TenantId, @BudCons, @AccAmministrazione, N'Onorario amministratore',              1800.00, 1800.00, NULL, 0, @UserId, @UserName, @Now),
    (@BIC4, @TenantId, @BudCons, @AccAssicurazione,   N'Assicurazione fabbricato',             1420.00, 1420.00, N'Adeguamento premio', 0, @UserId, @UserName, @Now),
    (@BIC5, @TenantId, @BudCons, @AccUtenze,          N'Utenze (luce scale, acqua)',           2380.00, 2380.00, NULL, 0, @UserId, @UserName, @Now),
    (@BIC6, @TenantId, @BudCons, @AccFondoRiserva,    N'Fondo riserva',                        1400.00, 1400.00, NULL, 0, @UserId, @UserName, @Now);
END

-- ── CondominiumInstallment: 4 rate trimestrali ───────────────
-- Colonne reali: Description (= Name fisico), InstallmentNumber, DueDate, TotalAmount, StatusId (FK)
-- Rate: Q1=30%, Q2=30%, Q3=20%, Q4=20%
INSERT INTO CondominiumInstallment (Id, TenantId, CondominiumId, BudgetId, FiscalYearId,
    Description, InstallmentNumber, DueDate, TotalAmount, StatusId,
    IsDeleted, CreatedById, CreatedByFullName, CreationDate)
VALUES
(@CI1, @TenantId, @CondId, @BudPrev, @FyId, N'1ª Rata 2024', 1, '2024-03-15', 3720.00, 3, 0, @UserId, @UserName, @Now),
(@CI2, @TenantId, @CondId, @BudPrev, @FyId, N'2ª Rata 2024', 2, '2024-06-15', 3720.00, 3, 0, @UserId, @UserName, @Now),
(@CI3, @TenantId, @CondId, @BudPrev, @FyId, N'3ª Rata 2024', 3, '2024-09-15', 2480.00, 3, 0, @UserId, @UserName, @Now),
(@CI4, @TenantId, @CondId, @BudPrev, @FyId, N'4ª Rata 2024', 4, '2024-12-15', 2480.00, 3, 0, @UserId, @UserName, @Now);
-- StatusId=3 = Paid

-- ── CondominiumFee: quote per unità (4 rate × 12 unità = 48 righe) ──
-- Quota per unità = TotaleRata * (millesimi / 1000)
-- Importi calcolati per ogni unità e rata:
-- A01(98.75): Q1=367.65, Q2=367.65, Q3=245.10, Q4=245.10
-- A02(127.75): Q1=475.23, Q2=475.23, Q3=316.82, Q4=316.82
-- A03(104.50): Q1=388.74, Q2=388.74, Q3=259.16, Q4=259.16
-- B01(75.45):  Q1=280.67, Q2=280.67, Q3=187.12, Q4=187.12
-- B02(139.30): Q1=518.20, Q2=518.20, Q3=345.46, Q4=345.46
-- B03(110.30): Q1=410.32, Q2=410.32, Q3=273.54, Q4=273.54
-- C01(92.90):  Q1=345.59, Q2=345.59, Q3=230.39, Q4=230.39
-- C02(69.70):  Q1=259.32, Q2=259.32, Q3=172.88, Q4=172.88
-- C03(150.95): Q1=561.53, Q2=561.53, Q3=374.36, Q4=374.36
-- BX1(10.35):  Q1=38.50,  Q2=38.50,  Q3=25.67,  Q4=25.67
-- BX2(10.35):  Q1=38.50,  Q2=38.50,  Q3=25.67,  Q4=25.67
-- BX3(10.35):  Q1=38.50,  Q2=38.50,  Q3=25.67,  Q4=25.67
-- Nota: C03-Greco e B01-Galli hanno una rata Q4 non pagata (morosità)

-- CondominiumFee: Id è BIGINT (hilo usa Int64)
DECLARE @FeeHi BIGINT = CAST(@HiCondFee AS BIGINT);

IF OBJECT_ID('tempdb..#fees') IS NOT NULL DROP TABLE #fees;
CREATE TABLE #fees (
    Id BIGINT, UnitId INT, InstId INT,
    AmountDue DECIMAL(18,4), AmountPaid DECIMAL(18,4)
);

-- A01 - Ferrari (tutti pagati)
INSERT INTO #fees VALUES
(@FeeHi*11+0,  @UA01, @CI1, 367.65, 367.65),
(@FeeHi*11+1,  @UA01, @CI2, 367.65, 367.65),
(@FeeHi*11+2,  @UA01, @CI3, 245.10, 245.10),
(@FeeHi*11+3,  @UA01, @CI4, 245.10, 245.10);
SET @FeeHi += 1;

-- A02 - Marchetti (tutti pagati)
INSERT INTO #fees VALUES
(@FeeHi*11+0,  @UA02, @CI1, 475.23, 475.23),
(@FeeHi*11+1,  @UA02, @CI2, 475.23, 475.23),
(@FeeHi*11+2,  @UA02, @CI3, 316.82, 316.82),
(@FeeHi*11+3,  @UA02, @CI4, 316.82, 316.82);
SET @FeeHi += 1;

-- A03 - Colombo (tutti pagati)
INSERT INTO #fees VALUES
(@FeeHi*11+0,  @UA03, @CI1, 388.74, 388.74),
(@FeeHi*11+1,  @UA03, @CI2, 388.74, 388.74),
(@FeeHi*11+2,  @UA03, @CI3, 259.16, 259.16),
(@FeeHi*11+3,  @UA03, @CI4, 259.16, 259.16);
SET @FeeHi += 1;

-- B01 - Galli (Q4 non pagata — morosità 187.12)
INSERT INTO #fees VALUES
(@FeeHi*11+0,  @UB01, @CI1, 280.67, 280.67),
(@FeeHi*11+1,  @UB01, @CI2, 280.67, 280.67),
(@FeeHi*11+2,  @UB01, @CI3, 187.12, 187.12),
(@FeeHi*11+3,  @UB01, @CI4, 187.12,   0.00);  -- non pagata
SET @FeeHi += 1;

-- B02 - Fontana (tutti pagati)
INSERT INTO #fees VALUES
(@FeeHi*11+0,  @UB02, @CI1, 518.20, 518.20),
(@FeeHi*11+1,  @UB02, @CI2, 518.20, 518.20),
(@FeeHi*11+2,  @UB02, @CI3, 345.46, 345.46),
(@FeeHi*11+3,  @UB02, @CI4, 345.46, 345.46);
SET @FeeHi += 1;

-- B03 - Rizzo (tutti pagati)
INSERT INTO #fees VALUES
(@FeeHi*11+0,  @UB03, @CI1, 410.32, 410.32),
(@FeeHi*11+1,  @UB03, @CI2, 410.32, 410.32),
(@FeeHi*11+2,  @UB03, @CI3, 273.54, 273.54),
(@FeeHi*11+3,  @UB03, @CI4, 273.54, 273.54);
SET @FeeHi += 1;

-- C01 - Barbieri (tutti pagati)
INSERT INTO #fees VALUES
(@FeeHi*11+0,  @UC01, @CI1, 345.59, 345.59),
(@FeeHi*11+1,  @UC01, @CI2, 345.59, 345.59),
(@FeeHi*11+2,  @UC01, @CI3, 230.39, 230.39),
(@FeeHi*11+3,  @UC01, @CI4, 230.39, 230.39);
SET @FeeHi += 1;

-- C02 - Conti (tutti pagati)
INSERT INTO #fees VALUES
(@FeeHi*11+0,  @UC02, @CI1, 259.32, 259.32),
(@FeeHi*11+1,  @UC02, @CI2, 259.32, 259.32),
(@FeeHi*11+2,  @UC02, @CI3, 172.88, 172.88),
(@FeeHi*11+3,  @UC02, @CI4, 172.88, 172.88);
SET @FeeHi += 1;

-- C03 - Greco (Q3+Q4 non pagate — morosità 748.72)
INSERT INTO #fees VALUES
(@FeeHi*11+0,  @UC03, @CI1, 561.53, 561.53),
(@FeeHi*11+1,  @UC03, @CI2, 561.53, 561.53),
(@FeeHi*11+2,  @UC03, @CI3, 374.36,   0.00),  -- non pagata
(@FeeHi*11+3,  @UC03, @CI4, 374.36,   0.00);  -- non pagata
SET @FeeHi += 1;

-- BOX-A1 - Ferrari (tutti pagati)
INSERT INTO #fees VALUES
(@FeeHi*11+0,  @UBOX1, @CI1, 38.50, 38.50),
(@FeeHi*11+1,  @UBOX1, @CI2, 38.50, 38.50),
(@FeeHi*11+2,  @UBOX1, @CI3, 25.67, 25.67),
(@FeeHi*11+3,  @UBOX1, @CI4, 25.67, 25.67);
SET @FeeHi += 1;

-- BOX-B1 - Fontana (tutti pagati)
INSERT INTO #fees VALUES
(@FeeHi*11+0,  @UBOX2, @CI1, 38.50, 38.50),
(@FeeHi*11+1,  @UBOX2, @CI2, 38.50, 38.50),
(@FeeHi*11+2,  @UBOX2, @CI3, 25.67, 25.67),
(@FeeHi*11+3,  @UBOX2, @CI4, 25.67, 25.67);
SET @FeeHi += 1;

-- BOX-C1 - Greco (tutti pagati)
INSERT INTO #fees VALUES
(@FeeHi*11+0,  @UBOX3, @CI1, 38.50, 38.50),
(@FeeHi*11+1,  @UBOX3, @CI2, 38.50, 38.50),
(@FeeHi*11+2,  @UBOX3, @CI3, 25.67, 25.67),
(@FeeHi*11+3,  @UBOX3, @CI4, 25.67, 25.67);
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

-- ── Expenses (6 spese reali) ──────────────────────────────────
-- Colonne reali: AccountId (NOT NULL), MillesimalTableId (NOT NULL),
--   Description (= Name fisico), PaymentStatusId (FK), ExpenseTypeId (FK),
--   ChargeabilityType (FK int)
IF @AccManutenzione IS NOT NULL
BEGIN
    DECLARE @EtManutenzione    INT = (SELECT TOP 1 Id FROM ExpenseTypeLookup WHERE Id = 1);
    DECLARE @EtPulizie         INT = (SELECT TOP 1 Id FROM ExpenseTypeLookup WHERE Id = 2);
    DECLARE @EtUtenze          INT = (SELECT TOP 1 Id FROM ExpenseTypeLookup WHERE Id = 4);
    DECLARE @EtAmministrazione INT = (SELECT TOP 1 Id FROM ExpenseTypeLookup WHERE Id = 5);
    DECLARE @EtAssicurazione   INT = (SELECT TOP 1 Id FROM ExpenseTypeLookup WHERE Id = 6);
    DECLARE @PayPagata         INT = (SELECT TOP 1 Id FROM ExpensePaymentStatusLookup WHERE Id = 2);
    DECLARE @ChargeOwner       INT = (SELECT TOP 1 Id FROM ChargeabilityTypeLookup   WHERE Id = 0);

    IF @EtManutenzione    IS NULL SELECT @EtManutenzione    = MIN(Id) FROM ExpenseTypeLookup;
    IF @EtPulizie         IS NULL SET @EtPulizie         = @EtManutenzione;
    IF @EtUtenze          IS NULL SET @EtUtenze          = @EtManutenzione;
    IF @EtAmministrazione IS NULL SET @EtAmministrazione = @EtManutenzione;
    IF @EtAssicurazione   IS NULL SET @EtAssicurazione   = @EtManutenzione;
    IF @PayPagata         IS NULL SELECT @PayPagata       = MIN(Id) FROM ExpensePaymentStatusLookup;
    IF @ChargeOwner       IS NULL SELECT @ChargeOwner     = MIN(Id) FROM ChargeabilityTypeLookup;

    INSERT INTO Expense (Id, TenantId, CondominiumId, FiscalYearId, AccountId, MillesimalTableId,
        SupplierId, Description, GrossAmount, NetAmount, VatAmount,
        ExpenseTypeId, PaymentStatusId, ChargeabilityType,
        DocumentDate, RegistrationDate,
        IsDeleted, CreatedById, CreatedByFullName, CreationDate)
    VALUES
    (@EXP1, @TenantId, @CondId, @FyId, @AccManutenzione, @MtId,
        @Sup1, N'Manutenzione cancello automatico',
        1250.00, 1025.41, 224.59, @EtManutenzione, @PayPagata, @ChargeOwner,
        '2024-03-10', '2024-03-12', 0, @UserId, @UserName, @Now),

    (@EXP2, @TenantId, @CondId, @FyId, @AccManutenzione, @MtId,
        @Sup1, N'Riparazione citofoni scala B',
        580.00, 475.41, 104.59, @EtManutenzione, @PayPagata, @ChargeOwner,
        '2024-05-20', '2024-05-22', 0, @UserId, @UserName, @Now),

    (@EXP3, @TenantId, @CondId, @FyId, @AccPulizie, @MtId,
        @Sup2, N'Servizio pulizie annuale',
        2400.00, 1967.21, 432.79, @EtPulizie, @PayPagata, @ChargeOwner,
        '2024-12-31', '2024-12-31', 0, @UserId, @UserName, @Now),

    (@EXP4, @TenantId, @CondId, @FyId, @AccAmministrazione, @MtId,
        @Sup3, N'Onorario amministratore 2024',
        1800.00, 1639.34, 160.66, @EtAmministrazione, @PayPagata, @ChargeOwner,
        '2024-12-31', '2024-12-31', 0, @UserId, @UserName, @Now),

    (@EXP5, @TenantId, @CondId, @FyId, @AccAssicurazione, @MtId,
        NULL, N'Assicurazione fabbricato 2024',
        1420.00, 1420.00, 0.00, @EtAssicurazione, @PayPagata, @ChargeOwner,
        '2024-01-15', '2024-01-16', 0, @UserId, @UserName, @Now),

    (@EXP6, @TenantId, @CondId, @FyId, @AccUtenze, @MtId,
        NULL, N'Bollette energia elettrica scale',
        2380.00, 1950.82, 429.18, @EtUtenze, @PayPagata, @ChargeOwner,
        '2024-12-15', '2024-12-16', 0, @UserId, @UserName, @Now);
END
ELSE
    PRINT N'ATTENZIONE: Piano dei conti non trovato — Expense non inserite.';

-- ── UnitOpeningBalance 2024 ───────────────────────────────────
-- Primo esercizio: OpeningBalance = 0 per tutti
-- ClosingBalance = RateAddebitate - RateIncassate (insoluto)
-- Morosità: B01-Galli = 187.12, C03-Greco = 748.72

DECLARE @UobHi INT = @HiUnitOpeningBalance;
DECLARE @UobId0  INT = @UobHi * (@MaxLo+1) + 0;
DECLARE @UobId1  INT = @UobHi * (@MaxLo+1) + 1;
DECLARE @UobId2  INT = @UobHi * (@MaxLo+1) + 2;
DECLARE @UobId3  INT = @UobHi * (@MaxLo+1) + 3;
DECLARE @UobId4  INT = @UobHi * (@MaxLo+1) + 4;
DECLARE @UobId5  INT = @UobHi * (@MaxLo+1) + 5;
DECLARE @UobId6  INT = @UobHi * (@MaxLo+1) + 6;
DECLARE @UobId7  INT = @UobHi * (@MaxLo+1) + 7;
DECLARE @UobId8  INT = @UobHi * (@MaxLo+1) + 8;
DECLARE @UobId9  INT = @UobHi * (@MaxLo+1) + 9;
DECLARE @UobId10 INT = @UobHi * (@MaxLo+1) + 10;
SET @HiUnitOpeningBalance += 2;
DECLARE @UobId11 INT = @HiUnitOpeningBalance * (@MaxLo+1); SET @HiUnitOpeningBalance += 1;

-- Calcolo QuotaConsuntiva per unità = 12850 * millesimi / 1000
-- SaldoConguaglio = QuotaConsuntiva - RateAddebitate (totale 4 rate)
-- RateAddebitate totali per unità = (Q1+Q2+Q3+Q4).AmountDue

INSERT INTO UnitOpeningBalance (Id, TenantId, UnitId, FiscalYearId,
    OpeningBalance, RateAddebitate, RateIncassate, QuotaConsuntiva, SaldoConguaglio,
    TotalMovements, ClosingBalance,
    IsDeleted, CreatedById, CreatedByFullName, CreationDate)
VALUES
-- A01 Ferrari: rate=1225.50, incassate=1225.50, quota=1268.79, saldo=43.29, closing=43.29
(@UobId0,  @TenantId, @UA01,  @FyId, 0, 1225.50, 1225.50, 1268.79,   43.29,   43.29,   43.29, 0, @UserId, @UserName, @Now),
-- A02 Marchetti: rate=1584.10, incassate=1584.10, quota=1641.54, saldo=57.44, closing=57.44
(@UobId1,  @TenantId, @UA02,  @FyId, 0, 1584.10, 1584.10, 1641.54,   57.44,   57.44,   57.44, 0, @UserId, @UserName, @Now),
-- A03 Colombo: rate=1295.80, incassate=1295.80, quota=1342.83, saldo=47.03, closing=47.03
(@UobId2,  @TenantId, @UA03,  @FyId, 0, 1295.80, 1295.80, 1342.83,   47.03,   47.03,   47.03, 0, @UserId, @UserName, @Now),
-- B01 Galli: rate=935.58, incassate=748.46, quota=969.03, saldo=33.45, closing=187.12 (morosità)
(@UobId3,  @TenantId, @UB01,  @FyId, 0,  935.58,  748.46,  969.03,   33.45,  187.12,  187.12, 0, @UserId, @UserName, @Now),
-- B02 Fontana: rate=1727.32, incassate=1727.32, quota=1789.81, saldo=62.49, closing=62.49
(@UobId4,  @TenantId, @UB02,  @FyId, 0, 1727.32, 1727.32, 1789.81,   62.49,   62.49,   62.49, 0, @UserId, @UserName, @Now),
-- B03 Rizzo: rate=1367.72, incassate=1367.72, quota=1416.60, saldo=48.88, closing=48.88
(@UobId5,  @TenantId, @UB03,  @FyId, 0, 1367.72, 1367.72, 1416.60,   48.88,   48.88,   48.88, 0, @UserId, @UserName, @Now),
-- C01 Barbieri: rate=1151.96, incassate=1151.96, quota=1193.24, saldo=41.28, closing=41.28
(@UobId6,  @TenantId, @UC01,  @FyId, 0, 1151.96, 1151.96, 1193.24,   41.28,   41.28,   41.28, 0, @UserId, @UserName, @Now),
-- C02 Conti: rate=864.40, incassate=864.40, quota=895.65, saldo=31.25, closing=31.25
(@UobId7,  @TenantId, @UC02,  @FyId, 0,  864.40,  864.40,  895.65,   31.25,   31.25,   31.25, 0, @UserId, @UserName, @Now),
-- C03 Greco: rate=1871.78, incassate=1123.06, quota=1939.71, saldo=67.93, closing=748.72 (morosità)
(@UobId8,  @TenantId, @UC03,  @FyId, 0, 1871.78, 1123.06, 1939.71,   67.93,  748.72,  748.72, 0, @UserId, @UserName, @Now),
-- BOX-A1 Ferrari: rate=128.34, incassate=128.34, quota=132.96, saldo=4.62, closing=4.62
(@UobId9,  @TenantId, @UBOX1, @FyId, 0,  128.34,  128.34,  132.96,    4.62,    4.62,    4.62, 0, @UserId, @UserName, @Now),
-- BOX-B1 Fontana: rate=128.34, incassate=128.34, quota=132.96, saldo=4.62, closing=4.62
(@UobId10, @TenantId, @UBOX2, @FyId, 0,  128.34,  128.34,  132.96,    4.62,    4.62,    4.62, 0, @UserId, @UserName, @Now),
-- BOX-C1 Greco: rate=128.34, incassate=128.34, quota=132.96, saldo=4.62, closing=4.62
(@UobId11, @TenantId, @UBOX3, @FyId, 0,  128.34,  128.34,  132.96,    4.62,    4.62,    4.62, 0, @UserId, @UserName, @Now);

-- ── AccountBalance ────────────────────────────────────────────
-- Solo se esistono conti nel piano dei conti per questo condominio
-- (opzionale — se il piano dei conti non è ancora stato creato, saltare)
-- INSERT INTO AccountBalance ... (omesso: dipende dalla configurazione del piano dei conti)

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
PRINT N'Seed "Condominio La Quercia" completato con successo.';
PRINT N'CondominiumId: ' + CAST(@CondId AS NVARCHAR);
PRINT N'FiscalYearId:  ' + CAST(@FyId  AS NVARCHAR);

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@Msg, 16, 1);
END CATCH;
