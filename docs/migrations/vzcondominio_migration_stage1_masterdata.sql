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
-- - Non aggiorna record già migrati; inserisce solo i mancanti.
-- - Il mapping fornitori Stage 1 assume tenant unico nel db sorgente.
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @RunCode NVARCHAR(100) = CONCAT('VZ-STAGE1-', FORMAT(SYSDATETIME(), 'yyyyMMdd-HHmmss'));
DECLARE @CreatedById INT = 1;
DECLARE @CreatedByFullName NVARCHAR(200) = N'Migrazione VZ';

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
            N'Scale' AS Name,
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
           mt.Code = s.Code,
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
            N'Scale' AS Name,
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
GO
