-- ============================================================
-- Migrazione Eagle_Auth → DomuWave_SDB (schema auth)
-- Ricrea tutte le tabelle di Eagle_Auth.dbo nello schema auth
-- del database DomuWave_SDB, con struttura identica alla sorgente.
--
-- Per tornare al setup a due database: nessuna modifica al
-- codice, basta ripristinare sql-auth su Eagle_Auth in appsettings.
-- ============================================================

USE DomuWave_SDB;
GO

-- ── Schema ────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'auth')
    EXEC('CREATE SCHEMA auth');
GO

-- ── hibernate_unique_key ──────────────────────────────────────
-- Le entità auth usano la stessa tabella hilo del DB principale.
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
    ModuleId    INT            NOT NULL PRIMARY KEY,
    Code        NVARCHAR(50)   NOT NULL UNIQUE,
    Description NVARCHAR(255)  NULL,
    IsActive    BIT            NOT NULL DEFAULT 1,
    SortIndex   INT            NOT NULL DEFAULT 0,
    IsDefault   BIT            NOT NULL DEFAULT 0
);
GO

-- ── auth_Areas ────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'auth_Areas' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.auth_Areas (
    AreaId      INT            NOT NULL PRIMARY KEY,
    ModuleId    INT            NOT NULL,
    Code        NVARCHAR(50)   NOT NULL UNIQUE,
    Description NVARCHAR(255)  NULL,
    IsActive    BIT            NOT NULL DEFAULT 1,
    CONSTRAINT FK_auth_Areas_Modules FOREIGN KEY (ModuleId) REFERENCES auth.auth_Modules(ModuleId)
);
GO

-- ── auth_Authorizations ───────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'auth_Authorizations' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.auth_Authorizations (
    AuthorizationId INT            NOT NULL PRIMARY KEY,
    Code            NVARCHAR(50)   NOT NULL UNIQUE,
    Description     NVARCHAR(255)  NOT NULL,
    AreaId          INT            NOT NULL,
    CONSTRAINT FK_auth_Authorizations_Areas FOREIGN KEY (AreaId) REFERENCES auth.auth_Areas(AreaId)
);
GO

-- ── auth_Groups ───────────────────────────────────────────────
-- Contiene GroupBase + Group + Role via discriminator ('group'|'role')
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'auth_Groups' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.auth_Groups (
    GroupId           INT            NOT NULL PRIMARY KEY,
    Code              NVARCHAR(50)   NOT NULL UNIQUE,
    Description       NVARCHAR(1000) NOT NULL,
    IsActive          BIT            NOT NULL DEFAULT 1,
    Discriminator     NVARCHAR(255)  NULL,
    IsSystemEntity    BIT            NOT NULL DEFAULT 0,
    Weight            INT            NULL,
    IsFieldForceGroup BIT            NULL,
    IsDefault         BIT            NOT NULL DEFAULT 0,
    CompanyType       NVARCHAR(50)   NOT NULL DEFAULT '',
    IsDeleted         BIT            NOT NULL DEFAULT 0,
    CompanyId         INT            NULL,
    ParentId          INT            NULL,
    UseInternal       BIT            NOT NULL DEFAULT 0
);
GO

-- ── base_Users ────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'base_Users' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.base_Users (
    UserId             INT            NOT NULL PRIMARY KEY,
    RoleId             INT            NULL,
    SupervisorUserId   INT            NULL,
    Code               NVARCHAR(50)   NULL,
    Login              NVARCHAR(255)  NOT NULL UNIQUE,
    Email              NVARCHAR(255)  NOT NULL,
    FirstName          NVARCHAR(255)  NOT NULL,
    LastName           NVARCHAR(255)  NOT NULL,
    Culture            NVARCHAR(10)   NOT NULL DEFAULT 'it-IT',
    IsActive           BIT            NOT NULL DEFAULT 1,
    IsSystemUser       BIT            NOT NULL DEFAULT 0,
    Password           NVARCHAR(256)  NULL,
    AvatarPath         NVARCHAR(255)  NULL DEFAULT '/content/images/avatar/avatar.png',
    Color              NVARCHAR(20)   NULL,
    HomePageMenuId     INT            NULL,
    HierarchicalLevel  SMALLINT       NOT NULL DEFAULT 0,
    CreatedById        INT            NOT NULL,
    CreationDate       DATETIME       NOT NULL,
    LastUpdatedById    INT            NOT NULL DEFAULT 0,
    LastUpdateDate     DATETIME       NULL,
    PasswordExpired    BIT            NOT NULL DEFAULT 0,
    _FullName          NVARCHAR(511)  NOT NULL DEFAULT '',
    TzDislpayName      NVARCHAR(512)  NULL,
    Path               NVARCHAR(MAX)  NULL,
    LastAccess         DATETIME       NULL,
    IsDeleted          BIT            NULL DEFAULT 0,
    Token              NVARCHAR(256)  NULL,
    Discriminator      NVARCHAR(50)   NULL,
    RefreshTokenId     NVARCHAR(256)  NULL,
    IsEmailConfirmed   BIT            NULL,
    FailedLoginAttempts INT           NULL,
    LockoutUntil       DATETIME       NULL,
    CONSTRAINT FK_base_Users_Role       FOREIGN KEY (RoleId)           REFERENCES auth.auth_Groups(GroupId),
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
    CanDelete            BIT  NOT NULL DEFAULT 0,
    CanCreate            BIT  NOT NULL DEFAULT 0,
    CanModify            BIT  NOT NULL DEFAULT 0,
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
    AuthorizationId     INT  NOT NULL,
    UserId              INT  NOT NULL,
    CanView             BIT  NOT NULL DEFAULT 0,
    CanDelete           BIT  NOT NULL DEFAULT 0,
    CanCreate           BIT  NOT NULL DEFAULT 0,
    CanModify           BIT  NOT NULL DEFAULT 0,
    CanAction           BIT  NOT NULL DEFAULT 0,
    CONSTRAINT FK_auth_UA_User          FOREIGN KEY (UserId)          REFERENCES auth.base_Users(UserId),
    CONSTRAINT FK_auth_UA_Authorization FOREIGN KEY (AuthorizationId) REFERENCES auth.auth_Authorizations(AuthorizationId)
);
GO

