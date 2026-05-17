-- Migration: add DocumentContent table for binary file storage
-- Documents metadata stays in Document table; binary content in DocumentContent (1:1)

CREATE TABLE DocumentContent (
    Id                      INT              NOT NULL PRIMARY KEY,
    TenantId                UNIQUEIDENTIFIER NOT NULL,
    DocumentId              INT              NOT NULL,
    FileContent             VARBINARY(MAX)   NOT NULL,
    CreatedById             INT              NOT NULL,
    CreatedByFullName       NVARCHAR(200)    NULL,
    LastUpdatedById         INT              NULL,
    LastUpdatedByFullName   NVARCHAR(200)    NULL,
    IsDeleted               BIT              NOT NULL DEFAULT 0,
    CreationDate            DATETIME2        NOT NULL,
    LastUpdateDate          DATETIME2        NULL,

    CONSTRAINT FK_DocumentContent_Document FOREIGN KEY (DocumentId) REFERENCES Document(Id),
    CONSTRAINT UQ_DocumentContent_DocumentId UNIQUE (DocumentId)
);

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'DocumentContent', 1
WHERE NOT EXISTS (
    SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'DocumentContent'
);
