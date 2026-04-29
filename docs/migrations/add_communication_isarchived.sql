-- Add IsArchived column to Communication table
ALTER TABLE Communication ADD IsArchived BIT NOT NULL DEFAULT 0;
