-- ============================================================
-- Migration: MillesimalTable - aggiunta colonna IsDraft
-- Date: 2026-03-06
-- ============================================================

-- Aggiunge la colonna IsDraft alla tabella MillesimalTable.
-- Le tabelle esistenti vengono messe in stato Bozza (IsDraft = 1, IsActive = 0)
-- a meno che non abbiano già IsActive = 1.

ALTER TABLE MillesimalTable
    ADD IsDraft BIT NOT NULL DEFAULT 1;

-- Le tabelle già attive (IsActive = 1) escono dalla bozza
UPDATE MillesimalTable
SET IsDraft = 0
WHERE IsActive = 1 AND IsDeleted = 0;
