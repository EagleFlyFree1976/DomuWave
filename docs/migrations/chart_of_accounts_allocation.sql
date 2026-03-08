-- Aggiunge i campi per il metodo di calcolo della ripartizione su ChartOfAccounts
-- AllocationMethod: 0 = Standard (100% millesimale), 1 = Misto (Y% millesimale + (100-Y)% fattore piano/abitanti)
-- MillesimalPercentage: percentuale Y (0-100) da ripartire via millesimale (solo se AllocationMethod = 1)
-- FloorWeight: peso del piano nel fattore = FloorWeight * Floor + InhabitantsWeight * NumeroAbitanti (solo se AllocationMethod = 1)
-- InhabitantsWeight: peso del numero di abitanti nel fattore (solo se AllocationMethod = 1)

ALTER TABLE ChartOfAccounts ADD AllocationMethod     INT           NOT NULL DEFAULT 0;
ALTER TABLE ChartOfAccounts ADD MillesimalPercentage DECIMAL(5,2)  NULL;
ALTER TABLE ChartOfAccounts ADD FloorWeight          DECIMAL(10,4) NULL;
ALTER TABLE ChartOfAccounts ADD InhabitantsWeight    DECIMAL(10,4) NULL;
