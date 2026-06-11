using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

/// <summary>
/// Salva in blocco gli override manuali delle celle del bilancio di ripartizione di un
/// esercizio, sostituendo l'eventuale set precedente. Ritorna il report ricalcolato.
/// </summary>
public class SaveBilancioRipartizioneOverridesCommand : BaseCommand, IQuery<BilancioRipartizioneReportDto>
{
    public int FiscalYearId { get; set; }
    public List<BilancioCellOverrideDto> Cells { get; set; } = new();

    public SaveBilancioRipartizioneOverridesCommand() { }

    public SaveBilancioRipartizioneOverridesCommand(int currentUserId, int fiscalYearId, List<BilancioCellOverrideDto> cells)
        : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
        Cells        = cells ?? new();
    }
}
