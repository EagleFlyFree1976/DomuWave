-- ============================================================
-- Sistema generico di allegati file
-- ============================================================

-- Tabella 1: Metadati del file (EntityKey + EntityId + info file)
CREATE TABLE [dbo].[FileAttachment] (
    [FileAttachmentId]      INT                IDENTITY(1,1) NOT NULL,
    [TenantId]              UNIQUEIDENTIFIER   NOT NULL,
    [EntityKey]             NVARCHAR(100)      NOT NULL,
    [EntityId]              INT                NOT NULL,
    [FileName]              NVARCHAR(500)      NOT NULL,
    [ContentType]           NVARCHAR(200)      NOT NULL,
    [FileSize]              BIGINT             NOT NULL,

    [CreatedById]           BIGINT             NOT NULL,
    [CreatedByFullName]     NVARCHAR(200)      NULL,
    [CreationDate]          DATETIME2(3)       NOT NULL,
    [LastUpdatedById]       BIGINT             NULL,
    [LastUpdatedByFullName] NVARCHAR(200)      NULL,
    [LastUpdateDate]        DATETIME2(3)       NULL,
    [IsDeleted]             BIT                NOT NULL DEFAULT 0,

    CONSTRAINT [PK_FileAttachment] PRIMARY KEY CLUSTERED ([FileAttachmentId] ASC)
);

CREATE INDEX [IX_FileAttachment_Entity] ON [dbo].[FileAttachment] ([EntityKey], [EntityId]);
CREATE INDEX [IX_FileAttachment_Tenant] ON [dbo].[FileAttachment] ([TenantId]);

-- ----------------------------------------------------------------

-- Tabella 2: Contenuto base64 del file (separato per non appesantire le query di lista)
CREATE TABLE [dbo].[FileAttachmentContent] (
    [FileAttachmentContentId] INT              IDENTITY(1,1) NOT NULL,
    [FileAttachmentId]        INT              NOT NULL,
    [Base64Data]              NVARCHAR(MAX)    NOT NULL,

    [CreatedById]           BIGINT             NOT NULL,
    [CreatedByFullName]     NVARCHAR(200)      NULL,
    [CreationDate]          DATETIME2(3)       NOT NULL,
    [LastUpdatedById]       BIGINT             NULL,
    [LastUpdatedByFullName] NVARCHAR(200)      NULL,
    [LastUpdateDate]        DATETIME2(3)       NULL,
    [IsDeleted]             BIT                NOT NULL DEFAULT 0,

    CONSTRAINT [PK_FileAttachmentContent] PRIMARY KEY CLUSTERED ([FileAttachmentContentId] ASC),
    CONSTRAINT [FK_FileAttachmentContent_FileAttachment] FOREIGN KEY ([FileAttachmentId])
        REFERENCES [dbo].[FileAttachment] ([FileAttachmentId]) ON DELETE CASCADE
);
