-- ============================================================
-- Migrazione Eagle_Auth → DomuWave (schema auth)
-- Crea lo schema auth nel database DomuWave e ricrea
-- tutte le tabelle/view di Eagle_Auth.
--
-- Per tornare al setup a due database: nessuna modifica al
-- codice, basta ripristinare sql-auth in appsettings.
-- ============================================================

USE DomuWave;
GO

-- ── Schema ────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'auth')
    EXEC('CREATE SCHEMA auth');
GO

-- ── hibernate_unique_key (se non esiste già nello schema auth) ─
-- Le entry per le entità auth devono coesistere nella tabella
-- che la factory AUTH usa. Poiché la factory AUTH punta ora
-- allo stesso DB, usiamo la stessa tabella dbo.hibernate_unique_key.
-- Aggiungiamo solo le entry mancanti.

INSERT INTO dbo.hibernate_unique_key (entity_type, next_hi)
SELECT e.entity_type, 1
FROM (VALUES
    ('IUser'),
    ('GroupBase'),
    ('GroupAuthorization'),
    ('UserAuthorization'),
    ('DefaultStatusFilter')
) AS e(entity_type)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.hibernate_unique_key h WHERE h.entity_type = e.entity_type
);
GO

-- ── auth_Modules ──────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'auth_Modules' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.auth_Modules (
    ModuleId    INT           NOT NULL PRIMARY KEY,
    Code        NVARCHAR(50)  NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL,
    SortIndex   INT           NULL,
    IsDefault   BIT           NOT NULL DEFAULT 0,
    IsActive    BIT           NOT NULL DEFAULT 1
);
GO

-- ── auth_Areas ────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'auth_Areas' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.auth_Areas (
    AreaId      INT           NOT NULL PRIMARY KEY,
    Code        NVARCHAR(50)  NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL,
    IsActive    BIT           NOT NULL DEFAULT 1,
    ModuleId    INT           NOT NULL,
    CONSTRAINT FK_auth_Areas_Modules FOREIGN KEY (ModuleId) REFERENCES auth.auth_Modules(ModuleId)
);
GO

-- ── auth_Authorizations ───────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'auth_Authorizations' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.auth_Authorizations (
    AuthorizationId INT           NOT NULL PRIMARY KEY,
    Code            NVARCHAR(50)  NOT NULL UNIQUE,
    Description     NVARCHAR(255) NOT NULL,
    AreaId          INT           NOT NULL,
    CONSTRAINT FK_auth_Authorizations_Areas FOREIGN KEY (AreaId) REFERENCES auth.auth_Areas(AreaId)
);
GO

-- ── auth_Groups (GroupBase + Group + Role via discriminator) ───
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'auth_Groups' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.auth_Groups (
    GroupId         INT            NOT NULL PRIMARY KEY,
    Discriminator   NVARCHAR(50)   NOT NULL,          -- 'group' | 'role'
    Code            NVARCHAR(50)   NOT NULL UNIQUE,
    Description     NVARCHAR(1000) NOT NULL,
    Weight          INT            NULL,
    IsActive        BIT            NOT NULL DEFAULT 1,
    IsFieldForceGroup BIT          NOT NULL DEFAULT 0,
    IsSystemEntity  BIT            NOT NULL DEFAULT 0,
    IsDefault       BIT            NOT NULL DEFAULT 0,
    IsDeleted       BIT            NOT NULL DEFAULT 0,
    ParentId        INT            NULL
);
GO

-- ── base_Users ────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'base_Users' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.base_Users (
    UserId          INT            NOT NULL PRIMARY KEY,
    Login           NVARCHAR(255)  NOT NULL UNIQUE,
    Email           NVARCHAR(255)  NOT NULL,
    FirstName       NVARCHAR(200)  NOT NULL,
    LastName        NVARCHAR(200)  NOT NULL,
    Password        NVARCHAR(MAX)  NULL,
    Code            NVARCHAR(100)  NULL,
    Token           NVARCHAR(MAX)  NULL,
    AvatarPath      NVARCHAR(500)  NULL DEFAULT '/content/images/avatar/avatar.png',
    Path            NVARCHAR(MAX)  NULL,
    IsActive        BIT            NOT NULL DEFAULT 1,
    IsDeleted       BIT            NOT NULL DEFAULT 0,
    IsSystemUser    BIT            NOT NULL DEFAULT 0,
    PasswordExpired BIT            NOT NULL DEFAULT 0,
    RoleId          INT            NULL,
    SupervisorUserId INT           NULL,
    CreatedById     INT            NOT NULL,
    CreationDate    DATETIME2      NOT NULL,
    LastUpdatedById INT            NOT NULL DEFAULT 0,
    LastUpdateDate  DATETIME2      NULL,
    CONSTRAINT FK_base_Users_Role    FOREIGN KEY (RoleId)           REFERENCES auth.auth_Groups(GroupId),
    CONSTRAINT FK_base_Users_Supervisor FOREIGN KEY (SupervisorUserId) REFERENCES auth.base_Users(UserId)
);
GO

