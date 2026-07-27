-- Consente di sovrascrivere manualmente la denominazione (DisplayName) dell'unità immobiliare,
-- impedendo il ricalcolo automatico basato sui cognomi dei proprietari attivi.

ALTER TABLE RealEstateUnit ADD IsDisplayNameOverridden BIT NOT NULL DEFAULT 0;
