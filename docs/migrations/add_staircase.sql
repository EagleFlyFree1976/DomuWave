-- ============================================================
-- add_staircase.sql
-- Aggiunge la tabella Staircase e la FK su RealEstateUnit
-- ============================================================

-- 1. Tabella Staircase
CREATE TABLE Staircase (
    Id                    INT            NOT NULL PRIMARY KEY,
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    CondominiumId         INT            NOT NULL,
    Name                  NVARCHAR(50)   NOT NULL,
    IsActive              BIT            NOT NULL DEFAULT 1,
    CreatedById           INT            NOT NULL,
    CreatedByFullName     NVARCHAR(200)  NULL,
    LastUpdatedById       INT            NULL,
    LastUpdatedByFullName NVARCHAR(200)  NULL,
    IsDeleted             BIT            NOT NULL DEFAULT 0,
    CreationDate          DATETIME2      NOT NULL,
    LastUpdateDate        DATETIME2      NULL,
    CONSTRAINT FK_Staircase_Condominium FOREIGN KEY (CondominiumId) REFERENCES Condominium(Id)
);

-- 2. Indice univoco per nome scala per condominio (escludendo i deleted)
CREATE UNIQUE INDEX UQ_Staircase_Condominium_Name
    ON Staircase (CondominiumId, Name)
    WHERE IsDeleted = 0;

-- 3. Hilo sequence
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'Staircase', 1
WHERE NOT EXISTS (
    SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'Staircase'
);

-- 4. Colonna FK su RealEstateUnit (nullable — l'unità può non avere scala)
ALTER TABLE RealEstateUnit ADD StaircaseId INT NULL;
ALTER TABLE RealEstateUnit ADD CONSTRAINT FK_RealEstateUnit_Staircase
    FOREIGN KEY (StaircaseId) REFERENCES Staircase(Id);

-- 5. Rimuovi la vecchia colonna stringa Staircase
-- NOTA: eseguire solo dopo aver migrato i dati esistenti se necessario
-- ALTER TABLE RealEstateUnit DROP COLUMN Staircase;
