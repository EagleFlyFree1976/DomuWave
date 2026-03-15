-- ============================================================
-- Migration: VZCondominio -> DomuWave (Stored Procedures)
-- Contiene:
-- - dbo.sp_migration_stage1_masterdata
-- - dbo.sp_migration_stage2_financials
-- - dbo.sp_migration_run_steps
--
-- Nota: Stage 0 load e' gestito da dbo.sp_migration_stage0_stagedata.
-- ============================================================

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_migration_stage1_masterdata
    @RunCode NVARCHAR(100) = NULL,
    @CreatedById INT = 1,
    @CreatedByFullName NVARCHAR(200) = N'Migrazione VZ'
AS
BEGIN
    IF @RunCode IS NULL
        SET @RunCode = CONCAT('VZ-STAGE1-', FORMAT(SYSDATETIME(), 'yyyyMMdd-HHmmss'));

-- ============================================================
-- Migration: VZCondominio -> DomuWave (Stage 1 Master Data)
-- Target:
-- - dbo.Condominium
-- - dbo.CondominiumAddress
-- - dbo.RealEstateUnit
-- - dbo.Supplier
--
-- Prerequisiti:
-- 1) Eseguire prima: vzcondominio_migration_stage0.sql
-- 2) Popolare tabelle stg.VZ_*
-- 3) Popolare dbo.MigrationTenantMap_VZ (VZCondominioId -> TenantId)
--
-- NOTE:
-- - Script idempotente: usa tabelle Map_VZ_* per evitare doppi inserimenti.
-- - Non aggiorna record giÃ  migrati; inserisce solo i mancanti.
-- - Il mapping fornitori Stage 1 assume tenant unico nel db sorgente.
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;


