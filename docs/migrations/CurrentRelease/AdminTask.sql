-- ============================================================================
-- AdminTask — Attività pianificabili dall'amministratore
-- Task con priorità, stato, scadenza, assegnatario (collaboratore) e
-- collegamento opzionale a 0/1/N condomìni (tabella AdminTaskCondominium).
-- Idempotente.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ── Lookup priorità ──────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AdminTaskPriorityLookup')
BEGIN
    CREATE TABLE AdminTaskPriorityLookup (
        Id   INT          NOT NULL PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL
    );
END
GO
INSERT INTO AdminTaskPriorityLookup (Id, Name)
SELECT v.Id, v.Name
FROM (VALUES (1,'Bassa'),(2,'Media'),(3,'Alta'),(4,'Urgente')) AS v(Id, Name)
WHERE NOT EXISTS (SELECT 1 FROM AdminTaskPriorityLookup t WHERE t.Id = v.Id);
GO

-- ── Lookup stato ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AdminTaskStatusLookup')
BEGIN
    CREATE TABLE AdminTaskStatusLookup (
        Id   INT          NOT NULL PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL
    );
END
GO
INSERT INTO AdminTaskStatusLookup (Id, Name)
SELECT v.Id, v.Name
FROM (VALUES (1,'Da fare'),(2,'In corso'),(3,'Completata'),(4,'Annullata')) AS v(Id, Name)
WHERE NOT EXISTS (SELECT 1 FROM AdminTaskStatusLookup t WHERE t.Id = v.Id);
GO

-- ── Tabella AdminTask ────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AdminTask')
BEGIN
    CREATE TABLE AdminTask (
        Id                    INT              NOT NULL PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        Title                 NVARCHAR(200)    NOT NULL,
        Description           NVARCHAR(2000)   NULL,
        PriorityId            INT              NOT NULL DEFAULT 2,
        StatusId              INT              NOT NULL DEFAULT 1,
        DueDate               DATETIME2        NULL,
        AssignedToUserId      INT              NULL,
        AssignedToFullName    NVARCHAR(200)    NULL,

        CreatedById           INT              NOT NULL,
        CreatedByFullName     NVARCHAR(200)    NULL,
        LastUpdatedById       INT              NULL,
        LastUpdatedByFullName NVARCHAR(200)    NULL,
        IsDeleted             BIT              NOT NULL DEFAULT 0,
        CreationDate          DATETIME2        NOT NULL,
        LastUpdateDate        DATETIME2        NULL,

        CONSTRAINT FK_AdminTask_Tenant   FOREIGN KEY (TenantId)   REFERENCES Tenant(Id),
        CONSTRAINT FK_AdminTask_Priority FOREIGN KEY (PriorityId) REFERENCES AdminTaskPriorityLookup(Id),
        CONSTRAINT FK_AdminTask_Status   FOREIGN KEY (StatusId)   REFERENCES AdminTaskStatusLookup(Id)
    );

    CREATE INDEX IX_AdminTask_TenantId ON AdminTask (TenantId) WHERE IsDeleted = 0;
END
GO

-- ── Tabella di collegamento AdminTaskCondominium ─────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AdminTaskCondominium')
BEGIN
    CREATE TABLE AdminTaskCondominium (
        Id                    INT              NOT NULL PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        TaskId                INT              NOT NULL,
        CondominiumId         INT              NOT NULL,

        CreatedById           INT              NOT NULL,
        CreatedByFullName     NVARCHAR(200)    NULL,
        LastUpdatedById       INT              NULL,
        LastUpdatedByFullName NVARCHAR(200)    NULL,
        IsDeleted             BIT              NOT NULL DEFAULT 0,
        CreationDate          DATETIME2        NOT NULL,
        LastUpdateDate        DATETIME2        NULL,

        CONSTRAINT FK_AdminTaskCondominium_Task        FOREIGN KEY (TaskId)        REFERENCES AdminTask(Id),
        CONSTRAINT FK_AdminTaskCondominium_Condominium FOREIGN KEY (CondominiumId) REFERENCES Condominium(Id)
    );

    CREATE INDEX IX_AdminTaskCondominium_TaskId ON AdminTaskCondominium (TaskId) WHERE IsDeleted = 0;
END
GO

-- ── Sequenze hilo ────────────────────────────────────────────────────────────
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'AdminTask', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'AdminTask');
GO
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'AdminTaskCondominium', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'AdminTaskCondominium');
GO