-- ── auth_UsersGroups ──────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'auth_UsersGroups' AND schema_id = SCHEMA_ID('auth'))
CREATE TABLE auth.auth_UsersGroups (
    GroupId INT NOT NULL,
    UserId  INT NOT NULL,
    CONSTRAINT PK_auth_UsersGroups PRIMARY KEY (GroupId, UserId),
    CONSTRAINT FK_auth_UG_Group FOREIGN KEY (GroupId) REFERENCES auth.auth_Groups(GroupId),
    CONSTRAINT FK_auth_UG_User  FOREIGN KEY (UserId)  REFERENCES auth.base_Users(UserId)
);
GO

-- ── auth_GroupsModules ────────────────────────────────────────
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

-- ── vw_DefaultStatusFilters ───────────────────────────────────
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

-- ============================================================
-- MIGRAZIONE DATI da Eagle_Auth
-- Eseguire solo se entrambi i database sono sullo stesso server.
-- Ordine rispetta i vincoli FK.
-- ============================================================

INSERT INTO auth.auth_Modules (ModuleId, Code, Description, IsActive, SortIndex, IsDefault)
SELECT ModuleId, Code, Description, IsActive, SortIndex, IsDefault
FROM Eagle_Auth.dbo.auth_Modules;

INSERT INTO auth.auth_Areas (AreaId, ModuleId, Code, Description, IsActive)
SELECT AreaId, ModuleId, Code, Description, IsActive
FROM Eagle_Auth.dbo.auth_Areas;

INSERT INTO auth.auth_Authorizations (AuthorizationId, Code, Description, AreaId)
SELECT AuthorizationId, Code, Description, AreaId
FROM Eagle_Auth.dbo.auth_Authorizations;

INSERT INTO auth.auth_Groups (
    GroupId, Code, Description, IsActive, Discriminator, IsSystemEntity,
    Weight, IsFieldForceGroup, IsDefault, CompanyType, IsDeleted,
    CompanyId, ParentId, UseInternal)
SELECT
    GroupId, Code, Description, IsActive, Discriminator, IsSystemEntity,
    Weight, IsFieldForceGroup, IsDefault, CompanyType, IsDeleted,
    CompanyId, ParentId, UseInternal
FROM Eagle_Auth.dbo.auth_Groups;

INSERT INTO auth.base_Users (
    UserId, RoleId, SupervisorUserId, Code, Login, Email,
    FirstName, LastName, Culture, IsActive, IsSystemUser,
    Password, AvatarPath, Color, HomePageMenuId, HierarchicalLevel,
    CreatedById, CreationDate, LastUpdatedById, LastUpdateDate,
    PasswordExpired, _FullName, TzDislpayName, Path, LastAccess,
    IsDeleted, Token, Discriminator, RefreshTokenId,
    IsEmailConfirmed, FailedLoginAttempts, LockoutUntil)
SELECT
    UserId, RoleId, SupervisorUserId, Code, Login, Email,
    FirstName, LastName, Culture, IsActive, IsSystemUser,
    Password, AvatarPath, Color, HomePageMenuId, HierarchicalLevel,
    CreatedById, CreationDate, LastUpdatedById, LastUpdateDate,
    PasswordExpired, _FullName, TzDislpayName, Path, LastAccess,
    IsDeleted, Token, Discriminator, RefreshTokenId,
    IsEmailConfirmed, FailedLoginAttempts, LockoutUntil
FROM Eagle_Auth.dbo.base_Users;

INSERT INTO auth.auth_GroupsAuthorizations (
    GroupAuthorizationId, AuthorizationId, GroupId, RoleId,
    CanView, CanDelete, CanCreate, CanModify, CanAction)
SELECT
    GroupAuthorizationId, AuthorizationId, GroupId, RoleId,
    CanView, CanDelete, CanCreate, CanModify, CanAction
FROM Eagle_Auth.dbo.auth_GroupsAuthorizations;

INSERT INTO auth.auth_UsersAuthorizations (
    UserAuthorizationId, AuthorizationId, UserId,
    CanView, CanDelete, CanCreate, CanModify, CanAction)
SELECT
    UserAuthorizationId, AuthorizationId, UserId,
    CanView, CanDelete, CanCreate, CanModify, CanAction
FROM Eagle_Auth.dbo.auth_UsersAuthorizations;

INSERT INTO auth.auth_UsersGroups (GroupId, UserId)
SELECT GroupId, UserId
FROM Eagle_Auth.dbo.auth_UsersGroups;

INSERT INTO auth.auth_GroupsModules (GroupId, ModuleId)
SELECT GroupId, ModuleId
FROM Eagle_Auth.dbo.auth_GroupsModules;
