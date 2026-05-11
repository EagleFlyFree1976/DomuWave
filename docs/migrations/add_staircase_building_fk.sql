-- Aggiunge BuildingId (nullable) alla tabella Staircase
-- Una scala può appartenere a un edificio specifico del condominio

ALTER TABLE Staircase
    ADD BuildingId INT NULL;

ALTER TABLE Staircase
    ADD CONSTRAINT FK_Staircase_Building
        FOREIGN KEY (BuildingId) REFERENCES Building(Id);
