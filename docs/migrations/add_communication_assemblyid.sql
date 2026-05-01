-- Aggiunge FK AssemblyId alla tabella Communication
-- Una comunicazione di tipo Meeting è collegata a un'assemblea

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Communication') AND name = 'AssemblyId'
)
BEGIN
    ALTER TABLE Communication
        ADD AssemblyId INT NULL;

    ALTER TABLE Communication
        ADD CONSTRAINT FK_Communication_Assembly
        FOREIGN KEY (AssemblyId) REFERENCES Assembly(Id);
END
