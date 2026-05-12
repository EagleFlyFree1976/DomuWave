-- ============================================================================
-- Migrazione puntuale VZCondominio -> Condominium
-- Input obbligatorio:
--   1) @SourceVZCondominioId   = Id sorgente in [Domus].[VZCondominio]
--   2) @TargetCondominiumId     = Id destinazione in [dbo].[Condominium]
--
-- Campi migrati:
--   [Domus].[VZCondominio].[nome]          -> [dbo].[Condominium].[Name]
--   [Domus].[VZCondominio].[note]          -> [dbo].[Condominium].[Notes]
--   [Domus].[VZCondominio].[iban]          -> [dbo].[Condominium].[Iban]
--   [Domus].[VZCondominio].[codiceFiscale] -> [dbo].[Condominium].[TaxCode]
--
-- Edifici copiati:
--   [Domus].[VZEdificio] (idCondominio = @SourceVZCondominioId)
--     -> UPSERT idempotente in [dbo].[Building] con CondominiumId = @TargetCondominiumId
--
-- Note:
-- - Script pensato per SQL Server (T-SQL)
-- - Esegue UPDATE del record destinazione (non crea nuovo Condominium)
-- ============================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @SourceVZCondominioId INT = 4;   -- TODO: valorizzare
DECLARE @TargetCondominiumId INT = 1276;    -- TODO: valorizzare


-- Opzionale ma consigliato per audit
DECLARE @UpdatedById INT = NULL;            -- es: 1

-- â”€â”€ Variabili scalari di sessione â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

DECLARE @EffectiveUserId       INT;
DECLARE @EffectiveUserFullName NVARCHAR(200);
DECLARE @TargetTenantId        UNIQUEIDENTIFIER;

-- HiLo: Building
DECLARE @BuildingMaxId          INT;
DECLARE @BuildingMaxLo          INT    = 10;
DECLARE @BuildingBlock          BIGINT = 11; -- max_lo + 1
DECLARE @BuildingComputedNextHi INT;
DECLARE @CurrentNextHi          INT;

-- HiLo: Staircase
DECLARE @StaircaseMaxId          INT;
DECLARE @StaircaseMaxLo          INT    = 10;
DECLARE @StaircaseBlock          BIGINT = 11; -- max_lo + 1
DECLARE @StaircaseComputedNextHi INT;
DECLARE @CurrentStaircaseNextHi  INT;

-- HiLo: RealEstateUnit
DECLARE @RealEstateUnitMaxId          INT;
DECLARE @RealEstateUnitMaxLo          INT    = 10;
DECLARE @RealEstateUnitBlock          BIGINT = 11; -- max_lo + 1
DECLARE @RealEstateUnitComputedNextHi INT;
DECLARE @CurrentRealEstateUnitNextHi  INT;

-- HiLo: UnitOwner
DECLARE @UnitOwnerMaxId          INT;
DECLARE @UnitOwnerMaxLo          INT    = 10;
DECLARE @UnitOwnerBlock          BIGINT = 11; -- max_lo + 1
DECLARE @UnitOwnerComputedNextHi INT;
DECLARE @CurrentUnitOwnerNextHi  INT;

-- HiLo: UserTenant
DECLARE @UserTenantMaxId          INT;
DECLARE @UserTenantMaxLo          INT    = 10;
DECLARE @UserTenantBlock          BIGINT = 11; -- max_lo + 1
DECLARE @UserTenantComputedNextHi INT;
DECLARE @CurrentUserTenantNextHi  INT;
DECLARE @Now                   DATETIME2 = SYSDATETIME();
DECLARE @UpdatedByFullName NVARCHAR(200) = N'Migrazione VZCondominio';
-- â”€â”€ Tabelle di audit / risultati â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
DECLARE @Changes TABLE
(
    CondominiumId INT NOT NULL,
    OldName NVARCHAR(200) NULL,
    NewName NVARCHAR(200) NULL,
    OldTaxCode NVARCHAR(16) NULL,
    NewTaxCode NVARCHAR(16) NULL,
    OldIban NVARCHAR(34) NULL,
    NewIban NVARCHAR(34) NULL,
    OldNotes NVARCHAR(2000) NULL,
    NewNotes NVARCHAR(2000) NULL,
    ChangeDate DATETIME2 NOT NULL
);

DECLARE @BuildingChanges TABLE
(
    SourceVZBuildingId INT NOT NULL,
    BuildingId INT NOT NULL,
    [Action] NVARCHAR(10) NOT NULL,
    BuildingName NVARCHAR(200) NOT NULL,
    BuildingAddress NVARCHAR(500) NULL,
    ChangeDate DATETIME2 NOT NULL
);

DECLARE @StaircaseChanges TABLE
(
    SourceVZStaircaseId INT NOT NULL,
    StaircaseId INT NOT NULL,
    BuildingId INT NULL,
    [Action] NVARCHAR(10) NOT NULL,
    StaircaseName NVARCHAR(50) NOT NULL,
    ChangeDate DATETIME2 NOT NULL
);

DECLARE @UnitChanges TABLE
(
    SourceVZUnitId INT NOT NULL,
    RealEstateUnitId INT NOT NULL,
    BuildingId INT NULL,
    StaircaseId INT NULL,
    [Action] NVARCHAR(10) NOT NULL,
    InternalNumber NVARCHAR(40) NULL,
    Floor INT NOT NULL,
    ChangeDate DATETIME2 NOT NULL
);

DECLARE @PersonChanges TABLE
(
    SourceVZPersonId INT NOT NULL,
    UserId INT NOT NULL,
    [Action] NVARCHAR(10) NOT NULL,
    Login NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(255) NOT NULL,
    LastName NVARCHAR(255) NOT NULL,
    ChangeDate DATETIME2 NOT NULL
);

DECLARE @UnitOwnerChanges TABLE
(
    SourceVZContractId INT NOT NULL,
    UnitOwnerId INT NOT NULL,
    UnitId INT NOT NULL,
    UserId BIGINT NOT NULL,
    [Action] NVARCHAR(10) NOT NULL,
    OwnershipQuota DECIMAL(18,4) NOT NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NULL,
    ChangeDate DATETIME2 NOT NULL
);

DECLARE @UserTenantChanges TABLE
(
    SourceVZPersonId INT NOT NULL,
    UserTenantId INT NOT NULL,
    UserId INT NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    [Action] NVARCHAR(10) NOT NULL,
    ChangeDate DATETIME2 NOT NULL
);

-- â”€â”€ Tabelle di staging â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
DECLARE @SourceBuildings TABLE
(
    SourceVZBuildingId INT NOT NULL PRIMARY KEY,
    BuildingCode NVARCHAR(50) NOT NULL,
    BuildingName NVARCHAR(200) NOT NULL,
    BuildingDescription NVARCHAR(1000) NULL,
    BuildingAddress NVARCHAR(500) NULL,
    NumberOfFloors INT NULL
);

DECLARE @BuildingToInsert TABLE
(
    NewId INT NOT NULL,
    SourceVZBuildingId INT NOT NULL,
    BuildingCode NVARCHAR(50) NOT NULL,
    BuildingName NVARCHAR(200) NOT NULL,
    BuildingDescription NVARCHAR(1000) NULL,
    BuildingAddress NVARCHAR(500) NULL,
    NumberOfFloors INT NULL
);

DECLARE @SourceStaircases TABLE
(
    SourceVZStaircaseId INT NOT NULL PRIMARY KEY,
    TargetBuildingId INT NULL,
    StaircaseName NVARCHAR(50) NOT NULL
);

DECLARE @StaircaseToInsert TABLE
(
    NewId INT NOT NULL,
    SourceVZStaircaseId INT NOT NULL,
    TargetBuildingId INT NULL,
    StaircaseName NVARCHAR(50) NOT NULL
);

DECLARE @SourceUnits TABLE
(
    SourceVZUnitId INT NOT NULL PRIMARY KEY,
    TargetBuildingId INT NULL,
    TargetStaircaseId INT NULL,
    StaircaseName NVARCHAR(10) NOT NULL,
    Floor INT NOT NULL,
    InternalNumber NVARCHAR(40) NOT NULL,
    Subordinate NVARCHAR(10) NULL,
    CadastralIncome DECIMAL(18,4) NULL,
    Notes NVARCHAR(1000) NULL,
    UnitNote NVARCHAR(1000) NOT NULL,
    DisplayName NVARCHAR(200) NULL,
    Sheet NVARCHAR(20) NULL,
    Parcel NVARCHAR(20) NULL
);