BEGIN TRY
    BEGIN TRANSACTION;

    -- ------------------------------------------------------------
    -- A) Register migration run
    -- ------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM dbo.MigrationRun_VZ WHERE RunCode = @RunCode)
    BEGIN
        INSERT INTO dbo.MigrationRun_VZ (RunCode, StartedAt, Status, Notes)
        VALUES (@RunCode, SYSDATETIME(), N'Started', N'Stage 1 Master Data');
    END

    -- Guardrail: mapping tenant obbligatorio
    IF NOT EXISTS (SELECT 1 FROM dbo.MigrationTenantMap_VZ WHERE IsActive = 1)
    BEGIN
        RAISERROR('MigrationTenantMap_VZ e'' vuota. Popolare prima la mappatura VZCondominioId -> TenantId.', 16, 1);
    END

    -- ------------------------------------------------------------
    -- B) Condominium
    -- ------------------------------------------------------------
    ;WITH CondoSource AS (
        SELECT
            c.VZId,
            tm.TenantId,
            c.Name,
            NULLIF(LTRIM(RTRIM(c.TaxCode)), '') AS TaxCode,
            c.Notes,
            CAST(ISNULL(u.UnitCount, 0) AS INT) AS NumberOfUnits,
            CAST(ISNULL(u.StaircaseCount, 1) AS INT) AS NumberOfStaircases,
            CAST(ISNULL(u.TotalMillesimal, 1000.0000) AS DECIMAL(18,4)) AS TotalMillesimal
        FROM stg.VZ_Condominium c
        INNER JOIN dbo.MigrationTenantMap_VZ tm
            ON tm.VZCondominioId = c.VZId
           AND tm.IsActive = 1
        OUTER APPLY (
            SELECT
                COUNT(1) AS UnitCount,
                COUNT(DISTINCT su.VZStaircaseId) AS StaircaseCount,
                SUM(ISNULL(su.MillesimalGeneral, 0)) AS TotalMillesimal
            FROM stg.VZ_Building sb
            INNER JOIN stg.VZ_Unit su ON su.VZBuildingId = sb.VZId
            WHERE sb.VZCondominiumId = c.VZId
        ) u
    )
    INSERT INTO dbo.Condominium (
        Id, TenantId, Name, Code, TaxCode, Notes,
        NumberOfUnits, NumberOfStaircases, TotalMillesimal,
        InstallmentFrequency, InstallmentDueDay,
        IsActive, CreatedById, CreatedByFullName, CreationDate, IsDeleted
    )
    SELECT
        NEXT_ID.NewId,
        s.TenantId,
        LEFT(s.Name, 200),
        CONCAT('VZ-', s.VZId),
        LEFT(s.TaxCode, 50),
        CASE
            WHEN s.Notes IS NULL OR s.Notes = '' THEN CONCAT('[VZ ID=', s.VZId, ']')
            ELSE CONCAT(LEFT(s.Notes, 1800), ' [VZ ID=', s.VZId, ']')
        END,
        ISNULL(NULLIF(s.NumberOfUnits, 0), 1),
        ISNULL(NULLIF(s.NumberOfStaircases, 0), 1),
        CASE WHEN ISNULL(s.TotalMillesimal, 0) <= 0 THEN 1000.0000 ELSE s.TotalMillesimal END,
        N'Monthly',
        10,
        1,
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME(),
        0
    FROM CondoSource s
    INNER JOIN (
        SELECT
            s2.VZId,
            (SELECT ISNULL(MAX(Id), 0) FROM dbo.Condominium) + ROW_NUMBER() OVER (ORDER BY s2.VZId) AS NewId
        FROM CondoSource s2
        LEFT JOIN dbo.Map_VZ_Condominium m2 ON m2.VZId = s2.VZId
        WHERE m2.VZId IS NULL
    ) NEXT_ID ON NEXT_ID.VZId = s.VZId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Map_VZ_Condominium m WHERE m.VZId = s.VZId);

    -- Mapping Condominium legacy -> target
    INSERT INTO dbo.Map_VZ_Condominium (VZId, TenantId, DomuWaveId, RunCode)
    SELECT
        c.VZId,
        tm.TenantId,
        d.Id,
        @RunCode
    FROM stg.VZ_Condominium c
    INNER JOIN dbo.MigrationTenantMap_VZ tm
        ON tm.VZCondominioId = c.VZId
       AND tm.IsActive = 1
    INNER JOIN dbo.Condominium d
        ON d.TenantId = tm.TenantId
       AND d.Code = CONCAT('VZ-', c.VZId)
       AND d.IsDeleted = 0
    LEFT JOIN dbo.Map_VZ_Condominium m
        ON m.VZId = c.VZId
    WHERE m.VZId IS NULL;

    -- ------------------------------------------------------------
    -- C) CondominiumAddress (1 record per condominium da primo edificio)
    -- ------------------------------------------------------------
    ;WITH FirstBuilding AS (
        SELECT
            b.VZCondominiumId,
            b.Street,
            b.City,
            b.Province,
            b.PostalCode,
            b.Country,
            ROW_NUMBER() OVER (PARTITION BY b.VZCondominiumId ORDER BY b.VZId) AS rn
        FROM stg.VZ_Building b
    ), AddressSource AS (
        SELECT
            m.DomuWaveId AS CondominiumId,
            m.TenantId,
            ISNULL(NULLIF(LTRIM(RTRIM(fb.Street)), ''), N'Sconosciuto') AS Street,
            N'SN' AS StreetNumber,
            ISNULL(NULLIF(LTRIM(RTRIM(fb.PostalCode)), ''), N'00000') AS PostalCode,
            ISNULL(NULLIF(LTRIM(RTRIM(fb.City)), ''), N'Sconosciuta') AS City,
            ISNULL(NULLIF(LTRIM(RTRIM(fb.Province)), ''), N'ND') AS Province,
            ISNULL(NULLIF(LTRIM(RTRIM(fb.Country)), ''), N'IT') AS Country
        FROM dbo.Map_VZ_Condominium m
        LEFT JOIN FirstBuilding fb
            ON fb.VZCondominiumId = m.VZId
           AND fb.rn = 1
    )
    INSERT INTO dbo.CondominiumAddress (
        Id, TenantId, CondominiumId,
        Street, StreetNumber, PostalCode, City, Province, Country,
        CreatedById, CreatedByFullName, CreationDate, IsDeleted
    )
    SELECT
        NEXT_ID.NewId,
        s.TenantId,
        s.CondominiumId,
        LEFT(s.Street, 200),
        LEFT(s.StreetNumber, 20),
        LEFT(s.PostalCode, 10),
        LEFT(s.City, 100),
        LEFT(s.Province, 10),
        LEFT(s.Country, 50),
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME(),
        0
    FROM AddressSource s
    INNER JOIN (
        SELECT
            a.CondominiumId,
            (SELECT ISNULL(MAX(Id), 0) FROM dbo.CondominiumAddress) + ROW_NUMBER() OVER (ORDER BY a.CondominiumId) AS NewId
        FROM AddressSource a
        LEFT JOIN dbo.CondominiumAddress ca ON ca.CondominiumId = a.CondominiumId AND ca.IsDeleted = 0
        WHERE ca.Id IS NULL
    ) NEXT_ID ON NEXT_ID.CondominiumId = s.CondominiumId
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.CondominiumAddress ca
        WHERE ca.CondominiumId = s.CondominiumId
          AND ca.IsDeleted = 0
    );

    -- ------------------------------------------------------------
    -- D) RealEstateUnit
    -- ------------------------------------------------------------
    ;WITH UnitSourceBase AS (
        SELECT
            u.VZId,
            mc.TenantId,
            mc.DomuWaveId AS CondominiumId,
            ISNULL(NULLIF(LTRIM(RTRIM(sc.Name)), ''), N'SD') AS StaircaseBase,
            ISNULL(u.FloorNumber, 0) AS FloorNumber,
            LEFT(ISNULL(NULLIF(LTRIM(RTRIM(u.Description)), ''), CONCAT('Unita-', u.VZId)), 50) AS InternalNumber,
            CAST(u.CadastralSub AS NVARCHAR(50)) AS Subordinate,
            CAST(u.MillesimalGeneral AS DECIMAL(18,4)) AS CadastralIncome,
            NULL AS AreaSqm,
            NULL AS Rooms,
            u.Notes,
            ROW_NUMBER() OVER (
                PARTITION BY mc.DomuWaveId,
                    ISNULL(NULLIF(LTRIM(RTRIM(sc.Name)), ''), N'SD'),
                    ISNULL(u.FloorNumber, 0),
                    LEFT(ISNULL(NULLIF(LTRIM(RTRIM(u.Description)), ''), CONCAT('Unita-', u.VZId)), 50)
                ORDER BY u.VZId
            ) AS DupRank
        FROM stg.VZ_Unit u
        INNER JOIN stg.VZ_Building b ON b.VZId = u.VZBuildingId
        INNER JOIN dbo.Map_VZ_Condominium mc ON mc.VZId = b.VZCondominiumId
        LEFT JOIN stg.VZ_Staircase sc ON sc.VZId = u.VZStaircaseId
    ),
    UnitSource AS (
        SELECT
            VZId, TenantId, CondominiumId, FloorNumber, InternalNumber,
            Subordinate, CadastralIncome, AreaSqm, Rooms, Notes,
            LEFT(
                CASE WHEN DupRank > 1
                     THEN CONCAT(StaircaseBase, N'-', CAST(DupRank AS NVARCHAR(5)))
                     ELSE StaircaseBase
                END, 50
            ) AS Staircase
        FROM UnitSourceBase
    )
    INSERT INTO dbo.RealEstateUnit (
        Id, TenantId, CondominiumId, Staircase, Floor, InternalNumber, Subordinate,
        CadastralIncome, AreaSqm, Rooms, UnitType, OccupancyStatus,
        Notes, IsActive,
        CreatedById, CreatedByFullName, CreationDate, IsDeleted,
        NumeroAbitanti, DisplayName
    )
    SELECT
        NEXT_ID.NewId,
        s.TenantId,
        s.CondominiumId,
        LEFT(s.Staircase, 50),
        s.FloorNumber,
        s.InternalNumber,
        LEFT(s.Subordinate, 50),
        s.CadastralIncome,
        s.AreaSqm,
        s.Rooms,
        N'Residential',
        N'Occupied',
        CASE
            WHEN s.Notes IS NULL OR s.Notes = '' THEN CONCAT('[VZ ID=', s.VZId, ']')
            ELSE CONCAT(LEFT(s.Notes, 1800), ' [VZ ID=', s.VZId, ']')
        END,
        1,
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME(),
        0,
        1,
        CONCAT(LEFT(s.Staircase, 20), '-', LEFT(s.InternalNumber, 50))
    FROM UnitSource s
    INNER JOIN (
        SELECT
            s2.VZId,
            (SELECT ISNULL(MAX(Id), 0) FROM dbo.RealEstateUnit) + ROW_NUMBER() OVER (ORDER BY s2.VZId) AS NewId
        FROM UnitSource s2
        LEFT JOIN dbo.Map_VZ_Unit m2 ON m2.VZId = s2.VZId
        WHERE m2.VZId IS NULL
    ) NEXT_ID ON NEXT_ID.VZId = s.VZId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Map_VZ_Unit m WHERE m.VZId = s.VZId);

    -- Mapping Unit legacy -> target
    ;WITH MapSourceBase AS (
        SELECT
            su.VZId,
            mc.TenantId,
            mc.DomuWaveId AS CondominiumId,
            ISNULL(NULLIF(LTRIM(RTRIM(sc.Name)), ''), N'SD') AS StaircaseBase,
            ISNULL(su.FloorNumber, 0) AS FloorNumber,
            LEFT(ISNULL(NULLIF(LTRIM(RTRIM(su.Description)), ''), CONCAT('Unita-', su.VZId)), 50) AS InternalNumber,
            ROW_NUMBER() OVER (
                PARTITION BY mc.DomuWaveId,
                    ISNULL(NULLIF(LTRIM(RTRIM(sc.Name)), ''), N'SD'),
                    ISNULL(su.FloorNumber, 0),
                    LEFT(ISNULL(NULLIF(LTRIM(RTRIM(su.Description)), ''), CONCAT('Unita-', su.VZId)), 50)
                ORDER BY su.VZId
            ) AS DupRank
        FROM stg.VZ_Unit su
        INNER JOIN stg.VZ_Building sb ON sb.VZId = su.VZBuildingId
        INNER JOIN dbo.Map_VZ_Condominium mc ON mc.VZId = sb.VZCondominiumId
        LEFT JOIN stg.VZ_Staircase sc ON sc.VZId = su.VZStaircaseId
    ),
    MapSource AS (
        SELECT
            VZId, TenantId, CondominiumId, FloorNumber, InternalNumber,
            LEFT(
                CASE WHEN DupRank > 1
                     THEN CONCAT(StaircaseBase, N'-', CAST(DupRank AS NVARCHAR(5)))
                     ELSE StaircaseBase
                END, 50
            ) AS Staircase
        FROM MapSourceBase
    )
    INSERT INTO dbo.Map_VZ_Unit (VZId, TenantId, DomuWaveId, RunCode)
    SELECT
        s.VZId,
        s.TenantId,
        u.Id,
        @RunCode
    FROM MapSource s
    INNER JOIN dbo.RealEstateUnit u
        ON u.TenantId = s.TenantId
       AND u.CondominiumId = s.CondominiumId
       AND u.Staircase = s.Staircase
       AND u.Floor = s.FloorNumber
       AND u.InternalNumber = s.InternalNumber
       AND u.IsDeleted = 0
    LEFT JOIN dbo.Map_VZ_Unit m
        ON m.VZId = s.VZId
    WHERE m.VZId IS NULL;

    -- ------------------------------------------------------------
    -- E) MillesimalTable + UnitMillesimal
    -- ------------------------------------------------------------
    ;WITH MillesimalTableSource AS (
        SELECT
            mc.TenantId,
            mc.DomuWaveId AS CondominiumId,
            N'DEFAULT' AS Code,
            N'default' AS Name,
            N'Tabella millesimale importata da VZ - millesimi generali' AS Description,
            CAST(
                CASE
                    WHEN SUM(ISNULL(u.MillesimalGeneral, 0)) <= 0 THEN 1000.0000
                    ELSE SUM(ISNULL(u.MillesimalGeneral, 0))
                END AS DECIMAL(18,4)
            ) AS TotalMillesimal
        FROM dbo.Map_VZ_Condominium mc
        LEFT JOIN stg.VZ_Building b ON b.VZCondominiumId = mc.VZId
        LEFT JOIN stg.VZ_Unit u ON u.VZBuildingId = b.VZId
        GROUP BY mc.TenantId, mc.DomuWaveId

        UNION ALL

        SELECT
            mc.TenantId,
            mc.DomuWaveId AS CondominiumId,
            N'ASCENSORE' AS Code,
            N'Ascensore' AS Name,
            N'Tabella millesimale importata da VZ - millesimi ascensore' AS Description,
            CAST(
                CASE
                    WHEN SUM(ISNULL(u.MillesimalLift, 0)) <= 0 THEN 1000.0000
                    ELSE SUM(ISNULL(u.MillesimalLift, 0))
                END AS DECIMAL(18,4)
            ) AS TotalMillesimal
        FROM dbo.Map_VZ_Condominium mc
        LEFT JOIN stg.VZ_Building b ON b.VZCondominiumId = mc.VZId
        LEFT JOIN stg.VZ_Unit u ON u.VZBuildingId = b.VZId
        GROUP BY mc.TenantId, mc.DomuWaveId

        UNION ALL

        SELECT
            mc.TenantId,
            mc.DomuWaveId AS CondominiumId,
            N'MILLESIMISCALE' AS Code,
            N'millesimiscale' AS Name,
            N'Tabella millesimale importata da VZ - millesimi scala' AS Description,
            CAST(
                CASE
                    WHEN SUM(ISNULL(u.MillesimalStaircase, 0)) <= 0 THEN 1000.0000
                    ELSE SUM(ISNULL(u.MillesimalStaircase, 0))
                END AS DECIMAL(18,4)
            ) AS TotalMillesimal
        FROM dbo.Map_VZ_Condominium mc
        LEFT JOIN stg.VZ_Building b ON b.VZCondominiumId = mc.VZId
        LEFT JOIN stg.VZ_Unit u ON u.VZBuildingId = b.VZId
        GROUP BY mc.TenantId, mc.DomuWaveId
    )
    UPDATE mt
       SET mt.TenantId = s.TenantId,
           mt.Name = s.Name,
           mt.Description = s.Description,
           mt.TotalMillesimal = s.TotalMillesimal,
           mt.IsActive = 1,
           mt.IsEnabled = 1,
           mt.IsDraft = 0,
           mt.LastUpdatedById = @CreatedById,
           mt.LastUpdatedByFullName = @CreatedByFullName,
           mt.LastUpdateDate = SYSDATETIME(),
           mt.IsDeleted = 0
    FROM dbo.MillesimalTable mt
    INNER JOIN MillesimalTableSource s
        ON s.CondominiumId = mt.CondominiumId
       AND s.Code = mt.Code;

    ;WITH MillesimalTableSource AS (
        SELECT
            mc.TenantId,
            mc.DomuWaveId AS CondominiumId,
            N'DEFAULT' AS Code,
            N'default' AS Name,
            N'Tabella millesimale importata da VZ - millesimi generali' AS Description,
            CAST(
                CASE
                    WHEN SUM(ISNULL(u.MillesimalGeneral, 0)) <= 0 THEN 1000.0000
                    ELSE SUM(ISNULL(u.MillesimalGeneral, 0))
                END AS DECIMAL(18,4)
            ) AS TotalMillesimal
        FROM dbo.Map_VZ_Condominium mc
        LEFT JOIN stg.VZ_Building b ON b.VZCondominiumId = mc.VZId
        LEFT JOIN stg.VZ_Unit u ON u.VZBuildingId = b.VZId
        GROUP BY mc.TenantId, mc.DomuWaveId

        UNION ALL

        SELECT
            mc.TenantId,
            mc.DomuWaveId AS CondominiumId,
            N'ASCENSORE' AS Code,
            N'Ascensore' AS Name,
            N'Tabella millesimale importata da VZ - millesimi ascensore' AS Description,
            CAST(
                CASE
                    WHEN SUM(ISNULL(u.MillesimalLift, 0)) <= 0 THEN 1000.0000
                    ELSE SUM(ISNULL(u.MillesimalLift, 0))
                END AS DECIMAL(18,4)
            ) AS TotalMillesimal
        FROM dbo.Map_VZ_Condominium mc
        LEFT JOIN stg.VZ_Building b ON b.VZCondominiumId = mc.VZId
        LEFT JOIN stg.VZ_Unit u ON u.VZBuildingId = b.VZId
        GROUP BY mc.TenantId, mc.DomuWaveId

        UNION ALL

        SELECT
            mc.TenantId,
            mc.DomuWaveId AS CondominiumId,
            N'MILLESIMISCALE' AS Code,
            N'millesimiscale' AS Name,
            N'Tabella millesimale importata da VZ - millesimi scala' AS Description,
            CAST(
                CASE
                    WHEN SUM(ISNULL(u.MillesimalStaircase, 0)) <= 0 THEN 1000.0000
                    ELSE SUM(ISNULL(u.MillesimalStaircase, 0))
                END AS DECIMAL(18,4)
            ) AS TotalMillesimal
        FROM dbo.Map_VZ_Condominium mc
        LEFT JOIN stg.VZ_Building b ON b.VZCondominiumId = mc.VZId
        LEFT JOIN stg.VZ_Unit u ON u.VZBuildingId = b.VZId
        GROUP BY mc.TenantId, mc.DomuWaveId
    )
    INSERT INTO dbo.MillesimalTable (
        Id, TenantId, CondominiumId, Code, Name, Description, TotalMillesimal,
        IsActive, CreatedById, CreatedByFullName, CreationDate, IsDeleted, IsDraft, IsEnabled
    )
    SELECT
        NEXT_ID.NewId,
        s.TenantId,
        s.CondominiumId,
        s.Code,
        s.Name,
        s.Description,
        s.TotalMillesimal,
        1,
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME(),
        0,
        0,
        1
    FROM MillesimalTableSource s
    INNER JOIN (
        SELECT
            s2.CondominiumId,
            s2.Code,
            (SELECT ISNULL(MAX(Id), 0) FROM dbo.MillesimalTable) + ROW_NUMBER() OVER (ORDER BY s2.CondominiumId, s2.Code) AS NewId
        FROM MillesimalTableSource s2
        LEFT JOIN dbo.MillesimalTable mt2
            ON mt2.CondominiumId = s2.CondominiumId
           AND mt2.Code = s2.Code
        WHERE mt2.Id IS NULL
    ) NEXT_ID
        ON NEXT_ID.CondominiumId = s.CondominiumId
       AND NEXT_ID.Code = s.Code;

        UPDATE um
             SET um.LastUpdatedById = @CreatedById,
                     um.LastUpdatedByFullName = @CreatedByFullName,
                     um.LastUpdateDate = SYSDATETIME(),
                     um.IsDeleted = 1
        FROM dbo.UnitMillesimal um
        INNER JOIN dbo.MillesimalTable mt ON mt.Id = um.MillesimalTableId
        INNER JOIN dbo.Map_VZ_Condominium mc
                ON mc.DomuWaveId = mt.CondominiumId
             AND mc.TenantId = mt.TenantId
        WHERE mt.Code IN (N'DEF', N'MILDEF')
            AND mt.IsDeleted = 0
            AND um.IsDeleted = 0
            AND EXISTS (
                        SELECT 1
                        FROM dbo.MillesimalTable mt2
                        WHERE mt2.CondominiumId = mt.CondominiumId
                            AND mt2.TenantId = mt.TenantId
                            AND mt2.Code = N'DEFAULT'
                            AND mt2.IsDeleted = 0
            );

        UPDATE mt
             SET mt.LastUpdatedById = @CreatedById,
                     mt.LastUpdatedByFullName = @CreatedByFullName,
                     mt.LastUpdateDate = SYSDATETIME(),
                     mt.IsDeleted = 1
        FROM dbo.MillesimalTable mt
        INNER JOIN dbo.Map_VZ_Condominium mc
                ON mc.DomuWaveId = mt.CondominiumId
             AND mc.TenantId = mt.TenantId
        WHERE mt.Code IN (N'DEF', N'MILDEF')
            AND mt.IsDeleted = 0
            AND EXISTS (
                        SELECT 1
                        FROM dbo.MillesimalTable mt2
                        WHERE mt2.CondominiumId = mt.CondominiumId
                            AND mt2.TenantId = mt.TenantId
                            AND mt2.Code = N'DEFAULT'
                            AND mt2.IsDeleted = 0
            );

    ;WITH UnitMillesimalSource AS (
        SELECT
            mu.TenantId,
            mt.Id AS MillesimalTableId,
            mu.DomuWaveId AS UnitId,
            CAST(ISNULL(su.MillesimalGeneral, 0) AS DECIMAL(18,4)) AS Millesimal,
            N'Import VZ - millesimi generali' AS Notes
        FROM dbo.Map_VZ_Unit mu
        INNER JOIN stg.VZ_Unit su ON su.VZId = mu.VZId
        INNER JOIN stg.VZ_Building sb ON sb.VZId = su.VZBuildingId
        INNER JOIN dbo.Map_VZ_Condominium mc
            ON mc.VZId = sb.VZCondominiumId
           AND mc.TenantId = mu.TenantId
        INNER JOIN dbo.MillesimalTable mt
            ON mt.CondominiumId = mc.DomuWaveId
           AND mt.TenantId = mc.TenantId
           AND mt.Code = N'DEFAULT'
        WHERE ISNULL(su.MillesimalGeneral, 0) > 0

        UNION ALL

        SELECT
            mu.TenantId,
            mt.Id AS MillesimalTableId,
            mu.DomuWaveId AS UnitId,
            CAST(ISNULL(su.MillesimalLift, 0) AS DECIMAL(18,4)) AS Millesimal,
            N'Import VZ - millesimi ascensore' AS Notes
        FROM dbo.Map_VZ_Unit mu
        INNER JOIN stg.VZ_Unit su ON su.VZId = mu.VZId
        INNER JOIN stg.VZ_Building sb ON sb.VZId = su.VZBuildingId
        INNER JOIN dbo.Map_VZ_Condominium mc
            ON mc.VZId = sb.VZCondominiumId
           AND mc.TenantId = mu.TenantId
        INNER JOIN dbo.MillesimalTable mt
            ON mt.CondominiumId = mc.DomuWaveId
           AND mt.TenantId = mc.TenantId
           AND mt.Code = N'ASCENSORE'
        WHERE ISNULL(su.MillesimalLift, 0) > 0

        UNION ALL

        SELECT
            mu.TenantId,
            mt.Id AS MillesimalTableId,
            mu.DomuWaveId AS UnitId,
            CAST(ISNULL(su.MillesimalStaircase, 0) AS DECIMAL(18,4)) AS Millesimal,
            N'Import VZ - millesimi scala' AS Notes
        FROM dbo.Map_VZ_Unit mu
        INNER JOIN stg.VZ_Unit su ON su.VZId = mu.VZId
        INNER JOIN stg.VZ_Building sb ON sb.VZId = su.VZBuildingId
        INNER JOIN dbo.Map_VZ_Condominium mc
            ON mc.VZId = sb.VZCondominiumId
           AND mc.TenantId = mu.TenantId
        INNER JOIN dbo.MillesimalTable mt
            ON mt.CondominiumId = mc.DomuWaveId
           AND mt.TenantId = mc.TenantId
           AND mt.Code = N'MILLESIMISCALE'
        WHERE ISNULL(su.MillesimalStaircase, 0) > 0
    )
    UPDATE um
       SET um.TenantId = s.TenantId,
           um.Millesimal = s.Millesimal,
           um.Notes = s.Notes,
           um.LastUpdatedById = @CreatedById,
           um.LastUpdatedByFullName = @CreatedByFullName,
           um.LastUpdateDate = SYSDATETIME(),
           um.IsDeleted = 0
    FROM dbo.UnitMillesimal um
    INNER JOIN UnitMillesimalSource s
        ON s.MillesimalTableId = um.MillesimalTableId
       AND s.UnitId = um.UnitId;

    ;WITH UnitMillesimalSource AS (
        SELECT
            mu.TenantId,
            mt.Id AS MillesimalTableId,
            mu.DomuWaveId AS UnitId,
            CAST(ISNULL(su.MillesimalGeneral, 0) AS DECIMAL(18,4)) AS Millesimal,
            N'Import VZ - millesimi generali' AS Notes
        FROM dbo.Map_VZ_Unit mu
        INNER JOIN stg.VZ_Unit su ON su.VZId = mu.VZId
        INNER JOIN stg.VZ_Building sb ON sb.VZId = su.VZBuildingId
        INNER JOIN dbo.Map_VZ_Condominium mc
            ON mc.VZId = sb.VZCondominiumId
           AND mc.TenantId = mu.TenantId
        INNER JOIN dbo.MillesimalTable mt
            ON mt.CondominiumId = mc.DomuWaveId
           AND mt.TenantId = mc.TenantId
           AND mt.Code = N'DEFAULT'
        WHERE ISNULL(su.MillesimalGeneral, 0) > 0

        UNION ALL

        SELECT
            mu.TenantId,
            mt.Id AS MillesimalTableId,
            mu.DomuWaveId AS UnitId,
            CAST(ISNULL(su.MillesimalLift, 0) AS DECIMAL(18,4)) AS Millesimal,
            N'Import VZ - millesimi ascensore' AS Notes
        FROM dbo.Map_VZ_Unit mu
        INNER JOIN stg.VZ_Unit su ON su.VZId = mu.VZId
        INNER JOIN stg.VZ_Building sb ON sb.VZId = su.VZBuildingId
        INNER JOIN dbo.Map_VZ_Condominium mc
            ON mc.VZId = sb.VZCondominiumId
           AND mc.TenantId = mu.TenantId
        INNER JOIN dbo.MillesimalTable mt
            ON mt.CondominiumId = mc.DomuWaveId
           AND mt.TenantId = mc.TenantId
           AND mt.Code = N'ASCENSORE'
        WHERE ISNULL(su.MillesimalLift, 0) > 0

        UNION ALL

        SELECT
            mu.TenantId,
            mt.Id AS MillesimalTableId,
            mu.DomuWaveId AS UnitId,
            CAST(ISNULL(su.MillesimalStaircase, 0) AS DECIMAL(18,4)) AS Millesimal,
            N'Import VZ - millesimi scala' AS Notes
        FROM dbo.Map_VZ_Unit mu
        INNER JOIN stg.VZ_Unit su ON su.VZId = mu.VZId
        INNER JOIN stg.VZ_Building sb ON sb.VZId = su.VZBuildingId
        INNER JOIN dbo.Map_VZ_Condominium mc
            ON mc.VZId = sb.VZCondominiumId
           AND mc.TenantId = mu.TenantId
        INNER JOIN dbo.MillesimalTable mt
            ON mt.CondominiumId = mc.DomuWaveId
           AND mt.TenantId = mc.TenantId
           AND mt.Code = N'MILLESIMISCALE'
        WHERE ISNULL(su.MillesimalStaircase, 0) > 0
    )
    INSERT INTO dbo.UnitMillesimal (
        Id, TenantId, MillesimalTableId, UnitId, Millesimal, Notes,
        CreatedById, CreatedByFullName, CreationDate, IsDeleted
    )
    SELECT
        NEXT_ID.NewId,
        s.TenantId,
        s.MillesimalTableId,
        s.UnitId,
        s.Millesimal,
        s.Notes,
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME(),
        0
    FROM UnitMillesimalSource s
    INNER JOIN (
        SELECT
            s2.MillesimalTableId,
            s2.UnitId,
            (SELECT ISNULL(MAX(Id), 0) FROM dbo.UnitMillesimal) + ROW_NUMBER() OVER (ORDER BY s2.MillesimalTableId, s2.UnitId) AS NewId
        FROM UnitMillesimalSource s2
        LEFT JOIN dbo.UnitMillesimal um2
            ON um2.MillesimalTableId = s2.MillesimalTableId
           AND um2.UnitId = s2.UnitId
        WHERE um2.Id IS NULL
    ) NEXT_ID
        ON NEXT_ID.MillesimalTableId = s.MillesimalTableId
       AND NEXT_ID.UnitId = s.UnitId;

    -- ------------------------------------------------------------
    -- F) Supplier
    -- ------------------------------------------------------------
    -- Stage 1 supporta mapping fornitore su tenant unico.
    IF (SELECT COUNT(DISTINCT TenantId) FROM dbo.MigrationTenantMap_VZ WHERE IsActive = 1) > 1
    BEGIN
        RAISERROR('Stage 1 Supplier richiede tenant unico. Estendere PK Map_VZ_Supplier (VZId,TenantId) per multi-tenant.', 16, 1);
    END

    DECLARE @SingleTenantId UNIQUEIDENTIFIER;
    SELECT TOP 1 @SingleTenantId = TenantId
    FROM dbo.MigrationTenantMap_VZ
    WHERE IsActive = 1;

    ;WITH SupplierSource AS (
        SELECT
            s.VZId,
            @SingleTenantId AS TenantId,
            LEFT(
                COALESCE(
                    NULLIF(LTRIM(RTRIM(s.CompanyName)), ''),
                    NULLIF(LTRIM(RTRIM(CONCAT(s.FirstName, ' ', s.LastName))), ''),
                    CONCAT('Fornitore VZ ', s.VZId)
                ),
                200
            ) AS CompanyName,
            LEFT(NULLIF(LTRIM(RTRIM(s.VatNumber)), ''), 50) AS VatNumber,
            LEFT(NULLIF(LTRIM(RTRIM(s.TaxCode)), ''), 50) AS TaxCode,
            LEFT(NULLIF(LTRIM(RTRIM(s.Address)), ''), 250) AS Address,
            LEFT(NULLIF(LTRIM(RTRIM(s.City)), ''), 100) AS City,
            LEFT(NULLIF(LTRIM(RTRIM(s.Province)), ''), 10) AS Province,
            LEFT(NULLIF(LTRIM(RTRIM(s.PostalCode)), ''), 10) AS PostalCode,
            LEFT(NULLIF(LTRIM(RTRIM(s.Email)), ''), 200) AS Email,
            LEFT(COALESCE(NULLIF(LTRIM(RTRIM(s.Phone)), ''), NULLIF(LTRIM(RTRIM(s.Mobile)), '')), 50) AS Phone,
            LEFT(NULLIF(LTRIM(RTRIM(s.Pec)), ''), 200) AS Pec,
            LEFT(NULLIF(LTRIM(RTRIM(CONCAT(s.FirstName, ' ', s.LastName))), ''), 200) AS ContactPerson,
            LEFT(s.Notes, 1000) AS Notes
        FROM stg.VZ_Supplier s
    )
    INSERT INTO dbo.Supplier (
        Id, TenantId, CompanyName, VatNumber, TaxCode, Address, City, Province, PostalCode,
        Email, Phone, Pec, ContactPerson, Notes,
        IsActive, CreatedById, CreatedByFullName, CreationDate, IsDeleted
    )
    SELECT
        NEXT_ID.NewId,
        s.TenantId,
        s.CompanyName,
        s.VatNumber,
        s.TaxCode,
        s.Address,
        s.City,
        s.Province,
        s.PostalCode,
        s.Email,
        s.Phone,
        s.Pec,
        s.ContactPerson,
        CASE
            WHEN s.Notes IS NULL OR s.Notes = '' THEN CONCAT('[VZ ID=', s.VZId, ']')
            ELSE CONCAT(LEFT(s.Notes, 900), ' [VZ ID=', s.VZId, ']')
        END,
        1,
        @CreatedById,
        @CreatedByFullName,
        SYSDATETIME(),
        0
    FROM SupplierSource s
    INNER JOIN (
        SELECT
            s2.VZId,
            (SELECT ISNULL(MAX(Id), 0) FROM dbo.Supplier) + ROW_NUMBER() OVER (ORDER BY s2.VZId) AS NewId
        FROM SupplierSource s2
        LEFT JOIN dbo.Map_VZ_Supplier m2 ON m2.VZId = s2.VZId
        WHERE m2.VZId IS NULL
    ) NEXT_ID ON NEXT_ID.VZId = s.VZId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Map_VZ_Supplier m WHERE m.VZId = s.VZId);

    -- Mapping Supplier legacy -> target
    INSERT INTO dbo.Map_VZ_Supplier (VZId, TenantId, DomuWaveId, RunCode)
    SELECT
        s.VZId,
        s.TenantId,
        d.Id,
        @RunCode
    FROM (
        SELECT
            src.VZId,
            src.TenantId,
            src.CompanyName
        FROM (
            SELECT
                s2.VZId,
                @SingleTenantId AS TenantId,
                LEFT(
                    COALESCE(
                        NULLIF(LTRIM(RTRIM(s2.CompanyName)), ''),
                        NULLIF(LTRIM(RTRIM(CONCAT(s2.FirstName, ' ', s2.LastName))), ''),
                        CONCAT('Fornitore VZ ', s2.VZId)
                    ),
                    200
                ) AS CompanyName
            FROM stg.VZ_Supplier s2
        ) src
    ) s
    INNER JOIN dbo.Supplier d
        ON d.TenantId = s.TenantId
       AND d.CompanyName = s.CompanyName
       AND d.IsDeleted = 0
    LEFT JOIN dbo.Map_VZ_Supplier m
        ON m.VZId = s.VZId
    WHERE m.VZId IS NULL;

    -- ------------------------------------------------------------
    -- G) Finalize run
    -- ------------------------------------------------------------
    UPDATE dbo.MigrationRun_VZ
       SET EndedAt = SYSDATETIME(),
           Status = N'Completed'
     WHERE RunCode = @RunCode;

    COMMIT TRANSACTION;

    -- ------------------------------------------------------------
    -- H) Esito sintetico
    -- ------------------------------------------------------------
    SELECT 'Map_VZ_Condominium' AS MapName, COUNT(*) AS MappedRows FROM dbo.Map_VZ_Condominium
    UNION ALL
    SELECT 'Map_VZ_Unit', COUNT(*) FROM dbo.Map_VZ_Unit
    UNION ALL
    SELECT 'Map_VZ_Supplier', COUNT(*) FROM dbo.Map_VZ_Supplier
    UNION ALL
    SELECT 'MillesimalTable', COUNT(*) FROM dbo.MillesimalTable WHERE IsDeleted = 0
    UNION ALL
    SELECT 'UnitMillesimal', COUNT(*) FROM dbo.UnitMillesimal WHERE IsDeleted = 0;
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

    RAISERROR('Stage 1 failed at line %d: %s', 16, 1, @ErrLine, @ErrMsg);
