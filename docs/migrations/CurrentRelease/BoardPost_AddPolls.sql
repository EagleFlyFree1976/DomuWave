-- ============================================================================
-- Sondaggi in bacheca: estensione di BoardPost + opzioni + voti.
--   BoardPost: IsPoll / IsAnonymous / AllowMultiple / ClosesAt
--   BoardPostOption: opzioni di risposta del sondaggio
--   BoardPostVote: voti dei condòmini (1 per opzione per utente; cambio voto via soft-delete)
-- Idempotente.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ── 1. Colonne sondaggio su BoardPost ───────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('BoardPost') AND name = 'IsPoll')
    ALTER TABLE BoardPost ADD IsPoll BIT NOT NULL CONSTRAINT DF_BoardPost_IsPoll DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('BoardPost') AND name = 'IsAnonymous')
    ALTER TABLE BoardPost ADD IsAnonymous BIT NOT NULL CONSTRAINT DF_BoardPost_IsAnonymous DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('BoardPost') AND name = 'AllowMultiple')
    ALTER TABLE BoardPost ADD AllowMultiple BIT NOT NULL CONSTRAINT DF_BoardPost_AllowMultiple DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('BoardPost') AND name = 'ClosesAt')
    ALTER TABLE BoardPost ADD ClosesAt DATETIME2 NULL;
GO

-- ── 2. Opzioni di risposta ──────────────────────────────────────────────────
IF OBJECT_ID('BoardPostOption') IS NULL
BEGIN
    CREATE TABLE BoardPostOption (
        Id                    INT              NOT NULL PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        BoardPostId           INT              NOT NULL,
        Text                  NVARCHAR(300)    NOT NULL,
        OrderKey              INT              NOT NULL DEFAULT 0,
        CreatedById           INT              NOT NULL,
        CreatedByFullName     NVARCHAR(200)    NULL,
        LastUpdatedById       INT              NULL,
        LastUpdatedByFullName NVARCHAR(200)    NULL,
        IsDeleted             BIT              NOT NULL DEFAULT 0,
        CreationDate          DATETIME2        NOT NULL,
        LastUpdateDate        DATETIME2        NULL,
        CONSTRAINT FK_BoardPostOption_BoardPost FOREIGN KEY (BoardPostId) REFERENCES BoardPost(Id)
    );
END
GO

-- ── 3. Voti ─────────────────────────────────────────────────────────────────
IF OBJECT_ID('BoardPostVote') IS NULL
BEGIN
    CREATE TABLE BoardPostVote (
        Id                    INT              NOT NULL PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        BoardPostId           INT              NOT NULL,
        BoardPostOptionId     INT              NOT NULL,
        VoterUserId           BIGINT           NOT NULL,
        VoterFullName         NVARCHAR(200)    NOT NULL,
        CreatedById           INT              NOT NULL,
        CreatedByFullName     NVARCHAR(200)    NULL,
        LastUpdatedById       INT              NULL,
        LastUpdatedByFullName NVARCHAR(200)    NULL,
        IsDeleted             BIT              NOT NULL DEFAULT 0,
        CreationDate          DATETIME2        NOT NULL,
        LastUpdateDate        DATETIME2        NULL,
        CONSTRAINT FK_BoardPostVote_BoardPost       FOREIGN KEY (BoardPostId)       REFERENCES BoardPost(Id),
        CONSTRAINT FK_BoardPostVote_BoardPostOption FOREIGN KEY (BoardPostOptionId) REFERENCES BoardPostOption(Id)
    );
END
GO

-- Anti doppio-voto: un utente può votare un'opzione una sola volta (tra i voti attivi).
-- Il filtro IsDeleted=0 consente il cambio voto (soft-delete del vecchio + insert del nuovo).
-- NB: il vincolo è per OPZIONE (non per post), così la scelta multipla resta possibile.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_BoardPostVote_Option_Voter' AND object_id = OBJECT_ID('BoardPostVote'))
    CREATE UNIQUE INDEX UQ_BoardPostVote_Option_Voter
        ON BoardPostVote (BoardPostOptionId, VoterUserId)
        WHERE IsDeleted = 0;
GO

-- ── 4. Hilo sequences ───────────────────────────────────────────────────────
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'BoardPostOption', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'BoardPostOption');

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'BoardPostVote', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'BoardPostVote');
GO