DECLARE @UnitToInsert TABLE
(
    NewId INT NOT NULL,
    SourceVZUnitId INT NOT NULL,
    TargetBuildingId INT NULL,
    TargetStaircaseId INT NULL,
    StaircaseName NVARCHAR(10) NOT NULL,
    Floor INT NOT NULL,
    InternalNumber NVARCHAR(40) NOT NULL,
    Subordinate NVARCHAR(10) NULL,
    CadastralIncome DECIMAL(18,4) NULL,
    UnitNote NVARCHAR(1000) NOT NULL,
    DisplayName NVARCHAR(200) NULL,
    Sheet NVARCHAR(20) NULL,
    Parcel NVARCHAR(20) NULL
);

DECLARE @SourcePersons TABLE
(
    SourceVZPersonId INT NOT NULL PRIMARY KEY,
    UserCode NVARCHAR(50) NOT NULL,
    Login NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(255) NOT NULL,
    LastName NVARCHAR(255) NOT NULL,
    RawTaxCode NVARCHAR(50) NULL,
    RawVatNumber NVARCHAR(50) NULL
);

DECLARE @UsersToInsert TABLE
(
    NewUserId INT NOT NULL,
    SourceVZPersonId INT NOT NULL,
    UserCode NVARCHAR(50) NOT NULL,
    Login NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(255) NOT NULL,
    LastName NVARCHAR(255) NOT NULL
);

DECLARE @PersonMap TABLE
(
    SourceVZPersonId INT NOT NULL PRIMARY KEY,
    UserId BIGINT NOT NULL,
    FirstName NVARCHAR(255) NOT NULL,
    LastName NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL
);

DECLARE @UserTenantToInsert TABLE
(
    NewId INT NOT NULL,
    SourceVZPersonId INT NOT NULL,
    UserId INT NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    IsDefault BIT NOT NULL
);

DECLARE @SourceOwnerContracts TABLE
(
    SourceVZContractId INT NOT NULL PRIMARY KEY,
    SourceVZPersonId INT NOT NULL,
    SourceVZUnitId INT NOT NULL,
    TargetUnitId INT NULL,
    TargetUserId BIGINT NULL,
    OwnerType NVARCHAR(50) NOT NULL,
    OwnershipQuota DECIMAL(18,4) NOT NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NULL,
    IsResident BIT NOT NULL,
    Notes NVARCHAR(500) NULL,
    FirstName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NULL,
    Email NVARCHAR(200) NULL
);

DECLARE @UnitOwnerToInsert TABLE
(
    NewId INT NOT NULL,
    SourceVZContractId INT NOT NULL,
    TargetUnitId INT NOT NULL,
    TargetUserId BIGINT NOT NULL,
    OwnerType NVARCHAR(50) NOT NULL,
    OwnershipQuota DECIMAL(18,4) NOT NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NULL,
    IsResident BIT NOT NULL,
    Notes NVARCHAR(500) NULL,
    FirstName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NULL,
    Email NVARCHAR(200) NULL
);
IF @SourceVZCondominioId IS NULL OR @TargetCondominiumId IS NULL
BEGIN
    THROW 51000, 'Valorizzare @SourceVZCondominioId e @TargetCondominiumId prima di eseguire lo script.', 1;
END;

IF NOT EXISTS (
    SELECT 1
    FROM [Domus].[VZCondominio] vz
    WHERE vz.id = @SourceVZCondominioId
)
BEGIN
    THROW 51001, 'Record sorgente non trovato in [Domus].[VZCondominio].', 1;
END;

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[Condominium] c
    WHERE c.Id = @TargetCondominiumId
)
BEGIN
    THROW 51002, 'Record destinazione non trovato in [dbo].[Condominium].', 1;
END;

