-- Migration: Add ChartOfAccountsTemplate and ChartOfAccountsTemplateItem tables
-- Date: 2026-03-08

-- ─── ChartOfAccountsTemplate ────────────────────────────────────────────────
CREATE TABLE ChartOfAccountsTemplate (
    Id                    INT            NOT NULL,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    Name                  NVARCHAR(200)  NOT NULL,
    Description           NVARCHAR(500)  NULL,
    IsActive              BIT            NOT NULL DEFAULT 1,
    IsDeleted             BIT            NOT NULL DEFAULT 0,
    CreatedById           INT            NOT NULL,
    CreatedByFullName     NVARCHAR(200)  NULL,
    LastUpdatedById       INT            NULL,
    LastUpdatedByFullName NVARCHAR(200)  NULL,
    CreationDate          DATETIME2      NOT NULL,
    LastUpdateDate        DATETIME2      NULL,
    CONSTRAINT PK_ChartOfAccountsTemplate PRIMARY KEY (Id),
    CONSTRAINT FK_ChartOfAccountsTemplate_Tenant FOREIGN KEY (TenantId) REFERENCES Tenant(Id)
);

-- ─── ChartOfAccountsTemplateItem ─────────────────────────────────────────────
CREATE TABLE ChartOfAccountsTemplateItem (
    Id                    INT            NOT NULL,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    TemplateId            INT            NOT NULL,
    ParentItemId          INT            NULL,
    Code                  NVARCHAR(50)   NOT NULL,
    Name                  NVARCHAR(200)  NOT NULL,
    Description           NVARCHAR(500)  NULL,
    Type                  INT            NOT NULL DEFAULT 2, -- 1=Entrata, 2=Uscita, 3=Patrimoniale
    SortOrder             INT            NOT NULL DEFAULT 0,
    IsActive              BIT            NOT NULL DEFAULT 1,
    IsDeleted             BIT            NOT NULL DEFAULT 0,
    CreatedById           INT            NOT NULL,
    CreatedByFullName     NVARCHAR(200)  NULL,
    LastUpdatedById       INT            NULL,
    LastUpdatedByFullName NVARCHAR(200)  NULL,
    CreationDate          DATETIME2      NOT NULL,
    LastUpdateDate        DATETIME2      NULL,
    CONSTRAINT PK_ChartOfAccountsTemplateItem PRIMARY KEY (Id),
    CONSTRAINT FK_ChartOfAccountsTemplateItem_Tenant    FOREIGN KEY (TenantId)    REFERENCES Tenant(Id),
    CONSTRAINT FK_ChartOfAccountsTemplateItem_Template  FOREIGN KEY (TemplateId)  REFERENCES ChartOfAccountsTemplate(Id),
    CONSTRAINT FK_ChartOfAccountsTemplateItem_Parent    FOREIGN KEY (ParentItemId) REFERENCES ChartOfAccountsTemplateItem(Id)
);

-- ─── NHibernate hilo sequences ───────────────────────────────────────────────
INSERT INTO hibernate_unique_key (entity_type, next_hi)
VALUES ('ChartOfAccountsTemplate', 1);

INSERT INTO hibernate_unique_key (entity_type, next_hi)
VALUES ('ChartOfAccountsTemplateItem', 1);
