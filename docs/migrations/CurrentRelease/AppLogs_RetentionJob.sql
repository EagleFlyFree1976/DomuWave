-- ============================================================================
-- AppLogs — pulizia automatica dei log piu vecchi di 4 giorni
-- Crea un SQL Server Agent Job che gira ogni notte alle 03:00.
-- Eseguire sul database in cui Serilog scrive la tabella AppLogs
-- (connection string "Logs").
-- ============================================================================
USE msdb;
GO

DECLARE @JobName      SYSNAME = N'DomuWave - Pulizia AppLogs (4 giorni)';
DECLARE @DatabaseName SYSNAME = DB_NAME();   -- adatta se AppLogs e su un altro DB

-- Rimuove il job se gia esistente (idempotente)
IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = @JobName)
    EXEC msdb.dbo.sp_delete_job @job_name = @JobName, @delete_unused_schedule = 1;

EXEC msdb.dbo.sp_add_job
    @job_name = @JobName,
    @enabled  = 1,
    @description = N'Cancella i record di AppLogs piu vecchi di 4 giorni.';

EXEC msdb.dbo.sp_add_jobstep
    @job_name   = @JobName,
    @step_name  = N'Delete old AppLogs',
    @subsystem  = N'TSQL',
    @database_name = @DatabaseName,
    @command    = N'
        SET NOCOUNT ON;
        -- Cancellazione a batch per evitare lock prolungati su tabelle grandi
        DECLARE @rows INT = 1;
        WHILE @rows > 0
        BEGIN
            DELETE TOP (5000) FROM dbo.AppLogs
            WHERE [TimeStamp] < DATEADD(DAY, -4, SYSUTCDATETIME());
            SET @rows = @@ROWCOUNT;
        END',
    @retry_attempts = 1,
    @retry_interval = 5;

EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'DomuWave_AppLogs_Daily_0300',
    @freq_type     = 4,            -- giornaliero
    @freq_interval = 1,
    @active_start_time = 030000;   -- 03:00:00

EXEC msdb.dbo.sp_attach_schedule
    @job_name      = @JobName,
    @schedule_name = N'DomuWave_AppLogs_Daily_0300';

EXEC msdb.dbo.sp_add_jobserver
    @job_name = @JobName;
GO
