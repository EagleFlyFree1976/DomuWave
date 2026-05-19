-- Seed autorizzazioni DomuWave per tutte le chiavi usate nei controller
-- Ogni blocco è idempotente: inserisce solo se il codice non esiste già.
-- Dopo ogni inserimento in auth_Authorizations assegna il permesso completo al gruppo -4.

DECLARE @code NVARCHAR(100);
DECLARE @description NVARCHAR(200);
DECLARE @authid INT;
DECLARE @authorizationid INT;

-- ── Condomini ────────────────────────────────────────────────────────────────
SET @code = N'Condominium';
SET @description = N'Gestione condomini';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    DECLARE @GroupAuthorizationId AS INT;
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Unità immobiliari ────────────────────────────────────────────────────────
SET @code = N'RealEstateUnit';
SET @description = N'Gestione unità immobiliari';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Proprietari unità ────────────────────────────────────────────────────────
SET @code = N'UnitOwner';
SET @description = N'Gestione proprietari delle unità immobiliari';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Conduttori unità ─────────────────────────────────────────────────────────
SET @code = N'UnitTenant';
SET @description = N'Gestione conduttori (inquilini) delle unità immobiliari';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Esercizi fiscali ─────────────────────────────────────────────────────────
SET @code = N'FSCY';
SET @description = N'Gestione esercizi fiscali';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Bilanci preventivi/consuntivi ────────────────────────────────────────────
SET @code = N'BDG';
SET @description = N'Gestione bilanci preventivi e consuntivi';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Voci di bilancio ─────────────────────────────────────────────────────────
SET @code = N'BudgetItem';
SET @description = N'Gestione voci di bilancio';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Spese ────────────────────────────────────────────────────────────────────
SET @code = N'Expense';
SET @description = N'Gestione spese condominiali';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Rate e oneri ─────────────────────────────────────────────────────────────
SET @code = N'Installment';
SET @description = N'Gestione rate e oneri condominiali';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Comunicazioni amministrative ─────────────────────────────────────────────
SET @code = N'Communication';
SET @description = N'Gestione comunicazioni ufficiali ai condomini';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Documenti ────────────────────────────────────────────────────────────────
SET @code = N'Document';
SET @description = N'Gestione documenti condominiali';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Bacheca condominiale ──────────────────────────────────────────────────────
SET @code = N'BoardPost';
SET @description = N'Gestione bacheca condominiale e messaggi tra condomini';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Assemblee ────────────────────────────────────────────────────────────────
SET @code = N'Assembly';
SET @description = N'Gestione assemblee condominiali';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Fornitori ────────────────────────────────────────────────────────────────
SET @code = N'Supplier';
SET @description = N'Gestione fornitori e ditte appaltatrici';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Contratti fornitori ───────────────────────────────────────────────────────
SET @code = N'SupplierContract';
SET @description = N'Gestione contratti con fornitori';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Tabelle millesimali ───────────────────────────────────────────────────────
SET @code = N'MillesimalTable';
SET @description = N'Gestione tabelle millesimali';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Millesimi per unità ───────────────────────────────────────────────────────
SET @code = N'UnitMillesimal';
SET @description = N'Gestione valori millesimali per unità immobiliare';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Piano dei conti ───────────────────────────────────────────────────────────
SET @code = N'ChartOfAccounts';
SET @description = N'Gestione piano dei conti condominiale';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Template piano dei conti ──────────────────────────────────────────────────
SET @code = N'ChartOfAccountsTemplate';
SET @description = N'Gestione template del piano dei conti';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Categorie piano dei conti ─────────────────────────────────────────────────
SET @code = N'ChartOfAccountsCategory';
SET @description = N'Gestione categorie del piano dei conti';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Gruppi di fatturazione ────────────────────────────────────────────────────
SET @code = N'BillingGroup';
SET @description = N'Gestione gruppi di fatturazione per le rate';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Edifici ───────────────────────────────────────────────────────────────────
SET @code = N'Building';
SET @description = N'Gestione edifici del condominio';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Scale ─────────────────────────────────────────────────────────────────────
SET @code = N'Staircase';
SET @description = N'Gestione scale e vani scala del condominio';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Manutenzioni ─────────────────────────────────────────────────────────────
SET @code = N'Maintenance';
SET @description = N'Gestione interventi di manutenzione ordinaria';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Lavori straordinari ───────────────────────────────────────────────────────
SET @code = N'ExtraordinaryWork';
SET @description = N'Gestione lavori straordinari e preventivi';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Consumi ───────────────────────────────────────────────────────────────────
SET @code = N'Consumption';
SET @description = N'Gestione consumi (acqua, gas, energia) per unità';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Saldi contabili ───────────────────────────────────────────────────────────
SET @code = N'AccountBalance';
SET @description = N'Gestione saldi e bilancio contabile per esercizio';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Dashboard ─────────────────────────────────────────────────────────────────
SET @code = N'Dashboard';
SET @description = N'Accesso alla dashboard riepilogativa';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Template notifiche ────────────────────────────────────────────────────────
SET @code = N'NotificationTemplate';
SET @description = N'Gestione template per le notifiche ai condomini';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Configurazione SMTP tenant ────────────────────────────────────────────────
SET @code = N'TenantSmtp';
SET @description = N'Configurazione server SMTP per l''invio email del tenant';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Notifiche comunicazioni ───────────────────────────────────────────────────
SET @code = N'CommunicationNotification';
SET @description = N'Gestione e invio notifiche legate alle comunicazioni';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Allegati notifiche ────────────────────────────────────────────────────────
SET @code = N'NotificationAttachment';
SET @description = N'Gestione allegati delle notifiche';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Allegati file ─────────────────────────────────────────────────────────────
SET @code = N'FileAttachment';
SET @description = N'Gestione allegati generici a entità del sistema';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── File dinamici ─────────────────────────────────────────────────────────────
SET @code = N'DynamicFile';
SET @description = N'Gestione repository file dinamici (upload/download)';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Segnalazioni guasti ───────────────────────────────────────────────────────
SET @code = N'Fault';
SET @description = N'Gestione segnalazioni di guasti e anomalie condominiali';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Messaggi privati ──────────────────────────────────────────────────────────
SET @code = N'PrivateThread';
SET @description = N'Gestione thread di messaggi privati tra condomino e amministratore';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Associazioni utente-tenant ────────────────────────────────────────────────
SET @code = N'UserTenant';
SET @description = N'Gestione associazioni tra utenti e tenant';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;

