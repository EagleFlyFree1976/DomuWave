-- ============================================================
-- Assemblee condominiali
-- ============================================================

-- ── Lookup: tipo assemblea ────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AssemblyTypeLookup')
BEGIN
    CREATE TABLE AssemblyTypeLookup (
        Id   INT          NOT NULL PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL
    );
END
IF NOT EXISTS (SELECT 1 FROM AssemblyTypeLookup WHERE Id = 1) INSERT INTO AssemblyTypeLookup (Id, Name) VALUES (1, 'Ordinaria');
IF NOT EXISTS (SELECT 1 FROM AssemblyTypeLookup WHERE Id = 2) INSERT INTO AssemblyTypeLookup (Id, Name) VALUES (2, 'Straordinaria');

-- ── Lookup: stato assemblea ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AssemblyStatusLookup')
BEGIN
    CREATE TABLE AssemblyStatusLookup (
        Id   INT          NOT NULL PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL
    );
END
IF NOT EXISTS (SELECT 1 FROM AssemblyStatusLookup WHERE Id = 1) INSERT INTO AssemblyStatusLookup (Id, Name) VALUES (1, 'Bozza');
IF NOT EXISTS (SELECT 1 FROM AssemblyStatusLookup WHERE Id = 2) INSERT INTO AssemblyStatusLookup (Id, Name) VALUES (2, 'Pianificata');
IF NOT EXISTS (SELECT 1 FROM AssemblyStatusLookup WHERE Id = 3) INSERT INTO AssemblyStatusLookup (Id, Name) VALUES (3, 'Convocata');
IF NOT EXISTS (SELECT 1 FROM AssemblyStatusLookup WHERE Id = 4) INSERT INTO AssemblyStatusLookup (Id, Name) VALUES (4, 'Svolta');
IF NOT EXISTS (SELECT 1 FROM AssemblyStatusLookup WHERE Id = 5) INSERT INTO AssemblyStatusLookup (Id, Name) VALUES (5, 'Annullata');

-- ── Lookup: esito votazione punto OdG ────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AgendaItemVoteResultLookup')
BEGIN
    CREATE TABLE AgendaItemVoteResultLookup (
        Id   INT          NOT NULL PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL
    );
END
IF NOT EXISTS (SELECT 1 FROM AgendaItemVoteResultLookup WHERE Id = 1) INSERT INTO AgendaItemVoteResultLookup (Id, Name) VALUES (1, 'Non Votato');
IF NOT EXISTS (SELECT 1 FROM AgendaItemVoteResultLookup WHERE Id = 2) INSERT INTO AgendaItemVoteResultLookup (Id, Name) VALUES (2, 'Approvato');
IF NOT EXISTS (SELECT 1 FROM AgendaItemVoteResultLookup WHERE Id = 3) INSERT INTO AgendaItemVoteResultLookup (Id, Name) VALUES (3, 'Respinto');
IF NOT EXISTS (SELECT 1 FROM AgendaItemVoteResultLookup WHERE Id = 4) INSERT INTO AgendaItemVoteResultLookup (Id, Name) VALUES (4, 'Rinviato');

-- ── Lookup: tipo presenza ────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AttendanceTypeLookup')
BEGIN
    CREATE TABLE AttendanceTypeLookup (
        Id   INT          NOT NULL PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL
    );
END
IF NOT EXISTS (SELECT 1 FROM AttendanceTypeLookup WHERE Id = 1) INSERT INTO AttendanceTypeLookup (Id, Name) VALUES (1, 'Presente');
IF NOT EXISTS (SELECT 1 FROM AttendanceTypeLookup WHERE Id = 2) INSERT INTO AttendanceTypeLookup (Id, Name) VALUES (2, 'Delegato');
IF NOT EXISTS (SELECT 1 FROM AttendanceTypeLookup WHERE Id = 3) INSERT INTO AttendanceTypeLookup (Id, Name) VALUES (3, 'Assente');

