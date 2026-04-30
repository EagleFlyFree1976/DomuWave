-- ============================================================
-- Sposta le tabelle di Eagle_Auth da schema dbo a schema auth
-- Eseguire sul database Eagle_Auth.
-- ATTENZIONE: operazione irreversibile senza uno script inverso.
-- ============================================================

USE Eagle_Auth;
GO

-- ── Crea schema auth se non esiste ───────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'auth')
    EXEC('CREATE SCHEMA auth');
GO

-- ── Sposta tabelle ────────────────────────────────────────────
-- L'ordine non importa per ALTER SCHEMA TRANSFER (non controlla FK)
ALTER SCHEMA auth TRANSFER dbo.auth_Modules;
ALTER SCHEMA auth TRANSFER dbo.auth_Areas;
ALTER SCHEMA auth TRANSFER dbo.auth_Authorizations;
ALTER SCHEMA auth TRANSFER dbo.auth_Groups;
ALTER SCHEMA auth TRANSFER dbo.base_Users;
ALTER SCHEMA auth TRANSFER dbo.auth_GroupsAuthorizations;
ALTER SCHEMA auth TRANSFER dbo.auth_UsersAuthorizations;
ALTER SCHEMA auth TRANSFER dbo.auth_UsersGroups;
ALTER SCHEMA auth TRANSFER dbo.auth_GroupsModules;
GO

-- ── Ricrea le view nello schema auth ──────────────────────────
IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'vw_flatuserauthorizations' AND schema_id = SCHEMA_ID('dbo'))
    DROP VIEW dbo.vw_flatuserauthorizations;
GO
IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'vw_flatuserauthorizations' AND schema_id = SCHEMA_ID('auth'))
    DROP VIEW auth.vw_flatuserauthorizations;
GO
CREATE VIEW auth.vw_flatuserauthorizations AS
SELECT
    ua.UserAuthorizationId  AS Id,
    ua.UserId,
    a.Code                  AS AuthorizationCode,
    ar.ModuleId,
    ua.CanView,
    ua.CanCreate,
    ua.CanModify,
    ua.CanDelete,
    ua.CanAction
FROM auth.auth_UsersAuthorizations ua
JOIN auth.auth_Authorizations a  ON a.AuthorizationId = ua.AuthorizationId
JOIN auth.auth_Areas          ar ON ar.AreaId          = a.AreaId

UNION ALL

SELECT
    ga.GroupAuthorizationId AS Id,
    ug.UserId,
    a.Code                  AS AuthorizationCode,
    ar.ModuleId,
    ga.CanView,
    ga.CanCreate,
    ga.CanModify,
    ga.CanDelete,
    ga.CanAction
FROM auth.auth_GroupsAuthorizations ga
JOIN auth.auth_UsersGroups          ug ON ug.GroupId = ga.GroupId
JOIN auth.auth_Authorizations        a ON a.AuthorizationId = ga.AuthorizationId
JOIN auth.auth_Areas                ar ON ar.AreaId = a.AreaId
WHERE ga.GroupId IS NOT NULL

UNION ALL

SELECT
    ga.GroupAuthorizationId AS Id,
    u.UserId,
    a.Code                  AS AuthorizationCode,
    ar.ModuleId,
    ga.CanView,
    ga.CanCreate,
    ga.CanModify,
    ga.CanDelete,
    ga.CanAction
FROM auth.auth_GroupsAuthorizations ga
JOIN auth.base_Users                 u ON u.RoleId = ga.RoleId
JOIN auth.auth_Authorizations        a ON a.AuthorizationId = ga.AuthorizationId
JOIN auth.auth_Areas                ar ON ar.AreaId = a.AreaId
WHERE ga.RoleId IS NOT NULL;
GO

IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'vw_DefaultStatusFilters' AND schema_id = SCHEMA_ID('dbo'))
    DROP VIEW dbo.vw_DefaultStatusFilters;
GO
IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'vw_DefaultStatusFilters' AND schema_id = SCHEMA_ID('auth'))
    DROP VIEW auth.vw_DefaultStatusFilters;
GO
CREATE VIEW auth.vw_DefaultStatusFilters AS
SELECT
    ga.GroupAuthorizationId AS DefaultStatusFilterId,
    a.Code,
    ga.GroupId,
    NULL AS EntityFullName
FROM auth.auth_GroupsAuthorizations ga
JOIN auth.auth_Authorizations a ON a.AuthorizationId = ga.AuthorizationId
WHERE 1 = 0;
GO

-- ── Verifica ──────────────────────────────────────────────────
SELECT s.name AS SchemaName, t.name AS TableName
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.name IN ('auth_Modules','auth_Areas','auth_Authorizations','auth_Groups',
                 'base_Users','auth_GroupsAuthorizations','auth_UsersAuthorizations',
                 'auth_UsersGroups','auth_GroupsModules')
ORDER BY s.name, t.name;
