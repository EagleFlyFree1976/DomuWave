-- Aggiunge i campi catastali Foglio e Particella a RealEstateUnit
ALTER TABLE RealEstateUnit ADD Sheet  NVARCHAR(20) NULL;
ALTER TABLE RealEstateUnit ADD Parcel NVARCHAR(20) NULL;