-- ── auth_GroupsAuthorizations ─────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'auth_GroupsAuthorizations' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.auth_GroupsAuthorizations (
    GroupAuthorizationId INT  NOT NULL PRIMARY KEY,
    AuthorizationId      INT  NOT NULL,
    GroupId              INT  NULL,
    RoleId               INT  NULL,
    CanView              BIT  NOT NULL DEFAULT 0,
    CanCreate            BIT  NOT NULL DEFAULT 0,
    CanModify            BIT  NOT NULL DEFAULT 0,
    CanDelete            BIT  NOT NULL DEFAULT 0,
    CanAction            BIT  NOT NULL DEFAULT 0,
    CONSTRAINT FK_auth_GA_Authorization FOREIGN KEY (AuthorizationId) REFERENCES auth.auth_Authorizations(AuthorizationId),
    CONSTRAINT FK_auth_GA_Group         FOREIGN KEY (GroupId)         REFERENCES auth.auth_Groups(GroupId),
    CONSTRAINT FK_auth_GA_Role          FOREIGN KEY (RoleId)          REFERENCES auth.auth_Groups(GroupId)
);
GO

-- ── auth_UsersAuthorizations ──────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'auth_UsersAuthorizations' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.auth_UsersAuthorizations (
    UserAuthorizationId INT  NOT NULL PRIMARY KEY,
    UserId              INT  NOT NULL,
    AuthorizationId     INT  NOT NULL,
    CanView             BIT  NOT NULL DEFAULT 0,
    CanCreate           BIT  NOT NULL DEFAULT 0,
    CanModify           BIT  NOT NULL DEFAULT 0,
    CanDelete           BIT  NOT NULL DEFAULT 0,
    CanAction           BIT  NOT NULL DEFAULT 0,
    CONSTRAINT FK_auth_UA_User          FOREIGN KEY (UserId)          REFERENCES auth.base_Users(UserId),
    CONSTRAINT FK_auth_UA_Authorization FOREIGN KEY (AuthorizationId) REFERENCES auth.auth_Authorizations(AuthorizationId)
);
GO

-- ── auth_UsersGroups (join table) ─────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'auth_UsersGroups' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.auth_UsersGroups (
    UserId  INT NOT NULL,
    GroupId INT NOT NULL,
    CONSTRAINT PK_auth_UsersGroups PRIMARY KEY (UserId, GroupId),
    CONSTRAINT FK_auth_UG_User  FOREIGN KEY (UserId)  REFERENCES auth.base_Users(UserId),
    CONSTRAINT FK_auth_UG_Group FOREIGN KEY (GroupId) REFERENCES auth.auth_Groups(GroupId)
);
GO

-- ── auth_GroupsModules (join table) ───────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'auth_GroupsModules' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.auth_GroupsModules (
    GroupId  INT NOT NULL,
    ModuleId INT NOT NULL,
    CONSTRAINT PK_auth_GroupsModules PRIMARY KEY (GroupId, ModuleId),
    CONSTRAINT FK_auth_GM_Group  FOREIGN KEY (GroupId)  REFERENCES auth.auth_Groups(GroupId),
    CONSTRAINT FK_auth_GM_Module FOREIGN KEY (ModuleId) REFERENCES auth.auth_Modules(ModuleId)
);
GO

-- ── vw_flatuserauthorizations ─────────────────────────────────
-- Vista che piatta tutte le autorizzazioni dell'utente
-- (dirette + da gruppo + da ruolo)
IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'vw_flatuserauthorizations' AND schema_id = SCHEMA_ID('auth'))
    DROP VIEW auth.vw_flatuserauthorizations;
GO
CREATE VIEW auth.vw_flatuserauthorizations AS
-- Autorizzazioni dirette dell'utente
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

-- Autorizzazioni via gruppo
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

-- Autorizzazioni via ruolo
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

-- ── vw_DefaultStatusFilters ───────────────────────────────────
IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'vw_DefaultStatusFilters' AND schema_id = SCHEMA_ID('auth'))
    DROP VIEW auth.vw_DefaultStatusFilters;
GO
CREATE VIEW auth.vw_DefaultStatusFilters AS
SELECT
    ga.GroupAuthorizationId AS DefaultStatusFilterId,
    a.Code,
    ga.GroupId,
    NULL AS EntityFullName     -- Popolato se necessario tramite metadati applicativi
FROM auth.auth_GroupsAuthorizations ga
JOIN auth.auth_Authorizations a ON a.AuthorizationId = ga.AuthorizationId
WHERE 1 = 0;  -- Vista placeholder: la logica reale dipende dall'implementazione specifica
GO

-- ============================================================
-- MIGRAZIONE DATI da Eagle_Auth (eseguire solo se entrambi
-- i database sono raggiungibili dallo stesso server)
-- ============================================================
-- Se Eagle_Auth è sullo stesso SQL Server instance:
--
-- INSERT INTO auth.auth_Modules    SELECT * FROM Eagle_Auth.dbo.auth_Modules;
-- INSERT INTO auth.auth_Areas      SELECT * FROM Eagle_Auth.dbo.auth_Areas;
-- INSERT INTO auth.auth_Authorizations SELECT * FROM Eagle_Auth.dbo.auth_Authorizations;
-- INSERT INTO auth.auth_Groups     SELECT * FROM Eagle_Auth.dbo.auth_Groups;
-- INSERT INTO auth.base_Users      SELECT * FROM Eagle_Auth.dbo.base_Users;
-- INSERT INTO auth.auth_GroupsAuthorizations SELECT * FROM Eagle_Auth.dbo.auth_GroupsAuthorizations;
-- INSERT INTO auth.auth_UsersAuthorizations  SELECT * FROM Eagle_Auth.dbo.auth_UsersAuthorizations;
-- INSERT INTO auth.auth_UsersGroups   SELECT * FROM Eagle_Auth.dbo.auth_UsersGroups;
-- INSERT INTO auth.auth_GroupsModules SELECT * FROM Eagle_Auth.dbo.auth_GroupsModules;
--
-- ============================================================