END CATCH;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_migration_stage2_financials
    @RunCode NVARCHAR(100) = NULL,
    @CreatedById INT = 1,
    @CreatedByFullName NVARCHAR(200) = N'Migrazione VZ'
AS
BEGIN
    IF @RunCode IS NULL
        SET @RunCode = CONCAT('VZ-STAGE2-', FORMAT(SYSDATETIME(), 'yyyyMMdd-HHmmss'));

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
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_migration_run_steps
    @RunStage0Foundation BIT = 0,
    @RunStage0Load BIT = 1,
    @RunStage1 BIT = 1,
    @RunStage2 BIT = 1,
    @CreatedById INT = 1,
    @CreatedByFullName NVARCHAR(200) = N'Migrazione VZ'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Stage1RunCode NVARCHAR(100) = CONCAT('VZ-STAGE1-', FORMAT(SYSDATETIME(), 'yyyyMMdd-HHmmss'));
    DECLARE @Stage2RunCode NVARCHAR(100) = CONCAT('VZ-STAGE2-', FORMAT(SYSDATETIME(), 'yyyyMMdd-HHmmss'));

    IF @RunStage0Foundation = 1
    BEGIN
        IF OBJECT_ID('dbo.sp_migration_stage0_foundation', 'P') IS NULL
            RAISERROR('Stored procedure dbo.sp_migration_stage0_foundation non trovata.', 16, 1);

        EXEC dbo.sp_migration_stage0_foundation;
    END

    IF @RunStage0Load = 1
    BEGIN
        IF OBJECT_ID('dbo.sp_migration_stage0_stagedata', 'P') IS NULL
            RAISERROR('Stored procedure dbo.sp_migration_stage0_stagedata non trovata.', 16, 1);

        EXEC dbo.sp_migration_stage0_stagedata;
    END

    IF @RunStage1 = 1
        EXEC dbo.sp_migration_stage1_masterdata
            @RunCode = @Stage1RunCode,
            @CreatedById = @CreatedById,
            @CreatedByFullName = @CreatedByFullName;

    IF @RunStage2 = 1
        EXEC dbo.sp_migration_stage2_financials
            @RunCode = @Stage2RunCode,
            @CreatedById = @CreatedById,
            @CreatedByFullName = @CreatedByFullName;
END
GO
