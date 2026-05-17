-- ============================================================
-- Centro Comunicazioni Condomini
-- BoardPost, BoardPostComment, Fault, FaultMessage,
-- PrivateThread, PrivateMessage
-- ============================================================

-- ── 1. BACHECA ───────────────────────────────────────────────

CREATE TABLE BoardPost (
    Id                    INT            NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    CondominiumId         INT            NOT NULL,
    AuthorUserId          BIGINT         NOT NULL,
    AuthorFullName        NVARCHAR(200)  NOT NULL,
    Title                 NVARCHAR(300)  NOT NULL,
    Body                  NVARCHAR(MAX)  NOT NULL,
    IsPinned              BIT            NOT NULL DEFAULT 0,
    CreatedById           INT            NOT NULL,
    CreatedByFullName     NVARCHAR(200)  NULL,
    LastUpdatedById       INT            NULL,
    LastUpdatedByFullName NVARCHAR(200)  NULL,
    IsDeleted             BIT            NOT NULL DEFAULT 0,
    CreationDate          DATETIME2      NOT NULL,
    LastUpdateDate        DATETIME2      NULL,
    CONSTRAINT FK_BoardPost_Condominium FOREIGN KEY (CondominiumId) REFERENCES Condominium(Id)
);

CREATE TABLE BoardPostComment (
    Id                    INT            NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    BoardPostId           INT            NOT NULL,
    AuthorUserId          BIGINT         NOT NULL,
    AuthorFullName        NVARCHAR(200)  NOT NULL,
    Body                  NVARCHAR(MAX)  NOT NULL,
    CreatedById           INT            NOT NULL,
    CreatedByFullName     NVARCHAR(200)  NULL,
    LastUpdatedById       INT            NULL,
    LastUpdatedByFullName NVARCHAR(200)  NULL,
    IsDeleted             BIT            NOT NULL DEFAULT 0,
    CreationDate          DATETIME2      NOT NULL,
    LastUpdateDate        DATETIME2      NULL,
    CONSTRAINT FK_BoardPostComment_BoardPost FOREIGN KEY (BoardPostId) REFERENCES BoardPost(Id)
);

-- ── 2. SEGNALAZIONI GUASTI ───────────────────────────────────

-- Status: 0=Aperta, 1=InLavorazione, 2=Risolta
CREATE TABLE FaultStatusLookup (
    Id   INT          NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);
INSERT INTO FaultStatusLookup (Id, Name) VALUES
    (0, 'Aperta'),
    (1, 'In lavorazione'),
    (2, 'Risolta');

CREATE TABLE Fault (
    Id                    INT            NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    CondominiumId         INT            NOT NULL,
    UnitId                INT            NULL,
    ReporterUserId        BIGINT         NOT NULL,
    ReporterFullName      NVARCHAR(200)  NOT NULL,
    Title                 NVARCHAR(300)  NOT NULL,
    Description           NVARCHAR(MAX)  NOT NULL,
    StatusId              INT            NOT NULL DEFAULT 0,
    CreatedById           INT            NOT NULL,
    CreatedByFullName     NVARCHAR(200)  NULL,
    LastUpdatedById       INT            NULL,
    LastUpdatedByFullName NVARCHAR(200)  NULL,
    IsDeleted             BIT            NOT NULL DEFAULT 0,
    CreationDate          DATETIME2      NOT NULL,
    LastUpdateDate        DATETIME2      NULL,
    CONSTRAINT FK_Fault_Condominium      FOREIGN KEY (CondominiumId) REFERENCES Condominium(Id),
    CONSTRAINT FK_Fault_RealEstateUnit   FOREIGN KEY (UnitId)        REFERENCES RealEstateUnit(Id),
    CONSTRAINT FK_Fault_FaultStatusLookup FOREIGN KEY (StatusId)     REFERENCES FaultStatusLookup(Id)
);

CREATE TABLE FaultMessage (
    Id                    INT            NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    FaultId               INT            NOT NULL,
    AuthorUserId          BIGINT         NOT NULL,
    AuthorFullName        NVARCHAR(200)  NOT NULL,
    Body                  NVARCHAR(MAX)  NOT NULL,
    CreatedById           INT            NOT NULL,
    CreatedByFullName     NVARCHAR(200)  NULL,
    LastUpdatedById       INT            NULL,
    LastUpdatedByFullName NVARCHAR(200)  NULL,
    IsDeleted             BIT            NOT NULL DEFAULT 0,
    CreationDate          DATETIME2      NOT NULL,
    LastUpdateDate        DATETIME2      NULL,
    CONSTRAINT FK_FaultMessage_Fault FOREIGN KEY (FaultId) REFERENCES Fault(Id)
);

-- ── 3. MESSAGGI PRIVATI ──────────────────────────────────────

CREATE TABLE PrivateThread (
    Id                    INT            NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    CondominiumId         INT            NOT NULL,
    CondominioUserId      BIGINT         NOT NULL,
    CondominioUserFullName NVARCHAR(200) NOT NULL,
    CreatedById           INT            NOT NULL,
    CreatedByFullName     NVARCHAR(200)  NULL,
    LastUpdatedById       INT            NULL,
    LastUpdatedByFullName NVARCHAR(200)  NULL,
    IsDeleted             BIT            NOT NULL DEFAULT 0,
    CreationDate          DATETIME2      NOT NULL,
    LastUpdateDate        DATETIME2      NULL,
    CONSTRAINT FK_PrivateThread_Condominium FOREIGN KEY (CondominiumId) REFERENCES Condominium(Id)
);

CREATE TABLE PrivateMessage (
    Id                    INT            NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    PrivateThreadId       INT            NOT NULL,
    SenderUserId          BIGINT         NOT NULL,
    SenderFullName        NVARCHAR(200)  NOT NULL,
    Body                  NVARCHAR(MAX)  NOT NULL,
    IsReadByRecipient     BIT            NOT NULL DEFAULT 0,
    CreatedById           INT            NOT NULL,
    CreatedByFullName     NVARCHAR(200)  NULL,
    LastUpdatedById       INT            NULL,
    LastUpdatedByFullName NVARCHAR(200)  NULL,
    IsDeleted             BIT            NOT NULL DEFAULT 0,
    CreationDate          DATETIME2      NOT NULL,
    LastUpdateDate        DATETIME2      NULL,
    CONSTRAINT FK_PrivateMessage_PrivateThread FOREIGN KEY (PrivateThreadId) REFERENCES PrivateThread(Id)
);

-- ── 4. HILO SEQUENCES ────────────────────────────────────────

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'BoardPost', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'BoardPost');

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'BoardPostComment', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'BoardPostComment');

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'Fault', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'Fault');

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'FaultMessage', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'FaultMessage');

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'PrivateThread', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'PrivateThread');

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'PrivateMessage', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'PrivateMessage');
