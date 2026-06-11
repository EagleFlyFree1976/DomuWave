using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

/// <summary>
/// Cancella tutti gli override manuali del bilancio di ripartizione di un esercizio
/// ("Ripristina automatico"). Ritorna il report ricalcolato dai dati.
/// </summary>
public class ResetBilancioRipartizioneOverridesCommand : BaseCommand, IQuery<BilancioRipartizioneReportDto>
{
    public int FiscalYearId { get; set; }

    public ResetBilancioRipartizioneOverridesCommand() { }

    public ResetBilancioRipartizioneOverridesCommand(int currentUserId, int fiscalYearId)
        : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
    }
}