IF EXISTS (
    SELECT 1
    FROM [dbo].[Condominium] c
    WHERE c.Id = @TargetCondominiumId
      AND c.IsDeleted = 1
)
BEGIN
    THROW 51003, 'Il record destinazione e'' marcato come IsDeleted=1. Operazione annullata.', 1;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    SELECT
        @TargetTenantId = c.TenantId,
        @EffectiveUserId = COALESCE(@UpdatedById, c.LastUpdatedById, c.CreatedById, 0),
        @EffectiveUserFullName = COALESCE(@UpdatedByFullName, c.LastUpdatedByFullName, c.CreatedByFullName, N'Migrazione VZCondominio')
    FROM [dbo].[Condominium] c
    WHERE c.Id = @TargetCondominiumId;

    UPDATE c
       SET c.Name = LEFT(LTRIM(RTRIM(vz.nome)), 200),
           c.Notes = CASE
                        WHEN vz.note IS NULL OR LTRIM(RTRIM(vz.note)) = '' THEN NULL
                        ELSE LEFT(vz.note, 2000)
                     END,
           c.Iban = CASE
                        WHEN vz.iban IS NULL OR LTRIM(RTRIM(vz.iban)) = '' THEN NULL
                        ELSE LEFT(CONVERT(NVARCHAR(150), LTRIM(RTRIM(vz.iban))), 34)
                    END,
           c.TaxCode = CASE
                           WHEN vz.codiceFiscale IS NULL OR LTRIM(RTRIM(vz.codiceFiscale)) = '' THEN NULL
                           ELSE LEFT(CONVERT(NVARCHAR(30), LTRIM(RTRIM(vz.codiceFiscale))), 16)
                       END,
           c.LastUpdatedById = COALESCE(@UpdatedById, c.LastUpdatedById, c.CreatedById),
           c.LastUpdatedByFullName = COALESCE(@UpdatedByFullName, c.LastUpdatedByFullName, c.CreatedByFullName),
           c.LastUpdateDate = SYSDATETIME()
    OUTPUT
        inserted.Id,
        deleted.Name,
        inserted.Name,
        deleted.TaxCode,
        inserted.TaxCode,
        deleted.Iban,
        inserted.Iban,
        deleted.Notes,
        inserted.Notes,
        inserted.LastUpdateDate
    INTO @Changes
    (
        CondominiumId,
        OldName,
        NewName,
        OldTaxCode,
        NewTaxCode,
        OldIban,
        NewIban,
        OldNotes,
        NewNotes,
        ChangeDate
    )
    FROM [dbo].[Condominium] c
    INNER JOIN [Domus].[VZCondominio] vz ON vz.id = @SourceVZCondominioId
    WHERE c.Id = @TargetCondominiumId;

    IF @@ROWCOUNT <> 1
    BEGIN
        THROW 51004, 'UPDATE non eseguito: controllare gli ID forniti.', 1;
    END;

    INSERT INTO @SourceBuildings
    (
        SourceVZBuildingId,
        BuildingCode,
        BuildingName,
        BuildingDescription,
        BuildingAddress,
        NumberOfFloors
    )
    SELECT
        vzb.id AS SourceVZBuildingId,
        LEFT(CONCAT(N'VZ-', CONVERT(NVARCHAR(20), vzb.id)), 50) AS BuildingCode,
        LEFT(CONVERT(NVARCHAR(255), LTRIM(RTRIM(vzb.nome))), 200) AS BuildingName,
        CASE
            WHEN NULLIF(LTRIM(RTRIM(vzb.note)), '') IS NULL THEN NULL
            ELSE LEFT(CONVERT(NVARCHAR(2000), vzb.note), 1000)
        END AS BuildingDescription,
        NULLIF(
            LEFT(
                CONCAT_WS(
                    N', ',
                    NULLIF(CONVERT(NVARCHAR(255), LTRIM(RTRIM(vzb.indirizzo))), N''),
                    NULLIF(CONVERT(NVARCHAR(50), LTRIM(RTRIM(vzb.codicePostale))), N''),
                    NULLIF(CONVERT(NVARCHAR(255), LTRIM(RTRIM(vzb.citta))), N''),
                    NULLIF(CONVERT(NVARCHAR(255), LTRIM(RTRIM(vzb.provincia))), N''),
                    NULLIF(CONVERT(NVARCHAR(50), LTRIM(RTRIM(vzb.nazione))), N'')
                ),
                500
            ),
            N''
        ) AS BuildingAddress,
        vzb.piani AS NumberOfFloors
    FROM [Domus].[VZEdificio] vzb
    WHERE vzb.idCondominio = @SourceVZCondominioId
      AND NULLIF(LTRIM(RTRIM(vzb.nome)), '') IS NOT NULL;

    UPDATE b
       SET b.TenantId = @TargetTenantId,
           b.Name = sb.BuildingName,
           b.Description = sb.BuildingDescription,
           b.Address = sb.BuildingAddress,
           b.NumberOfFloors = sb.NumberOfFloors,
           b.IsActive = 1,
           b.IsDeleted = 0,
           b.LastUpdatedById = @EffectiveUserId,
           b.LastUpdatedByFullName = @EffectiveUserFullName,
           b.LastUpdateDate = @Now
    OUTPUT
        sb.SourceVZBuildingId,
        inserted.Id,
        N'UPDATED',
        inserted.Name,
        inserted.Address,
        inserted.LastUpdateDate
    INTO @BuildingChanges
    (
        SourceVZBuildingId,
        BuildingId,
        [Action],
        BuildingName,
        BuildingAddress,
        ChangeDate
    )
    FROM [dbo].[Building] b
    INNER JOIN @SourceBuildings sb ON sb.BuildingCode = b.Code
    WHERE b.CondominiumId = @TargetCondominiumId;

    ;WITH ToInsert AS (
        SELECT
            sb.SourceVZBuildingId,
            sb.BuildingCode,
            sb.BuildingName,
            sb.BuildingDescription,
            sb.BuildingAddress,
            sb.NumberOfFloors,
            ROW_NUMBER() OVER (ORDER BY sb.SourceVZBuildingId) AS rn
        FROM @SourceBuildings sb
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dbo].[Building] b
            WHERE b.CondominiumId = @TargetCondominiumId
              AND b.Code = sb.BuildingCode
        )
    ), BaseId AS (
        SELECT ISNULL(MAX(b.Id), 0) AS MaxId
        FROM [dbo].[Building] b
    )
    INSERT INTO @BuildingToInsert
    (
        NewId,
        SourceVZBuildingId,
        BuildingCode,
        BuildingName,
        BuildingDescription,
        BuildingAddress,
        NumberOfFloors
    )
    SELECT
        bid.MaxId + ti.rn AS NewId,
        ti.SourceVZBuildingId,
        ti.BuildingCode,
        ti.BuildingName,
        ti.BuildingDescription,
        ti.BuildingAddress,
        ti.NumberOfFloors
    FROM ToInsert ti
    CROSS JOIN BaseId bid;

    INSERT INTO [dbo].[Building]
    (
        Id,
        TenantId,
        CondominiumId,
        Name,
        Description,
        Code,
        Address,
        YearOfConstruction,
        NumberOfFloors,
        HasElevator,
        IsActive,
        CreatedById,
        CreatedByFullName,
        LastUpdatedById,
        LastUpdatedByFullName,
        IsDeleted,
        CreationDate,
        LastUpdateDate
    )
    SELECT
        ti.NewId AS Id,
        @TargetTenantId,
        @TargetCondominiumId,
        ti.BuildingName,
        ti.BuildingDescription,
        ti.BuildingCode AS Code,
        ti.BuildingAddress,
        NULL,
        ti.NumberOfFloors,
        0,
        1,
        @EffectiveUserId,
        @EffectiveUserFullName,
        NULL,
        NULL,
        0,
        @Now,
        NULL
    FROM @BuildingToInsert ti;

    INSERT INTO @BuildingChanges
    (
        SourceVZBuildingId,
        BuildingId,
        [Action],
        BuildingName,
        BuildingAddress,
        ChangeDate
    )
    SELECT
        ti.SourceVZBuildingId,
        ti.NewId,
        N'INSERTED',
        ti.BuildingName,
        ti.BuildingAddress,
        @Now
    FROM @BuildingToInsert ti;

    ;WITH StairRaw AS (
        SELECT
            vzs.id AS SourceVZStaircaseId,
            b.Id AS TargetBuildingId,
            LEFT(
                ISNULL(
                    NULLIF(
                        LTRIM(RTRIM(
                            CONCAT(
                                CONVERT(NVARCHAR(255), vzs.nome),
                                CASE
                                    WHEN NULLIF(LTRIM(RTRIM(vzs.nCivico)), '') IS NULL THEN N''
                                    ELSE CONCAT(N' ', CONVERT(NVARCHAR(50), LTRIM(RTRIM(vzs.nCivico))))
                                END
                            )
                        )),
                        N''
                    ),
                    CONCAT(N'SC-', CONVERT(NVARCHAR(20), vzs.id))
                ),
                50
            ) AS StaircaseBaseName
        FROM [Domus].[VZScala] vzs
        INNER JOIN [Domus].[VZEdificio] vzb ON vzb.id = vzs.idEdificio
        LEFT JOIN [dbo].[Building] b
               ON b.CondominiumId = @TargetCondominiumId
              AND b.Code = LEFT(CONCAT(N'VZ-', CONVERT(NVARCHAR(20), vzb.id)), 50)
        WHERE vzb.idCondominio = @SourceVZCondominioId
    ), StairNamed AS (
        SELECT
            sr.SourceVZStaircaseId,
            sr.TargetBuildingId,
            LEFT(
                CASE
                    WHEN ROW_NUMBER() OVER (PARTITION BY sr.StaircaseBaseName ORDER BY sr.SourceVZStaircaseId) > 1
                        THEN CONCAT(sr.StaircaseBaseName, N'-', CONVERT(NVARCHAR(10), ROW_NUMBER() OVER (PARTITION BY sr.StaircaseBaseName ORDER BY sr.SourceVZStaircaseId)))
                    ELSE sr.StaircaseBaseName
                END,
                50
            ) AS StaircaseName
        FROM StairRaw sr
    )
    INSERT INTO @SourceStaircases (SourceVZStaircaseId, TargetBuildingId, StaircaseName)
    SELECT
        sn.SourceVZStaircaseId,
        sn.TargetBuildingId,
        sn.StaircaseName
    FROM StairNamed sn;

    IF EXISTS (
        SELECT 1
        FROM @SourceStaircases ss
        WHERE ss.TargetBuildingId IS NULL
    )
    BEGIN
        THROW 51005, 'Impossibile associare una o piu'' scale a un Building target. Verificare la migrazione Building.', 1;
    END;

    UPDATE s
       SET s.TenantId = @TargetTenantId,
           s.BuildingId = ss.TargetBuildingId,
           s.IsActive = 1,
           s.IsDeleted = 0,
           s.LastUpdatedById = @EffectiveUserId,
           s.LastUpdatedByFullName = @EffectiveUserFullName,
           s.LastUpdateDate = @Now
    OUTPUT
        ss.SourceVZStaircaseId,
        inserted.Id,
        inserted.BuildingId,
        N'UPDATED',
        inserted.Name,
        inserted.LastUpdateDate
    INTO @StaircaseChanges
    (
        SourceVZStaircaseId,
        StaircaseId,
        BuildingId,
        [Action],
        StaircaseName,
        ChangeDate
    )
    FROM [dbo].[Staircase] s
    INNER JOIN @SourceStaircases ss
            ON ss.StaircaseName = s.Name
    WHERE s.CondominiumId = @TargetCondominiumId;

    ;WITH ToInsert AS (
        SELECT
            ss.SourceVZStaircaseId,
            ss.TargetBuildingId,
            ss.StaircaseName,
            ROW_NUMBER() OVER (ORDER BY ss.SourceVZStaircaseId) AS rn
        FROM @SourceStaircases ss
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dbo].[Staircase] s
            WHERE s.CondominiumId = @TargetCondominiumId
              AND s.Name = ss.StaircaseName
        )
    ), BaseId AS (
        SELECT ISNULL(MAX(s.Id), 0) AS MaxId
        FROM [dbo].[Staircase] s
    )
    INSERT INTO @StaircaseToInsert
    (
        NewId,
        SourceVZStaircaseId,
        TargetBuildingId,
        StaircaseName
    )
    SELECT
        bid.MaxId + ti.rn AS NewId,
        ti.SourceVZStaircaseId,
        ti.TargetBuildingId,
        ti.StaircaseName
    FROM ToInsert ti
    CROSS JOIN BaseId bid;

    INSERT INTO [dbo].[Staircase]
    (
        Id,
        TenantId,
        CondominiumId,
        Name,
        IsActive,
        CreatedById,
        CreatedByFullName,
        LastUpdatedById,
        LastUpdatedByFullName,
        IsDeleted,
        CreationDate,
        LastUpdateDate,
        BuildingId
    )
    SELECT
        ti.NewId AS Id,
        @TargetTenantId,
        @TargetCondominiumId,
        ti.StaircaseName,
        1,
        @EffectiveUserId,
        @EffectiveUserFullName,
        NULL,
        NULL,
        0,
        @Now,
        NULL,
        ti.TargetBuildingId
    FROM @StaircaseToInsert ti;

    INSERT INTO @StaircaseChanges
    (
        SourceVZStaircaseId,
        StaircaseId,
        BuildingId,
        [Action],
        StaircaseName,
        ChangeDate
    )
    SELECT
        ti.SourceVZStaircaseId,
        ti.NewId,
        ti.TargetBuildingId,
        N'INSERTED',
        ti.StaircaseName,
        @Now
    FROM @StaircaseToInsert ti;

    INSERT INTO @SourceUnits
    (
        SourceVZUnitId,
        TargetBuildingId,
        TargetStaircaseId,
        StaircaseName,
        Floor,
        InternalNumber,
        Subordinate,
        CadastralIncome,
        Notes,
        UnitNote,
        DisplayName,
        Sheet,
        Parcel
    )
    SELECT
        vzu.id AS SourceVZUnitId,
        b.Id AS TargetBuildingId,
        st.Id AS TargetStaircaseId,
        LEFT(ISNULL(NULLIF(ss.StaircaseName, N''), N'SD'), 10) AS StaircaseName,
        ISNULL(vzu.piano, 0) AS Floor,
        LEFT(ISNULL(NULLIF(LTRIM(RTRIM(CONVERT(NVARCHAR(255), vzu.descrizione))), N''), CONCAT(N'Unita-', CONVERT(NVARCHAR(20), vzu.id))), 40) AS InternalNumber,
        LEFT(CONVERT(NVARCHAR(20), vzu.subalterno), 10) AS Subordinate,
        TRY_CONVERT(DECIMAL(18,4), vzu.millesimi) AS CadastralIncome,
        NULLIF(LTRIM(RTRIM(CONVERT(NVARCHAR(1000), vzu.note))), N'') AS Notes,
        LEFT(
            CASE
                WHEN NULLIF(LTRIM(RTRIM(CONVERT(NVARCHAR(1000), vzu.note))), N'') IS NULL
                    THEN CONCAT(N'[VZ ID=', CONVERT(NVARCHAR(20), vzu.id), N']')
                ELSE CONCAT(
                    LEFT(CONVERT(NVARCHAR(1000), vzu.note), 960),
                    N' [VZ ID=',
                    CONVERT(NVARCHAR(20), vzu.id),
                    N']'
                )
            END,
            1000
        ) AS UnitNote,
        LEFT(
            CONCAT(
                LEFT(ISNULL(NULLIF(ss.StaircaseName, N''), N'SD'), 20),
                N'-',
                LEFT(ISNULL(NULLIF(LTRIM(RTRIM(CONVERT(NVARCHAR(255), vzu.descrizione))), N''), CONCAT(N'Unita-', CONVERT(NVARCHAR(20), vzu.id))), 50)
            ),
            200
        ) AS DisplayName,
        LEFT(CONVERT(NVARCHAR(20), vzu.foglio), 20) AS Sheet,
        LEFT(CONVERT(NVARCHAR(20), vzu.mappale), 20) AS Parcel
    FROM [Domus].[VZImmobile] vzu
    INNER JOIN [Domus].[VZEdificio] vzb ON vzb.id = vzu.idEdificio
    LEFT JOIN @SourceStaircases ss ON ss.SourceVZStaircaseId = vzu.idScala
    LEFT JOIN [dbo].[Building] b
           ON b.CondominiumId = @TargetCondominiumId
          AND b.Code = LEFT(CONCAT(N'VZ-', CONVERT(NVARCHAR(20), vzb.id)), 50)
    LEFT JOIN [dbo].[Staircase] st
           ON st.CondominiumId = @TargetCondominiumId
          AND st.Name = ss.StaircaseName
          AND st.IsDeleted = 0
    WHERE vzb.idCondominio = @SourceVZCondominioId;

    IF EXISTS (
        SELECT 1
        FROM @SourceUnits su
        WHERE su.TargetBuildingId IS NULL
    )
    BEGIN
        THROW 51006, 'Impossibile associare una o piu'' unita'' al Building target.', 1;
    END;

    UPDATE u
       SET u.TenantId = @TargetTenantId,
           u.BuildingId = su.TargetBuildingId,
           u.StaircaseId = su.TargetStaircaseId,
           u.Staircase = su.StaircaseName,
           u.Floor = su.Floor,
           u.InternalNumber = su.InternalNumber,
           u.Subordinate = su.Subordinate,
           u.CadastralIncome = su.CadastralIncome,
           u.Sheet = su.Sheet,
           u.Parcel = su.Parcel,
           u.Notes = su.UnitNote,
           u.DisplayName = su.DisplayName,
           u.IsActive = 1,
           u.IsDeleted = 0,
           u.LastUpdatedById = @EffectiveUserId,
           u.LastUpdatedByFullName = @EffectiveUserFullName,
           u.LastUpdateDate = @Now
    OUTPUT
        su.SourceVZUnitId,
        inserted.Id,
        inserted.BuildingId,
        inserted.StaircaseId,
        N'UPDATED',
        inserted.InternalNumber,
        inserted.Floor,
        inserted.LastUpdateDate
    INTO @UnitChanges
    (
        SourceVZUnitId,
        RealEstateUnitId,
        BuildingId,
        StaircaseId,
        [Action],
        InternalNumber,
        Floor,
        ChangeDate
    )
    FROM [dbo].[RealEstateUnit] u
    INNER JOIN @SourceUnits su
            ON u.CondominiumId = @TargetCondominiumId
           AND CHARINDEX(CONCAT(N'[VZ ID=', CONVERT(NVARCHAR(20), su.SourceVZUnitId), N']'), ISNULL(u.Notes, N'')) > 0;

    ;WITH ToInsert AS (
        SELECT
            su.SourceVZUnitId,
            su.TargetBuildingId,
            su.TargetStaircaseId,
            su.StaircaseName,
            su.Floor,
            su.InternalNumber,
            su.Subordinate,
            su.CadastralIncome,
            su.UnitNote,
            su.DisplayName,
            su.Sheet,
            su.Parcel,
            ROW_NUMBER() OVER (ORDER BY su.SourceVZUnitId) AS rn
        FROM @SourceUnits su
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dbo].[RealEstateUnit] u
            WHERE u.CondominiumId = @TargetCondominiumId
              AND CHARINDEX(CONCAT(N'[VZ ID=', CONVERT(NVARCHAR(20), su.SourceVZUnitId), N']'), ISNULL(u.Notes, N'')) > 0
        )
    ), BaseId AS (
        SELECT ISNULL(MAX(u.Id), 0) AS MaxId
        FROM [dbo].[RealEstateUnit] u
    )
    INSERT INTO @UnitToInsert
    (
        NewId,
        SourceVZUnitId,
        TargetBuildingId,
        TargetStaircaseId,
        StaircaseName,
        Floor,
        InternalNumber,
        Subordinate,
        CadastralIncome,
        UnitNote,
        DisplayName,
        Sheet,
        Parcel
    )
    SELECT
        bid.MaxId + ti.rn AS NewId,
        ti.SourceVZUnitId,
        ti.TargetBuildingId,
        ti.TargetStaircaseId,
        ti.StaircaseName,
        ti.Floor,
        ti.InternalNumber,
        ti.Subordinate,
        ti.CadastralIncome,
        ti.UnitNote,
        ti.DisplayName,
        ti.Sheet,
        ti.Parcel
    FROM ToInsert ti
    CROSS JOIN BaseId bid;

    INSERT INTO [dbo].[RealEstateUnit]
    (
        Id,
        TenantId,
        CondominiumId,
        Staircase,
        Floor,
        InternalNumber,
        Subordinate,
        Category,
        CadastralIncome,
        AreaSqm,
        Rooms,
        UnitType,
        OccupancyStatus,
        Notes,
        IsActive,
        CreatedById,
        CreatedByFullName,
        CreationDate,
        LastUpdatedById,
        LastUpdatedByFullName,
        LastUpdateDate,
        IsDeleted,
        NumeroAbitanti,
        DisplayName,
        BillingGroupId,
        BuildingId,
        Sheet,
        Parcel,
        StaircaseId
    )
    SELECT
        ti.NewId AS Id,
        @TargetTenantId,
        @TargetCondominiumId,
        ti.StaircaseName,
        ti.Floor,
        ti.InternalNumber,
        ti.Subordinate,
        NULL,
        ti.CadastralIncome,
        NULL,
        NULL,
        N'Residential',
        N'Occupied',
        ti.UnitNote,
        1,
        @EffectiveUserId,
        @EffectiveUserFullName,
        @Now,
        NULL,
        NULL,
        NULL,
        0,
        1,
        ti.DisplayName,
        NULL,
        ti.TargetBuildingId,
        ti.Sheet,
        ti.Parcel,
        ti.TargetStaircaseId
    FROM @UnitToInsert ti;

    INSERT INTO @UnitChanges
    (
        SourceVZUnitId,
        RealEstateUnitId,
        BuildingId,
        StaircaseId,
        [Action],
        InternalNumber,
        Floor,
        ChangeDate
    )
    SELECT
        ti.SourceVZUnitId,
        ti.NewId,
        ti.TargetBuildingId,
        ti.TargetStaircaseId,
        N'INSERTED',
        ti.InternalNumber,
        ti.Floor,
        @Now
    FROM @UnitToInsert ti;

    ;WITH ContractPersons AS (
        SELECT DISTINCT
            c.idPersona AS SourceVZPersonId
        FROM [Gestione].[VZContratto] c
        INNER JOIN [Domus].[VZImmobile] i ON i.id = c.idImmobile
        INNER JOIN [Domus].[VZEdificio] b ON b.id = i.idEdificio
        WHERE b.idCondominio = @SourceVZCondominioId
    ), PersonBase AS (
        SELECT
            p.id AS SourceVZPersonId,
            LEFT(CONCAT(N'VZP-', CONVERT(NVARCHAR(20), p.id)), 50) AS UserCode,
            NULLIF(LTRIM(RTRIM(p.email)), N'') AS RawEmail,
            NULLIF(LTRIM(RTRIM(p.codiceFiscale)), N'') AS RawTaxCode,
            NULLIF(LTRIM(RTRIM(p.partitaIva)), N'') AS RawVatNumber,
            LEFT(ISNULL(NULLIF(LTRIM(RTRIM(p.nome)), N''), CONCAT(N'Persona-', CONVERT(NVARCHAR(20), p.id))), 255) AS FirstName,
            LEFT(ISNULL(NULLIF(LTRIM(RTRIM(p.cognome)), N''), N'Sconosciuto'), 255) AS LastName
        FROM [Anagrafiche].[VZPersona] p
        INNER JOIN ContractPersons cp ON cp.SourceVZPersonId = p.id
    ), PersonWithLogin AS (
        SELECT
            pb.SourceVZPersonId,
            pb.UserCode,
            pb.RawTaxCode,
            pb.RawVatNumber,
            pb.FirstName,
            pb.LastName,
            LOWER(
                ISNULL(
                    CASE WHEN pb.RawEmail LIKE N'%@%' THEN pb.RawEmail END,
                    CONCAT(N'vzpersona', CONVERT(NVARCHAR(20), pb.SourceVZPersonId), N'@domuwave.local')
                )
            ) AS PreferredLogin,
            ROW_NUMBER() OVER (
                PARTITION BY LOWER(
                    ISNULL(
                        CASE WHEN pb.RawEmail LIKE N'%@%' THEN pb.RawEmail END,
                        CONCAT(N'vzpersona', CONVERT(NVARCHAR(20), pb.SourceVZPersonId), N'@domuwave.local')
                    )
                )
                ORDER BY pb.SourceVZPersonId
            ) AS LoginRank
        FROM PersonBase pb
    )
    INSERT INTO @SourcePersons
    (
        SourceVZPersonId,
        UserCode,
        Login,
        Email,
        FirstName,
        LastName,
        RawTaxCode,
        RawVatNumber
    )
    SELECT
        pwl.SourceVZPersonId,
        pwl.UserCode,
        CASE
            WHEN pwl.LoginRank = 1 THEN LEFT(pwl.PreferredLogin, 255)
            ELSE LEFT(CONCAT(N'vzpersona', CONVERT(NVARCHAR(20), pwl.SourceVZPersonId), N'@domuwave.local'), 255)
        END AS Login,
        CASE
            WHEN pwl.LoginRank = 1 THEN LEFT(pwl.PreferredLogin, 255)
            ELSE LEFT(CONCAT(N'vzpersona', CONVERT(NVARCHAR(20), pwl.SourceVZPersonId), N'@domuwave.local'), 255)
        END AS Email,
        pwl.FirstName,
        pwl.LastName,
        pwl.RawTaxCode,
        pwl.RawVatNumber
    FROM PersonWithLogin pwl;

    UPDATE sp
       SET sp.Login = LEFT(CONCAT(N'vzpersona', CONVERT(NVARCHAR(20), sp.SourceVZPersonId), N'@domuwave.local'), 255),
           sp.Email = LEFT(CONCAT(N'vzpersona', CONVERT(NVARCHAR(20), sp.SourceVZPersonId), N'@domuwave.local'), 255)
    FROM @SourcePersons sp
    WHERE EXISTS (
        SELECT 1
        FROM dbo.base_Users bu
        WHERE bu.Login = sp.Login COLLATE DATABASE_DEFAULT
          AND ISNULL(bu.Code, N'') <> sp.UserCode COLLATE DATABASE_DEFAULT
    );

    UPDATE bu
       SET bu.Login = sp.Login,
           bu.Email = sp.Email,
           bu.FirstName = sp.FirstName,
           bu.LastName = sp.LastName,
           bu.RoleId = 6,
           bu.Culture = N'it-IT',
           bu.IsActive = 1,
           bu.IsSystemUser = 0,
           bu.PasswordExpired = 0,
           bu._FullName = LEFT(CONCAT(sp.FirstName, N' ', sp.LastName), 511),
           bu.LastUpdatedById = @EffectiveUserId,
           bu.LastUpdateDate = GETDATE(),
           bu.IsDeleted = 0
    OUTPUT
        sp.SourceVZPersonId,
        inserted.UserId,
        N'UPDATED',
        inserted.Login,
        inserted.Email,
        inserted.FirstName,
        inserted.LastName,
        @Now
    INTO @PersonChanges
    (
        SourceVZPersonId,
        UserId,
        [Action],
        Login,
        Email,
        FirstName,
        LastName,
        ChangeDate
    )
    FROM dbo.base_Users bu
    INNER JOIN @SourcePersons sp ON sp.UserCode = bu.Code COLLATE DATABASE_DEFAULT;

    ;WITH MissingUsers AS (
        SELECT
            sp.SourceVZPersonId,
            sp.UserCode,
            sp.Login,
            sp.Email,
            sp.FirstName,
            sp.LastName,
            ROW_NUMBER() OVER (ORDER BY sp.SourceVZPersonId) AS rn
        FROM @SourcePersons sp
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.base_Users bu
            WHERE bu.Code = sp.UserCode COLLATE DATABASE_DEFAULT
        )
    ), BaseUserId AS (
        SELECT ISNULL(MAX(bu.UserId), 0) AS MaxUserId
        FROM dbo.base_Users bu
    )
    INSERT INTO @UsersToInsert
    (
        NewUserId,
        SourceVZPersonId,
        UserCode,
        Login,
        Email,
        FirstName,
        LastName
    )
    SELECT
        bu.MaxUserId + mu.rn,
        mu.SourceVZPersonId,
        mu.UserCode,
        mu.Login,
        mu.Email,
        mu.FirstName,
        mu.LastName
    FROM MissingUsers mu
    CROSS JOIN BaseUserId bu;

    INSERT INTO dbo.base_Users
    (
        UserId,
        RoleId,
        SupervisorUserId,
        Code,
        Login,
        Email,
        FirstName,
        LastName,
        Culture,
        IsActive,
        IsSystemUser,
        Password,
        HierarchicalLevel,
        CreatedById,
        CreationDate,
        LastUpdatedById,
        LastUpdateDate,
        PasswordExpired,
        _FullName,
        IsDeleted,
        Discriminator,
        FailedLoginAttempts
    )
    SELECT
        ui.NewUserId,
        6,
        NULL,
        ui.UserCode,
        ui.Login,
        ui.Email,
        ui.FirstName,
        ui.LastName,
        N'it-IT',
        1,
        0,
        NULL,
        0,
        @EffectiveUserId,
        GETDATE(),
        @EffectiveUserId,
        GETDATE(),
        0,
        LEFT(CONCAT(ui.FirstName, N' ', ui.LastName), 511),
        0,
        N'ORYX',
        0
    FROM @UsersToInsert ui;

    INSERT INTO @PersonChanges
    (
        SourceVZPersonId,
        UserId,
        [Action],
        Login,
        Email,
        FirstName,
        LastName,
        ChangeDate
    )
    SELECT
        ui.SourceVZPersonId,
        ui.NewUserId,
        N'INSERTED',
        ui.Login,
        ui.Email,
        ui.FirstName,
        ui.LastName,
        @Now
    FROM @UsersToInsert ui;

    INSERT INTO @PersonMap
    (
        SourceVZPersonId,
        UserId,
        FirstName,
        LastName,
        Email
    )
    SELECT
        sp.SourceVZPersonId,
        CONVERT(BIGINT, bu.UserId),
        bu.FirstName,
        bu.LastName,
        bu.Email
    FROM @SourcePersons sp
    INNER JOIN dbo.base_Users bu ON bu.Code = sp.UserCode COLLATE DATABASE_DEFAULT;

    UPDATE ut
       SET ut.IsActive = 1,
           ut.IsDefault = CASE WHEN ut.IsDefault = 1 THEN 1 ELSE 0 END,
           ut.LastUpdatedById = @EffectiveUserId,
           ut.LastUpdatedByFullName = @EffectiveUserFullName,
           ut.LastUpdateDate = @Now,
           ut.IsDeleted = 0
    FROM dbo.UserTenant ut
    INNER JOIN @SourcePersons sp
            ON ut.UserId = (
                SELECT bu.UserId
                FROM dbo.base_Users bu
                WHERE bu.Code = sp.UserCode COLLATE DATABASE_DEFAULT
            )
           AND ut.TenantId = @TargetTenantId;

    INSERT INTO @UserTenantChanges
    (
        SourceVZPersonId,
        UserTenantId,
        UserId,
        TenantId,
        [Action],
        ChangeDate
    )
    SELECT
        sp.SourceVZPersonId,
        ut.Id,
        ut.UserId,
        ut.TenantId,
        N'UPDATED',
        @Now
    FROM dbo.UserTenant ut
    INNER JOIN @SourcePersons sp
            ON ut.UserId = (
                SELECT bu.UserId
                FROM dbo.base_Users bu
                WHERE bu.Code = sp.UserCode COLLATE DATABASE_DEFAULT
            )
           AND ut.TenantId = @TargetTenantId;

    ;WITH MissingUserTenant AS (
        SELECT
            sp.SourceVZPersonId,
            bu.UserId,
            @TargetTenantId AS TenantId,
            CAST(0 AS BIT) AS IsDefault,
            ROW_NUMBER() OVER (ORDER BY sp.SourceVZPersonId) AS rn
        FROM @SourcePersons sp
        INNER JOIN dbo.base_Users bu ON bu.Code = sp.UserCode COLLATE DATABASE_DEFAULT
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.UserTenant ut
            WHERE ut.UserId = bu.UserId
              AND ut.TenantId = @TargetTenantId
        )
    ), BaseUserTenantId AS (
        SELECT ISNULL(MAX(ut.Id), 0) AS MaxId
        FROM dbo.UserTenant ut
    )
    INSERT INTO @UserTenantToInsert
    (
        NewId,
        SourceVZPersonId,
        UserId,
        TenantId,
        IsDefault
    )
    SELECT
        buti.MaxId + mut.rn,
        mut.SourceVZPersonId,
        mut.UserId,
        mut.TenantId,
        mut.IsDefault
    FROM MissingUserTenant mut
    CROSS JOIN BaseUserTenantId buti;

    INSERT INTO dbo.UserTenant
    (
        Id,
        UserId,
        TenantId,
        IsDefault,
        IsActive,
        CreatedById,
        CreatedByFullName,
        CreationDate,
        LastUpdatedById,
        LastUpdatedByFullName,
        LastUpdateDate,
        IsDeleted
    )
    SELECT
        uti.NewId,
        uti.UserId,
        uti.TenantId,
        uti.IsDefault,
        1,
        @EffectiveUserId,
        @EffectiveUserFullName,
        @Now,
        NULL,
        NULL,
        NULL,
        0
    FROM @UserTenantToInsert uti;

    INSERT INTO @UserTenantChanges
    (
        SourceVZPersonId,
        UserTenantId,
        UserId,
        TenantId,
        [Action],
        ChangeDate
    )
    SELECT
        uti.SourceVZPersonId,
        uti.NewId,
        uti.UserId,
        uti.TenantId,
        N'INSERTED',
        @Now
    FROM @UserTenantToInsert uti;

    INSERT INTO @SourceOwnerContracts
    (
        SourceVZContractId,
        SourceVZPersonId,
        SourceVZUnitId,
        TargetUnitId,
        TargetUserId,
        OwnerType,
        OwnershipQuota,
        StartDate,
        EndDate,
        IsResident,
        Notes,
        FirstName,
        LastName,
        Email
    )
    SELECT
        c.id AS SourceVZContractId,
        c.idPersona AS SourceVZPersonId,
        c.idImmobile AS SourceVZUnitId,
        u.Id AS TargetUnitId,
        pm.UserId AS TargetUserId,
        CASE
            WHEN c.tipo = 1 THEN N'Proprietario'
            WHEN c.tipo = 2 THEN N'Usufruttuario'
            ELSE N'Proprietario'
        END AS OwnerType,
        CAST(
            CASE
                WHEN TRY_CONVERT(DECIMAL(18,4), c.percentuale) IS NULL THEN 100.0000
                WHEN TRY_CONVERT(DECIMAL(18,4), c.percentuale) <= 1 THEN TRY_CONVERT(DECIMAL(18,4), c.percentuale) * 100
                ELSE TRY_CONVERT(DECIMAL(18,4), c.percentuale)
            END AS DECIMAL(18,4)
        ) AS OwnershipQuota,
        ISNULL(TRY_CONVERT(DATETIME2, c.dataInizio), @Now) AS StartDate,
        TRY_CONVERT(DATETIME2, c.dataFine) AS EndDate,
        CASE WHEN ISNULL(c.persone, 0) > 0 THEN 1 ELSE 0 END AS IsResident,
        LEFT(
            CONCAT(
                N'[VZCONTRATTO ID=',
                CONVERT(NVARCHAR(20), c.id),
                N']',
                CASE
                    WHEN NULLIF(LTRIM(RTRIM(c.note)), N'') IS NULL THEN N''
                    ELSE CONCAT(N' ', LEFT(CONVERT(NVARCHAR(400), LTRIM(RTRIM(c.note))), 400))
                END
            ),
            500
        ) AS Notes,
        LEFT(pm.FirstName, 100),
        LEFT(pm.LastName, 100),
        LEFT(pm.Email, 200)
    FROM [Gestione].[VZContratto] c
    INNER JOIN [Domus].[VZImmobile] i ON i.id = c.idImmobile
    INNER JOIN [Domus].[VZEdificio] b ON b.id = i.idEdificio
    LEFT JOIN dbo.RealEstateUnit u
           ON u.CondominiumId = @TargetCondominiumId
          AND CHARINDEX(CONCAT(N'[VZ ID=', CONVERT(NVARCHAR(20), c.idImmobile), N']'), ISNULL(u.Notes, N'')) > 0
          AND u.IsDeleted = 0
    LEFT JOIN @PersonMap pm ON pm.SourceVZPersonId = c.idPersona
    WHERE b.idCondominio = @SourceVZCondominioId;

    IF EXISTS (
        SELECT 1
        FROM @SourceOwnerContracts soc
        WHERE soc.TargetUnitId IS NULL
    )
    BEGIN
        THROW 51007, 'Impossibile associare uno o piu'' contratti alle unita'' target.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM @SourceOwnerContracts soc
        WHERE soc.TargetUserId IS NULL
    )
    BEGIN
        THROW 51008, 'Impossibile associare uno o piu'' contratti ai base_Users migrati.', 1;
    END;

    UPDATE uo
       SET uo.TenantId = @TargetTenantId,
           uo.UnitId = soc.TargetUnitId,
           uo.UserId = soc.TargetUserId,
           uo.OwnerType = soc.OwnerType,
           uo.OwnershipQuota = CASE
                                   WHEN soc.OwnershipQuota < 0 THEN 0
                                   WHEN soc.OwnershipQuota > 100 THEN 100
                                   ELSE soc.OwnershipQuota
                               END,
           uo.StartDate = soc.StartDate,
           uo.EndDate = soc.EndDate,
           uo.IsResident = soc.IsResident,
           uo.IsActive = 1,
           uo.IsAccessEnabled = 1,
           uo.Notes = soc.Notes,
           uo.FirstName = soc.FirstName,
           uo.LastName = soc.LastName,
           uo.Email = soc.Email,
           uo.IsDeleted = 0,
           uo.LastUpdatedById = @EffectiveUserId,
           uo.LastUpdatedByFullName = @EffectiveUserFullName,
           uo.LastUpdateDate = @Now
    FROM dbo.UnitOwner uo
    INNER JOIN @SourceOwnerContracts soc
            ON CHARINDEX(CONCAT(N'[VZCONTRATTO ID=', CONVERT(NVARCHAR(20), soc.SourceVZContractId), N']'), ISNULL(uo.Notes, N'')) > 0
    WHERE uo.TenantId = @TargetTenantId;

    INSERT INTO @UnitOwnerChanges
    (
        SourceVZContractId,
        UnitOwnerId,
        UnitId,
        UserId,
        [Action],
        OwnershipQuota,
        StartDate,
        EndDate,
        ChangeDate
    )
    SELECT
        soc.SourceVZContractId,
        uo.Id,
        uo.UnitId,
        uo.UserId,
        N'UPDATED',
        uo.OwnershipQuota,
        uo.StartDate,
        uo.EndDate,
        @Now
    FROM dbo.UnitOwner uo
    INNER JOIN @SourceOwnerContracts soc
            ON CHARINDEX(CONCAT(N'[VZCONTRATTO ID=', CONVERT(NVARCHAR(20), soc.SourceVZContractId), N']'), ISNULL(uo.Notes, N'')) > 0
    WHERE uo.TenantId = @TargetTenantId;

    ;WITH MissingOwners AS (
        SELECT
            soc.SourceVZContractId,
            soc.TargetUnitId,
            soc.TargetUserId,
            soc.OwnerType,
            CASE
                WHEN soc.OwnershipQuota < 0 THEN CAST(0 AS DECIMAL(18,4))
                WHEN soc.OwnershipQuota > 100 THEN CAST(100 AS DECIMAL(18,4))
                ELSE soc.OwnershipQuota
            END AS OwnershipQuota,
            soc.StartDate,
            soc.EndDate,
            soc.IsResident,
            soc.Notes,
            soc.FirstName,
            soc.LastName,
            soc.Email,
            ROW_NUMBER() OVER (ORDER BY soc.SourceVZContractId) AS rn
        FROM @SourceOwnerContracts soc
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.UnitOwner uo
            WHERE uo.TenantId = @TargetTenantId
              AND CHARINDEX(CONCAT(N'[VZCONTRATTO ID=', CONVERT(NVARCHAR(20), soc.SourceVZContractId), N']'), ISNULL(uo.Notes, N'')) > 0
        )
    ), BaseOwnerId AS (
        SELECT ISNULL(MAX(uo.Id), 0) AS MaxId
        FROM dbo.UnitOwner uo
    )
    INSERT INTO @UnitOwnerToInsert
    (
        NewId,
        SourceVZContractId,
        TargetUnitId,
        TargetUserId,
        OwnerType,
        OwnershipQuota,
        StartDate,
        EndDate,
        IsResident,
        Notes,
        FirstName,
        LastName,
        Email
    )
    SELECT
        bo.MaxId + mo.rn,
        mo.SourceVZContractId,
        mo.TargetUnitId,
        mo.TargetUserId,
        mo.OwnerType,
        mo.OwnershipQuota,
        mo.StartDate,
        mo.EndDate,
        mo.IsResident,
        mo.Notes,
        mo.FirstName,
        mo.LastName,
        mo.Email
    FROM MissingOwners mo
    CROSS JOIN BaseOwnerId bo;

    INSERT INTO dbo.UnitOwner
    (
        Id,
        TenantId,
        UnitId,
        UserId,
        OwnerType,
        OwnershipQuota,
        StartDate,
        EndDate,
        IsResident,
        IsActive,
        Notes,
        CreatedById,
        CreatedByFullName,
        CreationDate,
        LastUpdatedById,
        LastUpdatedByFullName,
        LastUpdateDate,
        IsDeleted,
        FirstName,
        LastName,
        Email,
        IsAccessEnabled
    )
    SELECT
        uoi.NewId,
        @TargetTenantId,
        uoi.TargetUnitId,
        uoi.TargetUserId,
        uoi.OwnerType,
        uoi.OwnershipQuota,
        uoi.StartDate,
        uoi.EndDate,
        uoi.IsResident,
        1,
        uoi.Notes,
        @EffectiveUserId,
        @EffectiveUserFullName,
        @Now,
        NULL,
        NULL,
        NULL,
        0,
        uoi.FirstName,
        uoi.LastName,
        uoi.Email,
        1
    FROM @UnitOwnerToInsert uoi;

    INSERT INTO @UnitOwnerChanges
    (
        SourceVZContractId,
        UnitOwnerId,
        UnitId,
        UserId,
        [Action],
        OwnershipQuota,
        StartDate,
        EndDate,
        ChangeDate
    )
    SELECT
        uoi.SourceVZContractId,
        uoi.NewId,
        uoi.TargetUnitId,
        uoi.TargetUserId,
        N'INSERTED',
        uoi.OwnershipQuota,
        uoi.StartDate,
        uoi.EndDate,
        @Now
    FROM @UnitOwnerToInsert uoi;

    SELECT @BuildingMaxId = ISNULL(MAX(b.Id), 0)
    FROM [dbo].[Building] b;

    SET @BuildingComputedNextHi = CASE
        WHEN @BuildingMaxId <= 0 THEN 1
        ELSE CAST(FLOOR((1.0 * @BuildingMaxId) / @BuildingBlock) + 1 AS INT)
    END;

    SELECT @CurrentNextHi = huk.next_hi
    FROM [dbo].[hibernate_unique_key] huk
    WHERE huk.entity_type = 'Building';

    IF @CurrentNextHi IS NULL
    BEGIN
        INSERT INTO [dbo].[hibernate_unique_key] (next_hi, entity_type)
        VALUES (@BuildingComputedNextHi, 'Building');
    END
    ELSE IF @CurrentNextHi < @BuildingComputedNextHi
    BEGIN
        UPDATE [dbo].[hibernate_unique_key]
           SET next_hi = @BuildingComputedNextHi
         WHERE entity_type = 'Building';
    END;

    SELECT @StaircaseMaxId = ISNULL(MAX(s.Id), 0)
    FROM [dbo].[Staircase] s;

    SET @StaircaseComputedNextHi = CASE
        WHEN @StaircaseMaxId <= 0 THEN 1
        ELSE CAST(FLOOR((1.0 * @StaircaseMaxId) / @StaircaseBlock) + 1 AS INT)
    END;

    SELECT @CurrentStaircaseNextHi = huk.next_hi
    FROM [dbo].[hibernate_unique_key] huk
    WHERE huk.entity_type = 'Staircase';

    IF @CurrentStaircaseNextHi IS NULL
    BEGIN
        INSERT INTO [dbo].[hibernate_unique_key] (next_hi, entity_type)
        VALUES (@StaircaseComputedNextHi, 'Staircase');
    END
    ELSE IF @CurrentStaircaseNextHi < @StaircaseComputedNextHi
    BEGIN
        UPDATE [dbo].[hibernate_unique_key]
           SET next_hi = @StaircaseComputedNextHi
         WHERE entity_type = 'Staircase';
    END;

    SELECT @RealEstateUnitMaxId = ISNULL(MAX(u.Id), 0)
    FROM [dbo].[RealEstateUnit] u;

    SET @RealEstateUnitComputedNextHi = CASE
        WHEN @RealEstateUnitMaxId <= 0 THEN 1
        ELSE CAST(FLOOR((1.0 * @RealEstateUnitMaxId) / @RealEstateUnitBlock) + 1 AS INT)
    END;

    SELECT @CurrentRealEstateUnitNextHi = huk.next_hi
    FROM [dbo].[hibernate_unique_key] huk
    WHERE huk.entity_type = 'RealEstateUnit';

    IF @CurrentRealEstateUnitNextHi IS NULL
    BEGIN
        INSERT INTO [dbo].[hibernate_unique_key] (next_hi, entity_type)
        VALUES (@RealEstateUnitComputedNextHi, 'RealEstateUnit');
    END
    ELSE IF @CurrentRealEstateUnitNextHi < @RealEstateUnitComputedNextHi
    BEGIN
        UPDATE [dbo].[hibernate_unique_key]
           SET next_hi = @RealEstateUnitComputedNextHi
         WHERE entity_type = 'RealEstateUnit';
    END;

    SELECT @UnitOwnerMaxId = ISNULL(MAX(uo.Id), 0)
    FROM [dbo].[UnitOwner] uo;

    SET @UnitOwnerComputedNextHi = CASE
        WHEN @UnitOwnerMaxId <= 0 THEN 1
        ELSE CAST(FLOOR((1.0 * @UnitOwnerMaxId) / @UnitOwnerBlock) + 1 AS INT)
    END;

    SELECT @CurrentUnitOwnerNextHi = huk.next_hi
    FROM [dbo].[hibernate_unique_key] huk
    WHERE huk.entity_type = 'UnitOwner';

    IF @CurrentUnitOwnerNextHi IS NULL
    BEGIN
        INSERT INTO [dbo].[hibernate_unique_key] (next_hi, entity_type)
        VALUES (@UnitOwnerComputedNextHi, 'UnitOwner');
    END
    ELSE IF @CurrentUnitOwnerNextHi < @UnitOwnerComputedNextHi
    BEGIN
        UPDATE [dbo].[hibernate_unique_key]
           SET next_hi = @UnitOwnerComputedNextHi
         WHERE entity_type = 'UnitOwner';
    END;

    SELECT @UserTenantMaxId = ISNULL(MAX(ut.Id), 0)
    FROM [dbo].[UserTenant] ut;

    SET @UserTenantComputedNextHi = CASE
        WHEN @UserTenantMaxId <= 0 THEN 1
        ELSE CAST(FLOOR((1.0 * @UserTenantMaxId) / @UserTenantBlock) + 1 AS INT)
    END;

    SELECT @CurrentUserTenantNextHi = huk.next_hi
    FROM [dbo].[hibernate_unique_key] huk
    WHERE huk.entity_type = 'UserTenant';

    IF @CurrentUserTenantNextHi IS NULL
    BEGIN
        INSERT INTO [dbo].[hibernate_unique_key] (next_hi, entity_type)
        VALUES (@UserTenantComputedNextHi, 'UserTenant');
    END
    ELSE IF @CurrentUserTenantNextHi < @UserTenantComputedNextHi
    BEGIN
        UPDATE [dbo].[hibernate_unique_key]
           SET next_hi = @UserTenantComputedNextHi
         WHERE entity_type = 'UserTenant';
    END;

    COMMIT TRANSACTION;

    -- Risultato finale (prima/dopo)
    SELECT
        ch.CondominiumId,
        ch.OldName,
        ch.NewName,
        ch.OldTaxCode,
        ch.NewTaxCode,
        ch.OldIban,
        ch.NewIban,
        ch.OldNotes,
        ch.NewNotes,
        ch.ChangeDate
    FROM @Changes ch;

    -- Snapshot record sorgente e destinazione aggiornato
    SELECT
        vz.id AS SourceVZCondominioId,
        vz.nome AS SourceNome,
        vz.codiceFiscale AS SourceCodiceFiscale,
        vz.iban AS SourceIban,
        vz.note AS SourceNote
    FROM [Domus].[VZCondominio] vz
    WHERE vz.id = @SourceVZCondominioId;

    SELECT
        c.Id AS TargetCondominiumId,
        c.Name,
        c.TaxCode,
        c.Iban,
        c.Notes,
        c.LastUpdatedById,
        c.LastUpdatedByFullName,
        c.LastUpdateDate
    FROM [dbo].[Condominium] c
    WHERE c.Id = @TargetCondominiumId;

    -- Edifici sincronizzati (UPDATED/INSERTED) nel condominio destinazione
    SELECT
        bc.SourceVZBuildingId,
        bc.BuildingId,
        bc.[Action],
        bc.BuildingName,
        bc.BuildingAddress,
        bc.ChangeDate
    FROM @BuildingChanges bc
    ORDER BY bc.SourceVZBuildingId;

    -- Scale sincronizzate (UPDATED/INSERTED) nel condominio destinazione
    SELECT
        sc.SourceVZStaircaseId,
        sc.StaircaseId,
        sc.BuildingId,
        sc.[Action],
        sc.StaircaseName,
        sc.ChangeDate
    FROM @StaircaseChanges sc
    ORDER BY sc.SourceVZStaircaseId;

    -- Unita' sincronizzate (UPDATED/INSERTED) nel condominio destinazione
    SELECT
        uc.SourceVZUnitId,
        uc.RealEstateUnitId,
        uc.BuildingId,
        uc.StaircaseId,
        uc.[Action],
        uc.InternalNumber,
        uc.Floor,
        uc.ChangeDate
    FROM @UnitChanges uc
    ORDER BY uc.SourceVZUnitId;

    -- Persone sincronizzate (UPDATED/INSERTED) in base_Users
    SELECT
        pc.SourceVZPersonId,
        pc.UserId,
        pc.[Action],
        pc.Login,
        pc.Email,
        pc.FirstName,
        pc.LastName,
        pc.ChangeDate
    FROM @PersonChanges pc
    ORDER BY pc.SourceVZPersonId;

    -- UserTenant sincronizzati (UPDATED/INSERTED) per il tenant target
    SELECT
        utc.SourceVZPersonId,
        utc.UserTenantId,
        utc.UserId,
        utc.TenantId,
        utc.[Action],
        utc.ChangeDate
    FROM @UserTenantChanges utc
    ORDER BY utc.SourceVZPersonId;

    -- Proprietari unita' sincronizzati (UPDATED/INSERTED) in UnitOwner
    SELECT
        uoc.SourceVZContractId,
        uoc.UnitOwnerId,
        uoc.UnitId,
        uoc.UserId,
        uoc.[Action],
        uoc.OwnershipQuota,
        uoc.StartDate,
        uoc.EndDate,
        uoc.ChangeDate
    FROM @UnitOwnerChanges uoc
    ORDER BY uoc.SourceVZContractId;

    -- Snapshot edifici attivi del condominio destinazione
    SELECT
        b.Id,
        b.Name,
        b.Code,
        b.Address,
        b.NumberOfFloors,
        b.IsActive,
        b.IsDeleted,
        b.CreationDate
    FROM [dbo].[Building] b
    WHERE b.CondominiumId = @TargetCondominiumId
      AND b.IsDeleted = 0
    ORDER BY b.Name, b.Id;

        -- Snapshot scale attive del condominio destinazione
        SELECT
                s.Id,
                s.Name,
                s.BuildingId,
                s.IsActive,
                s.IsDeleted,
                s.CreationDate,
                s.LastUpdateDate
        FROM [dbo].[Staircase] s
        WHERE s.CondominiumId = @TargetCondominiumId
            AND s.IsDeleted = 0
        ORDER BY s.Name, s.Id;

        -- Snapshot unita' attive del condominio destinazione
        SELECT
                u.Id,
                u.BuildingId,
                u.StaircaseId,
                u.Staircase,
                u.Floor,
                u.InternalNumber,
                u.Subordinate,
                u.CadastralIncome,
                u.UnitType,
                u.OccupancyStatus,
                u.IsActive,
                u.IsDeleted,
                u.CreationDate,
                u.LastUpdateDate
        FROM [dbo].[RealEstateUnit] u
        WHERE u.CondominiumId = @TargetCondominiumId
            AND u.IsDeleted = 0
        ORDER BY u.BuildingId, u.Staircase, u.Floor, u.InternalNumber, u.Id;

        -- Snapshot proprietari unita' attivi del condominio destinazione
        SELECT
                uo.Id,
                uo.UnitId,
                uo.UserId,
                uo.OwnerType,
                uo.OwnershipQuota,
                uo.StartDate,
                uo.EndDate,
                uo.IsResident,
                uo.IsAccessEnabled,
                uo.IsActive,
                uo.Notes,
                uo.FirstName,
                uo.LastName,
                uo.Email
        FROM [dbo].[UnitOwner] uo
        INNER JOIN [dbo].[RealEstateUnit] u ON u.Id = uo.UnitId
        WHERE u.CondominiumId = @TargetCondominiumId
            AND uo.IsDeleted = 0
        ORDER BY uo.UnitId, uo.Id;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrLine INT = ERROR_LINE();
    DECLARE @ThrowMsg NVARCHAR(2048);
    SET @ThrowMsg = N'Migrazione fallita alla linea ' + CONVERT(NVARCHAR(20), @ErrLine) + N': ' + ISNULL(@ErrMsg, N'Errore sconosciuto');
    THROW 51099, @ThrowMsg, 1;
END CATCH;
GO
