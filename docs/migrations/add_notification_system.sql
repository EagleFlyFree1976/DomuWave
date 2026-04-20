-- Migration: sistema notifiche generiche (template + SMTP + CommunicationNotification)
-- Data: 2026-04-20

-- ── 1. NotificationTemplate ───────────────────────────────────────────────────

CREATE TABLE NotificationTemplate (
    Id                    INT             NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    CondominiumId         INT             NOT NULL,
    Name                  NVARCHAR(200)   NOT NULL,
    Description           NVARCHAR(MAX)   NULL,
    CommunicationType     NVARCHAR(50)    NOT NULL,
    SubjectTemplate       NVARCHAR(500)   NOT NULL,
    BodyTemplate          NVARCHAR(MAX)   NOT NULL,
    IsDefault             BIT             NOT NULL DEFAULT 0,
    CreatedById           INT             NOT NULL,
    CreatedByFullName     NVARCHAR(200)   NULL,
    LastUpdatedById       INT             NULL,
    LastUpdatedByFullName NVARCHAR(200)   NULL,
    IsDeleted             BIT             NOT NULL DEFAULT 0,
    CreationDate          DATETIME2       NOT NULL,
    LastUpdateDate        DATETIME2       NULL,

    CONSTRAINT FK_NotificationTemplate_Condominium FOREIGN KEY (CondominiumId) REFERENCES Condominium(Id)
);

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'NotificationTemplate', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'NotificationTemplate');

-- ── 2. TenantSmtpSettings ─────────────────────────────────────────────────────

CREATE TABLE TenantSmtpSettings (
    Id                    INT             NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    Name                  NVARCHAR(200)   NOT NULL DEFAULT 'SMTP',
    Description           NVARCHAR(MAX)   NULL,
    Host                  NVARCHAR(255)   NOT NULL,
    Port                  INT             NOT NULL DEFAULT 587,
    UseSsl                BIT             NOT NULL DEFAULT 1,
    Username              NVARCHAR(255)   NOT NULL,
    PasswordEncrypted     NVARCHAR(500)   NOT NULL,
    FromEmail             NVARCHAR(255)   NOT NULL,
    FromName              NVARCHAR(200)   NOT NULL,
    IsEnabled             BIT             NOT NULL DEFAULT 1,
    CreatedById           INT             NOT NULL,
    CreatedByFullName     NVARCHAR(200)   NULL,
    LastUpdatedById       INT             NULL,
    LastUpdatedByFullName NVARCHAR(200)   NULL,
    IsDeleted             BIT             NOT NULL DEFAULT 0,
    CreationDate          DATETIME2       NOT NULL,
    LastUpdateDate        DATETIME2       NULL,

    CONSTRAINT UQ_TenantSmtpSettings_Tenant UNIQUE (TenantId)
);

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'TenantSmtpSettings', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'TenantSmtpSettings');

-- ── 3. NotificationDeliveryMethodLookup ──────────────────────────────────────

CREATE TABLE NotificationDeliveryMethodLookup (
    Id   INT          NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);
INSERT INTO NotificationDeliveryMethodLookup (Id, Name) VALUES
    (0, 'Email'),
    (1, 'Raccomandata');

-- ── 4. NotificationStatusLookup ──────────────────────────────────────────────

CREATE TABLE NotificationStatusLookup (
    Id   INT          NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);
INSERT INTO NotificationStatusLookup (Id, Name) VALUES
    (0, 'Draft'),
    (1, 'Scheduled'),
    (2, 'Sent'),
    (3, 'Delivered'),
    (4, 'Failed'),
    (5, 'Printed');

-- ── 5. CommunicationNotification ─────────────────────────────────────────────

CREATE TABLE CommunicationNotification (
    Id                    BIGINT          NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    CommunicationId       INT             NOT NULL,
    Name                  NVARCHAR(200)   NOT NULL DEFAULT '',
    Description           NVARCHAR(MAX)   NULL,
    RecipientUserId       BIGINT          NOT NULL,
    RecipientFullName     NVARCHAR(200)   NULL,
    UnitId                INT             NULL,
    DeliveryMethod        INT             NOT NULL DEFAULT 0,
    Status                INT             NOT NULL DEFAULT 0,
    ScheduledAt           DATETIME2       NULL,
    SentAt                DATETIME2       NULL,
    DeliveredAt           DATETIME2       NULL,
    PrintedAt             DATETIME2       NULL,
    TrackingNumber        NVARCHAR(100)   NULL,
    ErrorMessage          NVARCHAR(500)   NULL,
    EmailAddress          NVARCHAR(255)   NULL,
    PostalAddress         NVARCHAR(500)   NULL,
    SubjectResolved       NVARCHAR(500)   NULL,
    BodyResolved          NVARCHAR(MAX)   NULL,
    CreatedById           INT             NOT NULL,
    CreatedByFullName     NVARCHAR(200)   NULL,
    LastUpdatedById       INT             NULL,
    LastUpdatedByFullName NVARCHAR(200)   NULL,
    IsDeleted             BIT             NOT NULL DEFAULT 0,
    CreationDate          DATETIME2       NOT NULL,
    LastUpdateDate        DATETIME2       NULL,

    CONSTRAINT FK_CommunicationNotification_Communication FOREIGN KEY (CommunicationId) REFERENCES Communication(Id),
    CONSTRAINT FK_CommunicationNotification_Unit          FOREIGN KEY (UnitId)          REFERENCES RealEstateUnit(Id),
    CONSTRAINT FK_CommunicationNotification_DeliveryMethod FOREIGN KEY (DeliveryMethod) REFERENCES NotificationDeliveryMethodLookup(Id),
    CONSTRAINT FK_CommunicationNotification_Status        FOREIGN KEY (Status)          REFERENCES NotificationStatusLookup(Id)
);

INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'CommunicationNotification', 1
WHERE NOT EXISTS (SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'CommunicationNotification');
