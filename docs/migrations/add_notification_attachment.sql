-- Migration: NotificationAttachment table
-- Links a CommunicationNotification to a Document as an attachment

CREATE TABLE NotificationAttachment (
    Id                      INT             NOT NULL PRIMARY KEY,
    TenantId                UNIQUEIDENTIFIER NOT NULL,
    NotificationId          BIGINT          NOT NULL,
    DocumentId              INT             NOT NULL,
    CreatedById             INT             NOT NULL,
    CreatedByFullName       NVARCHAR(200)   NULL,
    LastUpdatedById         INT             NULL,
    LastUpdatedByFullName   NVARCHAR(200)   NULL,
    IsDeleted               BIT             NOT NULL DEFAULT 0,
    CreationDate            DATETIME2       NOT NULL,
    LastUpdateDate          DATETIME2       NULL,
    CONSTRAINT FK_NotificationAttachment_Notification FOREIGN KEY (NotificationId) REFERENCES CommunicationNotification(Id),
    CONSTRAINT FK_NotificationAttachment_Document FOREIGN KEY (DocumentId) REFERENCES Document(Id),
    CONSTRAINT UQ_NotificationAttachment_NotifDoc UNIQUE (NotificationId, DocumentId)
);

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'NotificationAttachment', 1
WHERE NOT EXISTS (
    SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'NotificationAttachment'
);
