-- Verifica email (double opt-in) + dati condominio sul record di staging registrazione
ALTER TABLE PendingRegistration ADD VerificationToken     NVARCHAR(200) NULL;
ALTER TABLE PendingRegistration ADD VerificationExpiresAt DATETIME2     NULL;
ALTER TABLE PendingRegistration ADD CondominiumName       NVARCHAR(200) NULL;
ALTER TABLE PendingRegistration ADD CondominiumCode       NVARCHAR(50)  NULL;
ALTER TABLE PendingRegistration ADD CondominiumCity       NVARCHAR(100) NULL;
ALTER TABLE PendingRegistration ADD CondominiumZip        NVARCHAR(10)  NULL;
