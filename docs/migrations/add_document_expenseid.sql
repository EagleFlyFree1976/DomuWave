-- Aggiunge EntityId e EntityFullName alla tabella Document
-- per collegare un documento a qualsiasi entità (Expense, WorkQuote, ecc.) senza FK

ALTER TABLE Document ADD EntityId       INT           NULL;
ALTER TABLE Document ADD EntityFullName NVARCHAR(500) NULL;
GO
CREATE INDEX IX_Document_Entity ON Document (EntityFullName, EntityId)
    WHERE EntityId IS NOT NULL AND IsDeleted = 0;