-- ── Tabella principale: Assembly ──────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Assembly')
BEGIN
    CREATE TABLE Assembly (
        Id                    INT              NOT NULL PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        CondominiumId         INT              NOT NULL,
        FiscalYearId          INT              NULL,
        Title                 NVARCHAR(200)    NOT NULL,
        AssemblyTypeId        INT              NOT NULL DEFAULT 1,
        StatusId              INT              NOT NULL DEFAULT 1,
        ScheduledDate         DATETIME2        NOT NULL,
        ActualDate            DATETIME2        NULL,
        Location              NVARCHAR(200)    NULL,
        Description           NVARCHAR(500)    NULL,
        Notes                 NVARCHAR(MAX)    NULL,
        MinutesPath           NVARCHAR(500)    NULL,
        CreatedById           INT              NOT NULL,
        CreatedByFullName     NVARCHAR(200)    NULL,
        LastUpdatedById       INT              NULL,
        LastUpdatedByFullName NVARCHAR(200)    NULL,
        IsDeleted             BIT              NOT NULL DEFAULT 0,
        CreationDate          DATETIME2        NOT NULL,
        LastUpdateDate        DATETIME2        NULL,

        CONSTRAINT FK_Assembly_Tenant        FOREIGN KEY (TenantId)       REFERENCES Tenant(Id),
        CONSTRAINT FK_Assembly_Condominium   FOREIGN KEY (CondominiumId)  REFERENCES Condominium(Id),
        CONSTRAINT FK_Assembly_FiscalYear    FOREIGN KEY (FiscalYearId)   REFERENCES FiscalYear(Id),
        CONSTRAINT FK_Assembly_Type          FOREIGN KEY (AssemblyTypeId) REFERENCES AssemblyTypeLookup(Id),
        CONSTRAINT FK_Assembly_Status        FOREIGN KEY (StatusId)       REFERENCES AssemblyStatusLookup(Id)
    );
END

-- ── Tabella: AssemblyAgendaItem (punti OdG) ───────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AssemblyAgendaItem')
BEGIN
    CREATE TABLE AssemblyAgendaItem (
        Id                    INT              NOT NULL PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        AssemblyId            INT              NOT NULL,
        OrderIndex            INT              NOT NULL DEFAULT 0,
        Title                 NVARCHAR(200)    NOT NULL,
        Description           NVARCHAR(MAX)    NULL,
        Resolution            NVARCHAR(MAX)    NULL,
        VoteResultId          INT              NOT NULL DEFAULT 1,
        CreatedById           INT              NOT NULL,
        CreatedByFullName     NVARCHAR(200)    NULL,
        LastUpdatedById       INT              NULL,
        LastUpdatedByFullName NVARCHAR(200)    NULL,
        IsDeleted             BIT              NOT NULL DEFAULT 0,
        CreationDate          DATETIME2        NOT NULL,
        LastUpdateDate        DATETIME2        NULL,

        CONSTRAINT FK_AgendaItem_Tenant      FOREIGN KEY (TenantId)     REFERENCES Tenant(Id),
        CONSTRAINT FK_AgendaItem_Assembly    FOREIGN KEY (AssemblyId)   REFERENCES Assembly(Id),
        CONSTRAINT FK_AgendaItem_VoteResult  FOREIGN KEY (VoteResultId) REFERENCES AgendaItemVoteResultLookup(Id)
    );
END

-- ── Tabella: AssemblyAttendance (presenze) ────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AssemblyAttendance')
BEGIN
    CREATE TABLE AssemblyAttendance (
        Id                    INT              NOT NULL PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        AssemblyId            INT              NOT NULL,
        UnitOwnerId           INT              NOT NULL,
        AttendanceTypeId      INT              NOT NULL DEFAULT 1,
        DelegateName          NVARCHAR(200)    NULL,
        MillesimalValue       DECIMAL(18,4)    NOT NULL DEFAULT 0,
        Notes                 NVARCHAR(500)    NULL,
        CreatedById           INT              NOT NULL,
        CreatedByFullName     NVARCHAR(200)    NULL,
        LastUpdatedById       INT              NULL,
        LastUpdatedByFullName NVARCHAR(200)    NULL,
        IsDeleted             BIT              NOT NULL DEFAULT 0,
        CreationDate          DATETIME2        NOT NULL,
        LastUpdateDate        DATETIME2        NULL,

        CONSTRAINT FK_Attendance_Tenant         FOREIGN KEY (TenantId)         REFERENCES Tenant(Id),
        CONSTRAINT FK_Attendance_Assembly       FOREIGN KEY (AssemblyId)       REFERENCES Assembly(Id),
        CONSTRAINT FK_Attendance_UnitOwner      FOREIGN KEY (UnitOwnerId)      REFERENCES UnitOwner(Id),
        CONSTRAINT FK_Attendance_AttendanceType FOREIGN KEY (AttendanceTypeId) REFERENCES AttendanceTypeLookup(Id),

        CONSTRAINT UQ_Attendance_Assembly_Owner UNIQUE (AssemblyId, UnitOwnerId)
    );
END

-- ── Hilo entries ──────────────────────────────────────────────
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'Assembly', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'Assembly');

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'AssemblyAgendaItem', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'AssemblyAgendaItem');

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'AssemblyAttendance', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'AssemblyAttendance');