-- ── Utenti ────────────────────────────────────────────────────────────────────
SET @code = N'users';
SET @description = N'Gestione utenti del sistema';
IF (NOT EXISTS (SELECT * FROM auth_Authorizations WHERE code = @code))
BEGIN
    SELECT @authid = MAX([AuthorizationId]) + 1
    FROM auth_Authorizations;
    INSERT INTO auth_Authorizations
    (
        [AuthorizationId],
        [Code],
        [Description],
        [AreaId]
    )
    VALUES
    (@authid, @code, @description, 3);
END;
SELECT @authorizationid = [AuthorizationId]
FROM auth_Authorizations
WHERE code = @code;
IF (NOT EXISTS
(
    SELECT *
    FROM [auth_GroupsAuthorizations]
    WHERE AuthorizationId = @authorizationid
          AND RoleId = -4
)
   )
BEGIN
    SELECT @GroupAuthorizationId = MAX([GroupAuthorizationId]) + 1
    FROM [auth_GroupsAuthorizations];
    INSERT INTO [auth_GroupsAuthorizations]
    (
        [GroupAuthorizationId],
        [AuthorizationId],
        [GroupId],
        [RoleId],
        [CanView],
        [CanDelete],
        [CanCreate],
        [CanModify],
        [CanAction]
    )
    VALUES
    (@GroupAuthorizationId, @authorizationid, NULL, -4, 1, 1, 1, 1, 1);
END;
