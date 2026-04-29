-- Aggiunge FeeNotice ai tipi di comunicazione ammessi dal CHECK constraint

ALTER TABLE Communication DROP CONSTRAINT CK_Communication_Type;

ALTER TABLE Communication ADD CONSTRAINT CK_Communication_Type
    CHECK (CommunicationType IN ('Notice', 'Meeting', 'Maintenance', 'Emergency', 'Info', 'FeeNotice'));
