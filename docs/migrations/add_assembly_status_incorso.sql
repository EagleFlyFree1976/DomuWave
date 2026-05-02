-- Aggiunge lo stato 'In corso' alla tabella AssemblyStatusLookup
IF NOT EXISTS (SELECT 1 FROM AssemblyStatusLookup WHERE Id = 10)
BEGIN
    INSERT INTO AssemblyStatusLookup (Id, Name) VALUES (10, 'In corso');
END
