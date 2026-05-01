-- Aggiunge FK InstallmentId alla tabella Communication
-- Vincolo: una rata può avere al massimo una comunicazione

ALTER TABLE Communication
    ADD InstallmentId INT NULL;

ALTER TABLE Communication
    ADD CONSTRAINT FK_Communication_Installment
    FOREIGN KEY (InstallmentId) REFERENCES CondominiumInstallment(Id);

-- Indice univoco parziale: solo sulle righe dove InstallmentId è valorizzato
CREATE UNIQUE INDEX UQ_Communication_Installment
    ON Communication (InstallmentId)
    WHERE InstallmentId IS NOT NULL AND IsDeleted = 0;
