-- Aggiunge il campo Features alla tabella base_menues.
-- Contiene un elenco di feature code separati da virgola (es. 'EXPORT_PDF,ANALYTICS').
-- Se NULL o stringa vuota, la voce di menu è sempre visibile.
ALTER TABLE dbo.base_menues
    ADD Features NVARCHAR(500) NULL;
