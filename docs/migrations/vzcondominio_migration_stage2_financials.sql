-- ============================================================
-- Migration: VZCondominio -> DomuWave (Stage 2 Financials)
-- Target:
-- - dbo.FiscalYear
-- - dbo.Budget
-- - dbo.BudgetItem
-- - dbo.Expense
-- - dbo.CondominiumInstallment
-- - dbo.CondominiumFee
-- - dbo.AccountBalance
--
-- Prerequisiti:
-- 1) Eseguire: vzcondominio_migration_stage0.sql
-- 2) Eseguire: vzcondominio_migration_stage0_load_staging.sql
-- 3) Eseguire: vzcondominio_migration_stage1_masterdata.sql
-- 4) Mapping tenant attivo su dbo.MigrationTenantMap_VZ
--
-- NOTE IMPORTANTI:
-- - Idempotente: usa tabelle Map_VZ_* per evitare doppi inserimenti.
-- - Se manca mapping categoria->conto, usa il primo conto attivo del condominio.
-- - CondominiumFee.UserId viene valorizzato a 0 (da riconciliare in Stage 3 owners/tenants).
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @RunCode NVARCHAR(100) = CONCAT('VZ-STAGE2-', FORMAT(SYSDATETIME(), 'yyyyMMdd-HHmmss'));
DECLARE @CreatedById INT = 1;
DECLARE @CreatedByFullName NVARCHAR(200) = N'Migrazione VZ';

DECLARE @BudgetTypePreventivo INT = 1;
DECLARE @BudgetStatusDraft INT = 1;
DECLARE @ExpenseTypeAltro INT = 6;
DECLARE @ExpensePaymentStatusPaid INT = 2;
DECLARE @ExpensePaymentStatusToPay INT = 1;
DECLARE @InstallmentStatusOpen INT = 2;
DECLARE @FiscalYearStatusClosed INT = 4;
DECLARE @FiscalYearStatusOpen INT = 2;
DECLARE @ChargeabilityTypeOwner INT = 0;

