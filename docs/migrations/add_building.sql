-- Migrazione: aggiunta tabella Building
-- Un condominio è composto da uno o più edifici.
-- Ogni unità immobiliare appartiene a un edificio.

-- 1. Tabella Building
CREATE TABLE Building (
    Id                    INT            NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    CondominiumId         INT            NOT NULL,
    Name                  NVARCHAR(200)  NOT NULL,
    Description           NVARCHAR(1000) NULL,
    Code                  NVARCHAR(50)   NULL,
    Address               NVARCHAR(500)  NULL,
    YearOfConstruction    INT            NULL,
    NumberOfFloors        INT            NULL,
    HasElevator           BIT            NOT NULL DEFAULT 0,
    IsActive              BIT            NOT NULL DEFAULT 1,
    CreatedById           INT            NOT NULL,
    CreatedByFullName     NVARCHAR(200)  NULL,
    LastUpdatedById       INT            NULL,
    LastUpdatedByFullName NVARCHAR(200)  NULL,
    IsDeleted             BIT            NOT NULL DEFAULT 0,
    CreationDate          DATETIME2      NOT NULL,
    LastUpdateDate        DATETIME2      NULL,
    CONSTRAINT FK_Building_Condominium FOREIGN KEY (CondominiumId) REFERENCES Condominium(Id)
);

-- 2. Indice su CondominiumId
CREATE INDEX IX_Building_CondominiumId ON Building (CondominiumId);
CREATE INDEX IX_Building_TenantId      ON Building (TenantId);

-- 3. Hilo sequence entry
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'Building', 1
WHERE NOT EXISTS (
    SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'Building'
);

-- 4. Aggiunta FK BuildingId a RealEstateUnit (nullable: le unità esistenti non hanno edificio)
ALTER TABLE RealEstateUnit ADD BuildingId INT NULL;
ALTER TABLE RealEstateUnit ADD CONSTRAINT FK_RealEstateUnit_Building
    FOREIGN KEY (BuildingId) REFERENCES Building(Id);
