-- ============================================================
-- Migration: Dynamic File Repository
-- Crea il sistema di repository file dinamico.
-- I file sono identificati da EntityFullName + EntityName + EntityId.
-- Il contenuto puo' essere salvato su DB (base64) o filesystem
-- in base alla configurazione DynamicFileSettings.
-- ============================================================

-- ─── 1. Lookup tipo storage ───────────────────────────────────────────────
CREATE TABLE DynamicFileStorageTypeLookup (
    Id   INT          NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_DynamicFileStorageTypeLookup PRIMARY KEY (Id)
);

INSERT INTO DynamicFileStorageTypeLookup (Id, Name) VALUES
    (0, 'Database'),
    (1, 'FileSystem'),
    (2, 'AzureBlob');

-- ─── 2. Metadati file ─────────────────────────────────────────────────────
CREATE TABLE DynamicFile (
    Id                   INT              NOT NULL,
    TenantId             UNIQUEIDENTIFIER NOT NULL,

    -- Identificatori entita' collegata
    EntityFullName       NVARCHAR(500)    NOT NULL,  -- es. "DomuWave.Services.Models.Expense"
    EntityName           NVARCHAR(200)    NOT NULL,  -- es. "Expense"
    EntityId             INT              NOT NULL,  -- PK dell'entita' collegata

    -- Attributi file
    FileName             NVARCHAR(500)    NOT NULL,
    FileExtension        NVARCHAR(20)     NULL,      -- es. ".pdf"
    ContentType          NVARCHAR(200)    NOT NULL,  -- MIME type
    FileSize             BIGINT           NOT NULL DEFAULT 0,
    Description          NVARCHAR(1000)   NULL,
    Tags                 NVARCHAR(500)    NULL,      -- tag separati da virgola

    -- Storage
    StorageTypeId        INT              NOT NULL DEFAULT 0,
    StorageReference     NVARCHAR(2000)   NULL,      -- path o URL per storage non-DB

    -- Trace
    CreatedById          INT              NOT NULL,
    CreatedByFullName    NVARCHAR(200)    NULL,
    LastUpdatedById      INT              NULL,
    LastUpdatedByFullName NVARCHAR(200)   NULL,
    IsDeleted            BIT              NOT NULL DEFAULT 0,
    CreationDate         DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    LastUpdateDate       DATETIME2        NULL,

    CONSTRAINT PK_DynamicFile            PRIMARY KEY (Id),
    CONSTRAINT FK_DynamicFile_StorageType FOREIGN KEY (StorageTypeId)
        REFERENCES DynamicFileStorageTypeLookup(Id)
);

CREATE INDEX IX_DynamicFile_TenantId        ON DynamicFile (TenantId);
CREATE INDEX IX_DynamicFile_EntityName      ON DynamicFile (EntityName) WHERE IsDeleted = 0;
CREATE INDEX IX_DynamicFile_EntityFullName  ON DynamicFile (EntityFullName) WHERE IsDeleted = 0;
CREATE INDEX IX_DynamicFile_EntityId        ON DynamicFile (EntityId) WHERE IsDeleted = 0;
CREATE INDEX IX_DynamicFile_EntityLookup    ON DynamicFile (EntityName, EntityId) WHERE IsDeleted = 0;

-- ─── 3. Contenuto file (solo per storage Database) ────────────────────────
CREATE TABLE DynamicFileContent (
    Id                   INT              NOT NULL,
    FileId               INT              NOT NULL,
    Base64Data           NVARCHAR(MAX)    NOT NULL,

    -- Trace
    CreatedById          INT              NOT NULL,
    CreatedByFullName    NVARCHAR(200)    NULL,
    LastUpdatedById      INT              NULL,
    LastUpdatedByFullName NVARCHAR(200)   NULL,
    IsDeleted            BIT              NOT NULL DEFAULT 0,
    CreationDate         DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    LastUpdateDate       DATETIME2        NULL,

    CONSTRAINT PK_DynamicFileContent             PRIMARY KEY (Id),
    CONSTRAINT FK_DynamicFileContent_DynamicFile FOREIGN KEY (FileId)
        REFERENCES DynamicFile(Id),
    CONSTRAINT UQ_DynamicFileContent_FileId      UNIQUE (FileId)
);

-- ─── 4. Hilo sequence entries ─────────────────────────────────────────────
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'DynamicFile', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'DynamicFile');

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'DynamicFileContent', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'DynamicFileContent');