BEGIN TRY
    BEGIN TRANSACTION;

    -- ------------------------------------------------------------
    -- A) Register migration run
    -- ------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM dbo.MigrationRun_VZ WHERE RunCode = @RunCode)
    BEGIN
        INSERT INTO dbo.MigrationRun_VZ (RunCode, StartedAt, Status, Notes)
        VALUES (@RunCode, SYSDATETIME(), N'Started', N'Stage 2 Financials');
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Map_VZ_Condominium)
    BEGIN
        RAISERROR('Map_VZ_Condominium vuota. Eseguire prima Stage 1.', 16, 1);
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Map_VZ_Unit)
    BEGIN
        RAISERROR('Map_VZ_Unit vuota. Eseguire prima Stage 1.', 16, 1);
    END

    IF EXISTS (
        SELECT 1
        FROM dbo.Map_VZ_Condominium m
        INNER JOIN dbo.Condominium c ON c.Id = m.DomuWaveId
        WHERE c.IsDeleted = 1
    )
    BEGIN
        RAISERROR('Map_VZ_Condominium contiene riferimenti a condomini IsDeleted=1. Correggere la mappa prima di eseguire Stage 2.', 16, 1);
    END

    -- ------------------------------------------------------------
    -- B) Mapping tables (se mancanti)
    -- ------------------------------------------------------------
    IF OBJECT_ID('dbo.Map_VZ_Budget', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Map_VZ_Budget (
            VZFiscalYearId      INT NOT NULL PRIMARY KEY,
            TenantId            UNIQUEIDENTIFIER NOT NULL,
            DomuWaveBudgetId    INT NOT NULL,
            MigratedAt          DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
            RunCode             NVARCHAR(100) NULL
        );
    END

    IF OBJECT_ID('dbo.Map_VZ_Installment', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Map_VZ_Installment (
            VZFiscalYearId          INT NOT NULL,
            DueDate                 DATE NOT NULL,
            Description             NVARCHAR(255) NOT NULL,
            TenantId                UNIQUEIDENTIFIER NOT NULL,
            DomuWaveInstallmentId   INT NOT NULL,
            MigratedAt              DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
            RunCode                 NVARCHAR(100) NULL,
            CONSTRAINT PK_Map_VZ_Installment PRIMARY KEY (VZFiscalYearId, DueDate, Description)
        );
    END

    IF OBJECT_ID('dbo.Map_VZ_Account', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Map_VZ_Account (
            TenantId            UNIQUEIDENTIFIER NOT NULL,
            CondominiumId       INT NOT NULL,
            VZMacroCategoryId   INT NOT NULL,
            VZCategoryId        INT NOT NULL,
            ChartOfAccountsId   INT NOT NULL,
            MigratedAt          DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
            RunCode             NVARCHAR(100) NULL,
            CONSTRAINT PK_Map_VZ_Account PRIMARY KEY (TenantId, CondominiumId, VZMacroCategoryId, VZCategoryId)
        );
    END

    -- ------------------------------------------------------------
    -- C) FiscalYear
    -- ------------------------------------------------------------
    ;WITH FiscalYearRanked AS (
        SELECT
            fy.VZId,
            mc.TenantId,
            mc.DomuWaveId AS CondominiumId,
            CONCAT('VZ-', fy.VZId) AS Code,
            fy.Description,
            CAST(fy.StartDate AS DATE) AS StartDate,
            CAST(fy.EndDate AS DATE) AS EndDate,
            ISNULL(fy.IsClosed, 0) AS IsClosed,
            ROW_NUMBER() OVER (
                PARTITION BY mc.TenantId, mc.DomuWaveId
                ORDER BY
                    CASE WHEN ISNULL(fy.IsClosed, 0) = 1 THEN 1 ELSE 0 END,
                    CAST(fy.EndDate AS DATE) DESC,
                    CAST(fy.StartDate AS DATE) DESC,
                    fy.VZId DESC
            ) AS OpenRank,
            (
                SELECT COUNT(1)
                FROM dbo.FiscalYear fyx
                WHERE fyx.TenantId = mc.TenantId
                  AND fyx.CondominiumId = mc.DomuWaveId
                  AND fyx.IsDeleted = 0
                  AND fyx.IsActive = 1
            ) AS ExistingActiveCount
        FROM stg.VZ_FiscalYear fy
        INNER JOIN dbo.Map_VZ_Condominium mc ON mc.VZId = fy.VZCondominiumId
    ), FiscalYearSource AS (
        SELECT
            r.VZId,
            r.TenantId,
            r.CondominiumId,
            r.Code,
            r.Description,
            r.StartDate,
            r.EndDate,
            CASE
                WHEN r.IsClosed = 1 THEN 0
                WHEN r.ExistingActiveCount > 0 THEN 0
                WHEN r.OpenRank = 1 THEN 1
                ELSE 0
            END AS IsActive,
            CASE WHEN ISNULL(r.IsClosed, 0) = 1 THEN @FiscalYearStatusClosed ELSE @FiscalYearStatusOpen END AS StatusId,
            r.IsClosed
        FROM FiscalYearRanked r
    )
    INSERT INTO dbo.FiscalYear (
        FiscalYearId, TenantId, CondominiumId, Code, Description,
        StartDate, EndDate, IsActive,
        ClosedDate, IsDeleted, CreationDate,
        CreatedBy, CreatedByFullName, StatusId
    )
    SELECT
        NEXT_ID.NewId,
        s.TenantId,
        s.CondominiumId,
        LEFT(s.Code, 50),
        LEFT(s.Description, 250),
        s.StartDate,
        s.EndDate,
        s.IsActive,
        CASE WHEN s.IsClosed = 1 THEN SYSDATETIME() ELSE NULL END,
        0,
        SYSDATETIME(),
        CAST(@CreatedById AS BIGINT),
        @CreatedByFullName,
        s.StatusId
    FROM FiscalYearSource s
    INNER JOIN (
        SELECT
            fs.VZId,
            CAST((SELECT ISNULL(MAX(FiscalYearId), 0) FROM dbo.FiscalYear) + ROW_NUMBER() OVER (ORDER BY fs.VZId) AS INT) AS NewId
        FROM FiscalYearSource fs
        LEFT JOIN dbo.Map_VZ_FiscalYear mf ON mf.VZId = fs.VZId
        WHERE mf.VZId IS NULL
    ) NEXT_ID ON NEXT_ID.VZId = s.VZId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Map_VZ_FiscalYear m WHERE m.VZId = s.VZId);

    INSERT INTO dbo.Map_VZ_FiscalYear (VZId, TenantId, DomuWaveId, RunCode)
    SELECT
        s.VZId,
        s.TenantId,
        d.FiscalYearId,
        @RunCode
    FROM (
        SELECT
            fy.VZId,
            mc.TenantId,
            mc.DomuWaveId AS CondominiumId,
            CONCAT('VZ-', fy.VZId) AS Code
        FROM stg.VZ_FiscalYear fy
        INNER JOIN dbo.Map_VZ_Condominium mc ON mc.VZId = fy.VZCondominiumId
    ) s
    INNER JOIN dbo.FiscalYear d
        ON d.TenantId = s.TenantId
       AND d.CondominiumId = s.CondominiumId
       AND d.Code = s.Code
       AND d.IsDeleted = 0
    LEFT JOIN dbo.Map_VZ_FiscalYear m
        ON m.VZId = s.VZId
    WHERE m.VZId IS NULL;

    -- Guardrail: evita "Completed" silenzioso quando non esistono dati finanziari
    -- per i fiscal year effettivamente mappati in questa migrazione.
    DECLARE @MappedBudgetRows INT;
    DECLARE @MappedExpenseRows INT;

    SELECT @MappedBudgetRows = COUNT(1)
    FROM stg.VZ_Budget b
    WHERE EXISTS (SELECT 1 FROM dbo.Map_VZ_FiscalYear mf WHERE mf.VZId = b.VZFiscalYearId);

    SELECT @MappedExpenseRows = COUNT(1)
    FROM stg.VZ_Expense e
    WHERE EXISTS (SELECT 1 FROM dbo.Map_VZ_FiscalYear mf WHERE mf.VZId = e.VZFiscalYearId);

    IF ISNULL(@MappedBudgetRows, 0) = 0
       AND ISNULL(@MappedExpenseRows, 0) = 0
    BEGIN
        RAISERROR('Nessun dato finanziario trovato per i fiscal year mappati (VZ_Budget/VZ_Expense). Verificare MigrationTenantMap_VZ e i riferimenti idEsercizio in sorgente.', 16, 1);
    END

     -- Ripulisce mappe budget non valide: target mancante o soft-deleted.
     -- In questo modo il budget viene ricreato e rimappato al prossimo inserimento.
     DELETE mb
     FROM dbo.Map_VZ_Budget mb
     LEFT JOIN dbo.Budget b
          ON b.Id = mb.DomuWaveBudgetId
         AND b.TenantId = mb.TenantId
     WHERE b.Id IS NULL
         OR b.IsDeleted = 1;

    -- ------------------------------------------------------------
    -- D) Budget (1 per esercizio VZ)
    -- ------------------------------------------------------------
    ;WITH BudgetSource AS (
        SELECT
            b.VZFiscalYearId,
            mf.TenantId,
            fy.CondominiumId,
            mf.DomuWaveId AS FiscalYearId,
            CAST(SUM(ISNULL(b.Amount, 0)) AS DECIMAL(18,4)) AS TotalExpenses,
            CAST(0 AS DECIMAL(18,4)) AS TotalIncome
        FROM stg.VZ_Budget b
        INNER JOIN dbo.Map_VZ_FiscalYear mf ON mf.VZId = b.VZFiscalYearId
        INNER JOIN dbo.FiscalYear fy ON fy.FiscalYearId = mf.DomuWaveId AND fy.IsDeleted = 0
        GROUP BY b.VZFiscalYearId, mf.TenantId, fy.CondominiumId, mf.DomuWaveId
    )
    INSERT INTO dbo.Budget (
        Id, TenantId, CondominiumId, FiscalYearId,
        TotalIncome, TotalExpenses, Notes,
        Type, StatusId,
        CreatedById, CreatedByFullName, CreationDate, IsDeleted
    )
    SELECT
        NEXT_ID.NewId,
        s.TenantId,
        s.CondominiumId,
        s.FiscalYearId,
        s.TotalIncome,
        s.TotalExpenses,
        CONCAT('Budget migrato da VZ - esercizio ', s.VZFiscalYearId),
        @BudgetTypePreventivo,
        @BudgetStatusDraft,
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME(),
        0
    FROM BudgetSource s
    INNER JOIN (
        SELECT
            bs.VZFiscalYearId,
            CAST((SELECT ISNULL(MAX(Id), 0) FROM dbo.Budget) + ROW_NUMBER() OVER (ORDER BY bs.VZFiscalYearId) AS INT) AS NewId
        FROM BudgetSource bs
        LEFT JOIN dbo.Map_VZ_Budget mb ON mb.VZFiscalYearId = bs.VZFiscalYearId
        WHERE mb.VZFiscalYearId IS NULL
    ) NEXT_ID ON NEXT_ID.VZFiscalYearId = s.VZFiscalYearId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Map_VZ_Budget m WHERE m.VZFiscalYearId = s.VZFiscalYearId);

    INSERT INTO dbo.Map_VZ_Budget (VZFiscalYearId, TenantId, DomuWaveBudgetId, RunCode)
    SELECT
        s.VZFiscalYearId,
        s.TenantId,
        b.Id,
        @RunCode
    FROM (
        SELECT
            src.VZFiscalYearId,
            src.TenantId,
            src.FiscalYearId
        FROM (
            SELECT
                b.VZFiscalYearId,
                mf.TenantId,
                mf.DomuWaveId AS FiscalYearId
            FROM stg.VZ_Budget b
            INNER JOIN dbo.Map_VZ_FiscalYear mf ON mf.VZId = b.VZFiscalYearId
            GROUP BY b.VZFiscalYearId, mf.TenantId, mf.DomuWaveId
        ) src
    ) s
    INNER JOIN dbo.Budget b
        ON b.TenantId = s.TenantId
       AND b.FiscalYearId = s.FiscalYearId
       AND b.Type = @BudgetTypePreventivo
       AND b.IsDeleted = 0
    LEFT JOIN dbo.Map_VZ_Budget m
        ON m.VZFiscalYearId = s.VZFiscalYearId
    WHERE m.VZFiscalYearId IS NULL;

    -- ------------------------------------------------------------
    -- E) Piano dei conti da MacroCategoria/Categoria VZ
    -- ------------------------------------------------------------
    ;WITH NeededMacro AS (
        SELECT DISTINCT
            mf.TenantId,
            b.VZMacroCategoryId,
            COALESCE(NULLIF(LTRIM(RTRIM(m.Name)), ''), CONCAT('Macro ', b.VZMacroCategoryId)) AS MacroName
        FROM stg.VZ_Budget b
        INNER JOIN dbo.Map_VZ_FiscalYear mf ON mf.VZId = b.VZFiscalYearId
        LEFT JOIN stg.VZ_ExpenseMacroCategory m ON m.VZMacroCategoryId = b.VZMacroCategoryId
    ), MacroToInsert AS (
        SELECT
            nm.TenantId,
            nm.VZMacroCategoryId,
            nm.MacroName,
            ROW_NUMBER() OVER (ORDER BY nm.TenantId, nm.VZMacroCategoryId) AS RowSeq
        FROM NeededMacro nm
        LEFT JOIN dbo.ChartOfAccountsCategory cat
            ON cat.TenantId = nm.TenantId
           AND cat.Name = nm.MacroName
           AND cat.IsDeleted = 0
        WHERE cat.Id IS NULL
    )
    INSERT INTO dbo.ChartOfAccountsCategory (
        Id, TenantId, Name, Description, IsActive, IsDeleted,
        CreatedById, CreatedByFullName, CreationDate
    )
    SELECT
        CAST((SELECT ISNULL(MAX(Id), 0) FROM dbo.ChartOfAccountsCategory) + m.RowSeq AS INT) AS NewId,
        m.TenantId,
        LEFT(m.MacroName, 200),
        CONCAT('Categoria da VZ macro ', m.VZMacroCategoryId),
        1,
        0,
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME()
    FROM MacroToInsert m;

    ;WITH ParentAccountSource AS (
        SELECT DISTINCT
            mf.TenantId,
            fy.CondominiumId,
            b.VZMacroCategoryId,
            COALESCE(NULLIF(LTRIM(RTRIM(m.Name)), ''), CONCAT('Macro ', b.VZMacroCategoryId)) AS MacroName,
            CONCAT('VZ.M', RIGHT(CONCAT('000', b.VZMacroCategoryId), 3)) AS ParentAccountCode
        FROM stg.VZ_Budget b
        INNER JOIN dbo.Map_VZ_FiscalYear mf ON mf.VZId = b.VZFiscalYearId
        INNER JOIN dbo.FiscalYear fy ON fy.FiscalYearId = mf.DomuWaveId AND fy.IsDeleted = 0
        LEFT JOIN stg.VZ_ExpenseMacroCategory m ON m.VZMacroCategoryId = b.VZMacroCategoryId
    ), ParentAccountToInsert AS (
        SELECT
            s.TenantId,
            s.CondominiumId,
            s.VZMacroCategoryId,
            s.MacroName,
            s.ParentAccountCode,
            cat.Id AS CategoryId,
            ROW_NUMBER() OVER (ORDER BY s.TenantId, s.CondominiumId, s.VZMacroCategoryId) AS RowSeq
        FROM ParentAccountSource s
        INNER JOIN dbo.ChartOfAccountsCategory cat
            ON cat.TenantId = s.TenantId
           AND cat.Name = s.MacroName
           AND cat.IsDeleted = 0
        LEFT JOIN dbo.ChartOfAccounts p
            ON p.TenantId = s.TenantId
           AND p.CondominiumId = s.CondominiumId
           AND p.Code = s.ParentAccountCode
           AND p.IsDeleted = 0
        WHERE p.Id IS NULL
    )
    INSERT INTO dbo.ChartOfAccounts (
        Id, TenantId, CondominiumId, Code, Name, Category, ParentAccountId, Level,
        Description, IsActive,
        CreatedById, CreatedByFullName, CreationDate, IsDeleted,
        Type, CategoryId, AllocationMethod, ChargeabilityType
    )
    SELECT
        CAST((SELECT ISNULL(MAX(Id), 0) FROM dbo.ChartOfAccounts) + p.RowSeq AS INT) AS NewId,
        p.TenantId,
        p.CondominiumId,
        LEFT(p.ParentAccountCode, 50),
        LEFT(p.MacroName, 200),
        LEFT(p.MacroName, 200),
        NULL,
        1,
        CONCAT('VZ Macro=', p.VZMacroCategoryId),
        1,
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME(),
        0,
        2,
        p.CategoryId,
        0,
        @ChargeabilityTypeOwner
    FROM ParentAccountToInsert p;

    ;WITH AccountSource AS (
        SELECT DISTINCT
            mf.TenantId,
            fy.CondominiumId,
            b.VZMacroCategoryId,
            b.VZCategoryId,
            COALESCE(NULLIF(LTRIM(RTRIM(m.Name)), ''), CONCAT('Macro ', b.VZMacroCategoryId)) AS MacroName,
            COALESCE(NULLIF(LTRIM(RTRIM(c.Name)), ''), CONCAT('Categoria ', b.VZCategoryId)) AS CategoryName,
            CONCAT('VZ.M', RIGHT(CONCAT('000', b.VZMacroCategoryId), 3)) AS ParentAccountCode,
            CONCAT('VZ.M', RIGHT(CONCAT('000', b.VZMacroCategoryId), 3), '.C', RIGHT(CONCAT('000', b.VZCategoryId), 3)) AS AccountCode
        FROM stg.VZ_Budget b
        INNER JOIN dbo.Map_VZ_FiscalYear mf ON mf.VZId = b.VZFiscalYearId
        INNER JOIN dbo.FiscalYear fy ON fy.FiscalYearId = mf.DomuWaveId AND fy.IsDeleted = 0
        LEFT JOIN stg.VZ_ExpenseMacroCategory m ON m.VZMacroCategoryId = b.VZMacroCategoryId
        LEFT JOIN stg.VZ_ExpenseCategory c ON c.VZCategoryId = b.VZCategoryId
    ), AccountToInsert AS (
        SELECT
            s.TenantId,
            s.CondominiumId,
            s.VZMacroCategoryId,
            s.VZCategoryId,
            s.MacroName,
            s.CategoryName,
            p.Id AS ParentAccountId,
            s.AccountCode,
            cat.Id AS CategoryId,
            ROW_NUMBER() OVER (ORDER BY s.TenantId, s.CondominiumId, s.VZMacroCategoryId, s.VZCategoryId) AS RowSeq
        FROM AccountSource s
        INNER JOIN dbo.ChartOfAccounts p
            ON p.TenantId = s.TenantId
           AND p.CondominiumId = s.CondominiumId
           AND p.Code = s.ParentAccountCode
           AND p.Level = 1
           AND p.IsDeleted = 0
        INNER JOIN dbo.ChartOfAccountsCategory cat
            ON cat.TenantId = s.TenantId
           AND cat.Name = s.MacroName
           AND cat.IsDeleted = 0
        LEFT JOIN dbo.ChartOfAccounts a
            ON a.TenantId = s.TenantId
           AND a.CondominiumId = s.CondominiumId
           AND a.Code = s.AccountCode
           AND a.IsDeleted = 0
        WHERE a.Id IS NULL
    )
    INSERT INTO dbo.ChartOfAccounts (
        Id, TenantId, CondominiumId, Code, Name, Category, ParentAccountId, Level,
        Description, IsActive,
        CreatedById, CreatedByFullName, CreationDate, IsDeleted,
        Type, CategoryId, AllocationMethod, ChargeabilityType
    )
    SELECT
        CAST((SELECT ISNULL(MAX(Id), 0) FROM dbo.ChartOfAccounts) + a.RowSeq AS INT) AS NewId,
        a.TenantId,
        a.CondominiumId,
        LEFT(a.AccountCode, 50),
        LEFT(a.CategoryName, 200),
        LEFT(a.MacroName, 200),
        a.ParentAccountId,
        2,
        CONCAT('VZ Macro=', a.VZMacroCategoryId, '; Categoria=', a.VZCategoryId),
        1,
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME(),
        0,
        2,
        a.CategoryId,
        0,
        @ChargeabilityTypeOwner
    FROM AccountToInsert a;

         -- Riallina gerarchia VZ al nuovo modello:
         -- - padre: livello 1, ParentAccountId NULL
         -- - figlio: livello 2, ParentAccountId valorizzato
         UPDATE p
             SET p.Level = 1,
                  p.ParentAccountId = NULL
         FROM dbo.ChartOfAccounts p
         WHERE p.IsDeleted = 0
            AND p.Code LIKE 'VZ.M%'
            AND p.Code NOT LIKE 'VZ.M%.C%'
            AND (p.Level <> 1 OR p.ParentAccountId IS NOT NULL);

        UPDATE c
             SET c.ParentAccountId = p.Id
        FROM dbo.ChartOfAccounts c
        INNER JOIN dbo.ChartOfAccounts p
                ON p.TenantId = c.TenantId
             AND p.CondominiumId = c.CondominiumId
             AND p.Code = LEFT(c.Code, CHARINDEX('.C', c.Code) - 1)
             AND p.Level = 1
             AND p.IsDeleted = 0
        WHERE c.IsDeleted = 0
            AND c.Code LIKE 'VZ.M%.C%'
            AND CHARINDEX('.C', c.Code) > 0
            AND (c.Level <> 2 OR c.ParentAccountId IS NULL OR c.ParentAccountId <> p.Id);

         UPDATE c
             SET c.Level = 2
         FROM dbo.ChartOfAccounts c
         WHERE c.IsDeleted = 0
            AND c.Code LIKE 'VZ.M%.C%'
            AND CHARINDEX('.C', c.Code) > 0
            AND c.Level <> 2;

            -- Ripulisce mappe account non valide: target mancante o soft-deleted.
            DELETE ma
            FROM dbo.Map_VZ_Account ma
            LEFT JOIN dbo.ChartOfAccounts a
                ON a.Id = ma.ChartOfAccountsId
               AND a.TenantId = ma.TenantId
               AND a.CondominiumId = ma.CondominiumId
            WHERE a.Id IS NULL
               OR a.IsDeleted = 1;

    INSERT INTO dbo.Map_VZ_Account (TenantId, CondominiumId, VZMacroCategoryId, VZCategoryId, ChartOfAccountsId, RunCode)
    SELECT
        s.TenantId,
        s.CondominiumId,
        s.VZMacroCategoryId,
        s.VZCategoryId,
        a.Id,
        @RunCode
    FROM (
        SELECT DISTINCT
            mf.TenantId,
            fy.CondominiumId,
            b.VZMacroCategoryId,
            b.VZCategoryId,
            CONCAT('VZ.M', RIGHT(CONCAT('000', b.VZMacroCategoryId), 3), '.C', RIGHT(CONCAT('000', b.VZCategoryId), 3)) AS AccountCode
        FROM stg.VZ_Budget b
        INNER JOIN dbo.Map_VZ_FiscalYear mf ON mf.VZId = b.VZFiscalYearId
        INNER JOIN dbo.FiscalYear fy ON fy.FiscalYearId = mf.DomuWaveId AND fy.IsDeleted = 0
    ) s
    INNER JOIN dbo.ChartOfAccounts a
        ON a.TenantId = s.TenantId
       AND a.CondominiumId = s.CondominiumId
       AND a.Code = s.AccountCode
       AND a.IsDeleted = 0
    LEFT JOIN dbo.Map_VZ_Account m
        ON m.TenantId = s.TenantId
       AND m.CondominiumId = s.CondominiumId
       AND m.VZMacroCategoryId = s.VZMacroCategoryId
       AND m.VZCategoryId = s.VZCategoryId
    WHERE m.ChartOfAccountsId IS NULL;

    -- ------------------------------------------------------------
    -- E2) BudgetItem
    -- Usa il mapping conto da MacroCategoria/Categoria; fallback al primo conto attivo
    -- ------------------------------------------------------------
    IF OBJECT_ID('tempdb..#BudgetItemSource') IS NOT NULL DROP TABLE #BudgetItemSource;
    IF OBJECT_ID('tempdb..#ParentBudgetItemSource') IS NOT NULL DROP TABLE #ParentBudgetItemSource;

    ;WITH DefaultAccount AS (
        SELECT
            c.Id AS CondominiumId,
            MIN(a.Id) AS AccountId
        FROM dbo.Condominium c
        INNER JOIN dbo.ChartOfAccounts a
            ON a.CondominiumId = c.Id
           AND a.IsDeleted = 0
           AND ISNULL(a.IsActive, 1) = 1
        GROUP BY c.Id
    )
    SELECT
        src.TenantId,
        src.BudgetId,
        src.AccountId,
        CAST(SUM(src.Amount) AS DECIMAL(18,4)) AS Amount,
        src.Description
    INTO #BudgetItemSource
    FROM (
        SELECT
            mb.TenantId,
            mb.DomuWaveBudgetId AS BudgetId,
            COALESCE(am.Id, da.AccountId) AS AccountId,
            CAST(ISNULL(b.Amount, 0) AS DECIMAL(18,4)) AS Amount,
            LEFT(CONCAT('Macro=', b.VZMacroCategoryId, '; Categoria=', b.VZCategoryId, '; Edificio=', b.VZBuildingId, '; Scala=', b.VZStaircaseId), 250) AS Description
        FROM stg.VZ_Budget b
        INNER JOIN dbo.Map_VZ_Budget mb ON mb.VZFiscalYearId = b.VZFiscalYearId
        INNER JOIN dbo.Budget db ON db.Id = mb.DomuWaveBudgetId
        INNER JOIN DefaultAccount da ON da.CondominiumId = db.CondominiumId
        LEFT JOIN dbo.Map_VZ_Account ma
            ON ma.TenantId = mb.TenantId
           AND ma.CondominiumId = db.CondominiumId
           AND ma.VZMacroCategoryId = b.VZMacroCategoryId
           AND ma.VZCategoryId = b.VZCategoryId
        LEFT JOIN dbo.ChartOfAccounts am
            ON am.Id = ma.ChartOfAccountsId
           AND am.IsDeleted = 0
    ) src
    GROUP BY src.TenantId, src.BudgetId, src.AccountId, src.Description;

    UPDATE bi
       SET bi.TenantId = s.TenantId,
           bi.AccountId = s.AccountId,
           bi.Amount = s.Amount,
           bi.Notes = N'Migrato da VZPreventivo',
           bi.LastUpdatedById = @CreatedById,
           bi.LastUpdatedByFullName = @CreatedByFullName,
           bi.LastUpdateDate = SYSDATETIME()
    FROM dbo.BudgetItem bi
    INNER JOIN #BudgetItemSource s
        ON s.BudgetId = bi.BudgetId
       AND s.Description = bi.Description
    WHERE bi.IsDeleted = 0;

    ;WITH BudgetItemPending AS (
        SELECT
            s.TenantId,
            s.BudgetId,
            s.AccountId,
            s.Amount,
            s.Description,
            ROW_NUMBER() OVER (ORDER BY s.BudgetId, s.Description, s.AccountId) AS RowSeq
        FROM #BudgetItemSource s
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.BudgetItem bi
            WHERE bi.BudgetId = s.BudgetId
              AND bi.Description = s.Description
              AND bi.IsDeleted = 0
        )
    )
    INSERT INTO dbo.BudgetItem (
        Id, TenantId, BudgetId, AccountId, Amount, Description, Notes,
        CreatedById, CreatedByFullName, CreationDate, IsDeleted
    )
    SELECT
        CAST((SELECT ISNULL(MAX(Id), 0) FROM dbo.BudgetItem) + p.RowSeq AS INT) AS NewId,
        p.TenantId,
        p.BudgetId,
        p.AccountId,
        p.Amount,
        p.Description,
        N'Migrato da VZPreventivo',
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME(),
        0
    FROM BudgetItemPending p;

    -- Inserisce righe a zero sui conti padre (livello 1) senza movimenti nel budget.
    SELECT DISTINCT
        mb.TenantId,
        mb.DomuWaveBudgetId AS BudgetId,
        a.Id AS AccountId,
        LEFT(CONCAT('Padre senza movimenti: ', a.Code, ' - ', a.Name), 250) AS Description
    INTO #ParentBudgetItemSource
    FROM dbo.Map_VZ_Budget mb
    INNER JOIN dbo.Budget b
        ON b.Id = mb.DomuWaveBudgetId
       AND b.TenantId = mb.TenantId
       AND b.IsDeleted = 0
    INNER JOIN dbo.ChartOfAccounts a
        ON a.TenantId = mb.TenantId
       AND a.CondominiumId = b.CondominiumId
       AND a.IsDeleted = 0
       AND a.Level = 1
       AND a.ParentAccountId IS NULL
       AND a.Code LIKE 'VZ.M%'
       AND a.Code NOT LIKE 'VZ.M%.C%';

    UPDATE bi
       SET bi.TenantId = s.TenantId,
           bi.Amount = CAST(0 AS DECIMAL(18,4)),
           bi.Description = s.Description,
           bi.Notes = N'Conto padre senza movimenti - import VZ',
           bi.LastUpdatedById = @CreatedById,
           bi.LastUpdatedByFullName = @CreatedByFullName,
           bi.LastUpdateDate = SYSDATETIME()
    FROM dbo.BudgetItem bi
    INNER JOIN #ParentBudgetItemSource s
        ON s.BudgetId = bi.BudgetId
       AND s.AccountId = bi.AccountId
    WHERE bi.IsDeleted = 0;

    ;WITH ParentBudgetItemPending AS (
        SELECT
            s.TenantId,
            s.BudgetId,
            s.AccountId,
            s.Description,
            ROW_NUMBER() OVER (ORDER BY s.BudgetId, s.AccountId) AS RowSeq
        FROM #ParentBudgetItemSource s
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.BudgetItem bi
            WHERE bi.BudgetId = s.BudgetId
              AND bi.AccountId = s.AccountId
              AND bi.IsDeleted = 0
        )
    )
    INSERT INTO dbo.BudgetItem (
        Id, TenantId, BudgetId, AccountId, Amount, Description, Notes,
        CreatedById, CreatedByFullName, CreationDate, IsDeleted
    )
    SELECT
        CAST((SELECT ISNULL(MAX(Id), 0) FROM dbo.BudgetItem) + p.RowSeq AS INT) AS NewId,
        p.TenantId,
        p.BudgetId,
        p.AccountId,
        CAST(0 AS DECIMAL(18,4)),
        p.Description,
        N'Conto padre senza movimenti - import VZ',
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME(),
        0
    FROM ParentBudgetItemPending p;

    DROP TABLE #BudgetItemSource;
    DROP TABLE #ParentBudgetItemSource;

    -- ------------------------------------------------------------
    -- F) Expense
    -- ------------------------------------------------------------
    ;WITH DefaultAccount AS (
        SELECT
            c.Id AS CondominiumId,
            MIN(a.Id) AS AccountId
        FROM dbo.Condominium c
        INNER JOIN dbo.ChartOfAccounts a
            ON a.CondominiumId = c.Id
           AND a.IsDeleted = 0
           AND ISNULL(a.IsActive, 1) = 1
        GROUP BY c.Id
    ), DefaultMillesimal AS (
        SELECT
            c.Id AS CondominiumId,
            MIN(mt.Id) AS MillesimalTableId
        FROM dbo.Condominium c
        INNER JOIN dbo.MillesimalTable mt
            ON mt.CondominiumId = c.Id
           AND mt.IsDeleted = 0
           AND UPPER(mt.Code) = N'DEFAULT'
        GROUP BY c.Id
    ), ExpenseSource AS (
        SELECT
            e.VZId,
            mf.TenantId,
            fy.CondominiumId,
            da.AccountId,
            ms.MillesimalTableId,
            msup.DomuWaveId AS SupplierId,
            inv.Number AS DocumentNumber,
            CAST(e.ExpenseDate AS DATETIME2) AS DocumentDate,
            CAST(e.ExpenseDate AS DATETIME2) AS RegistrationDate,
            e.Description,
            CAST(ISNULL(inv.TotalAmount, e.Amount) AS DECIMAL(18,4)) AS GrossAmount,
            CAST(ISNULL(inv.VatAmount, 0) AS DECIMAL(18,4)) AS VatAmount,
            CAST(ISNULL(inv.TotalAmount, e.Amount) - ISNULL(inv.VatAmount, 0) AS DECIMAL(18,4)) AS NetAmount,
            CAST(inv.PaymentDate AS DATETIME2) AS PaymentDate,
            CASE WHEN inv.PaymentDate IS NULL THEN @ExpensePaymentStatusToPay ELSE @ExpensePaymentStatusPaid END AS PaymentStatusId,
            @ExpenseTypeAltro AS ExpenseTypeId,
            CONCAT('VZExpenseId=', e.VZId, '; VZInvoiceId=', ISNULL(CONVERT(NVARCHAR(30), e.VZInvoiceId), 'NULL')) AS Notes,
            mf.DomuWaveId AS FiscalYearId
        FROM stg.VZ_Expense e
        INNER JOIN dbo.Map_VZ_FiscalYear mf ON mf.VZId = e.VZFiscalYearId
        INNER JOIN dbo.FiscalYear fy ON fy.FiscalYearId = mf.DomuWaveId AND fy.IsDeleted = 0
        INNER JOIN DefaultAccount da ON da.CondominiumId = fy.CondominiumId
        INNER JOIN DefaultMillesimal ms ON ms.CondominiumId = fy.CondominiumId
        LEFT JOIN stg.VZ_Invoice inv ON inv.VZId = e.VZInvoiceId
        LEFT JOIN dbo.Map_VZ_Supplier msup ON msup.VZId = inv.VZSupplierId
    )
    INSERT INTO dbo.Expense (
        Id, TenantId, CondominiumId, AccountId, SupplierId,
        DocumentNumber, DocumentDate, RegistrationDate,
        Description, GrossAmount, VatAmount, NetAmount,
        MillesimalTableId, PaymentDate, Notes,
        CreatedById, CreatedByFullName, CreationDate, IsDeleted,
        ExpenseTypeId, PaymentStatusId, FiscalYearId, ChargeabilityType
    )
    SELECT
        NEXT_ID.NewId,
        s.TenantId,
        s.CondominiumId,
        s.AccountId,
        s.SupplierId,
        LEFT(s.DocumentNumber, 100),
        s.DocumentDate,
        s.RegistrationDate,
        LEFT(s.Description, 500),
        s.GrossAmount,
        s.VatAmount,
        s.NetAmount,
        s.MillesimalTableId,
        s.PaymentDate,
        LEFT(s.Notes, 1000),
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME(),
        0,
        s.ExpenseTypeId,
        s.PaymentStatusId,
        s.FiscalYearId,
        @ChargeabilityTypeOwner
    FROM ExpenseSource s
    INNER JOIN (
        SELECT
            es.VZId,
            CAST((SELECT ISNULL(MAX(CAST(Id AS BIGINT)), 0) FROM dbo.Expense) + ROW_NUMBER() OVER (ORDER BY es.VZId) AS BIGINT) AS NewId
        FROM ExpenseSource es
        LEFT JOIN dbo.Map_VZ_Expense me ON me.VZId = es.VZId
        WHERE me.VZId IS NULL
    ) NEXT_ID ON NEXT_ID.VZId = s.VZId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Map_VZ_Expense m WHERE m.VZId = s.VZId);

    INSERT INTO dbo.Map_VZ_Expense (VZId, TenantId, DomuWaveId, RunCode)
    SELECT
        s.VZId,
        s.TenantId,
        d.Id,
        @RunCode
    FROM (
        SELECT
            e.VZId,
            mf.TenantId,
            mf.DomuWaveId AS FiscalYearId,
            CAST(e.ExpenseDate AS DATETIME2) AS DocumentDate,
            LEFT(e.Description, 500) AS Description,
            CAST(ISNULL(inv.TotalAmount, e.Amount) AS DECIMAL(18,4)) AS GrossAmount
        FROM stg.VZ_Expense e
        INNER JOIN dbo.Map_VZ_FiscalYear mf ON mf.VZId = e.VZFiscalYearId
        LEFT JOIN stg.VZ_Invoice inv ON inv.VZId = e.VZInvoiceId
    ) s
    INNER JOIN dbo.Expense d
        ON d.TenantId = s.TenantId
       AND d.FiscalYearId = s.FiscalYearId
       AND d.DocumentDate = s.DocumentDate
       AND d.Description = s.Description
       AND d.GrossAmount = s.GrossAmount
       AND d.IsDeleted = 0
    LEFT JOIN dbo.Map_VZ_Expense m
        ON m.VZId = s.VZId
    WHERE m.VZId IS NULL;

    -- ------------------------------------------------------------
    -- G) CondominiumInstallment
    -- ------------------------------------------------------------
    -- Import rate disabilitato: la sincronizzazione Stage 2 non popola
    -- CondominiumInstallment e non aggiorna Map_VZ_Installment.

    -- ------------------------------------------------------------
    -- H) CondominiumFee (riparto per millesimi da importi rata)
    -- ------------------------------------------------------------
    -- Import quote disabilitato: la sincronizzazione Stage 2 non popola
    -- CondominiumFee.

    -- ------------------------------------------------------------
    -- I) AccountBalance (apertura 0, chiusura = pagamenti - spese)
    -- ------------------------------------------------------------
    ;WITH DefaultAccount AS (
        SELECT
            c.Id AS CondominiumId,
            MIN(a.Id) AS AccountId
        FROM dbo.Condominium c
        INNER JOIN dbo.ChartOfAccounts a
            ON a.CondominiumId = c.Id
           AND a.IsDeleted = 0
           AND ISNULL(a.IsActive, 1) = 1
        GROUP BY c.Id
    ), ExpenseByYear AS (
        SELECT
            me.TenantId,
            e.FiscalYearId,
            SUM(e.GrossAmount) AS TotalExpense
        FROM dbo.Map_VZ_Expense me
        INNER JOIN dbo.Expense e ON e.Id = me.DomuWaveId AND e.IsDeleted = 0
        GROUP BY me.TenantId, e.FiscalYearId
    ), PaymentByYear AS (
        SELECT
            mf.TenantId,
            mf.DomuWaveId AS FiscalYearId,
            SUM(ISNULL(p.Amount, 0)) AS TotalPayment
        FROM stg.VZ_Payment p
        INNER JOIN dbo.Map_VZ_FiscalYear mf ON mf.VZId = p.VZFiscalYearId
        GROUP BY mf.TenantId, mf.DomuWaveId
    ), BalanceSource AS (
        SELECT
            fy.TenantId,
            fy.FiscalYearId,
            da.AccountId,
            CAST(0 AS DECIMAL(18,4)) AS OpeningBalance,
            CAST(ISNULL(py.TotalPayment, 0) - ISNULL(ey.TotalExpense, 0) AS DECIMAL(18,4)) AS ClosingBalance
        FROM dbo.FiscalYear fy
        INNER JOIN DefaultAccount da ON da.CondominiumId = fy.CondominiumId
        LEFT JOIN ExpenseByYear ey ON ey.TenantId = fy.TenantId AND ey.FiscalYearId = fy.FiscalYearId
        LEFT JOIN PaymentByYear py ON py.TenantId = fy.TenantId AND py.FiscalYearId = fy.FiscalYearId
                WHERE fy.IsDeleted = 0
                    AND EXISTS (SELECT 1 FROM dbo.Map_VZ_FiscalYear m WHERE m.DomuWaveId = fy.FiscalYearId)
    )
    INSERT INTO dbo.AccountBalance (
        Id, TenantId, FiscalYearId, ChartOfAccountsId,
        OpeningBalance, ClosingBalance,
        CreatedById, CreatedByFullName, CreationDate, IsDeleted,
        TotalBalance, TotalRoundingAdjustment
    )
    SELECT
        NEXT_ID.NewId,
        s.TenantId,
        s.FiscalYearId,
        s.AccountId,
        s.OpeningBalance,
        s.ClosingBalance,
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME(),
        0,
        s.ClosingBalance,
        0
    FROM BalanceSource s
    INNER JOIN (
        SELECT
            bs.TenantId,
            bs.FiscalYearId,
            bs.AccountId,
            CAST((SELECT ISNULL(MAX(Id), 0) FROM dbo.AccountBalance) + ROW_NUMBER() OVER (ORDER BY bs.TenantId, bs.FiscalYearId, bs.AccountId) AS INT) AS NewId
        FROM BalanceSource bs
        LEFT JOIN dbo.AccountBalance ab
            ON ab.TenantId = bs.TenantId
           AND ab.FiscalYearId = bs.FiscalYearId
           AND ab.ChartOfAccountsId = bs.AccountId
           AND ab.IsDeleted = 0
        WHERE ab.Id IS NULL
    ) NEXT_ID
        ON NEXT_ID.TenantId = s.TenantId
       AND NEXT_ID.FiscalYearId = s.FiscalYearId
       AND NEXT_ID.AccountId = s.AccountId
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.AccountBalance ab
        WHERE ab.TenantId = s.TenantId
          AND ab.FiscalYearId = s.FiscalYearId
          AND ab.ChartOfAccountsId = s.AccountId
          AND ab.IsDeleted = 0
    );

    -- ------------------------------------------------------------
    -- J) Finalize run
    -- ------------------------------------------------------------
    UPDATE dbo.MigrationRun_VZ
       SET EndedAt = SYSDATETIME(),
           Status = N'Completed'
     WHERE RunCode = @RunCode;

    COMMIT TRANSACTION;

    SELECT 'Map_VZ_FiscalYear' AS MapName, COUNT(*) AS MappedRows FROM dbo.Map_VZ_FiscalYear
    UNION ALL
    SELECT 'Map_VZ_Budget', COUNT(*) FROM dbo.Map_VZ_Budget
    UNION ALL
    SELECT 'Map_VZ_Expense', COUNT(*) FROM dbo.Map_VZ_Expense;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrLine INT = ERROR_LINE();

    UPDATE dbo.MigrationRun_VZ
       SET EndedAt = SYSDATETIME(),
           Status = N'Failed',
           Notes = CONCAT(ISNULL(Notes, ''), ' | Error line ', @ErrLine, ': ', @ErrMsg)
     WHERE RunCode = @RunCode;

    RAISERROR('Stage 2 failed at line %d: %s', 16, 1, @ErrLine, @ErrMsg);
END CATCH;
GO
