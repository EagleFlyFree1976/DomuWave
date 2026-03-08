-- Aggiunge il numero statico di abitanti per unità immobiliare.
-- Usato nel calcolo della ripartizione Mista su ChartOfAccounts (fattore = FloorWeight * Floor + InhabitantsWeight * NumeroAbitanti).

ALTER TABLE RealEstateUnit ADD NumeroAbitanti INT NOT NULL DEFAULT 1;
