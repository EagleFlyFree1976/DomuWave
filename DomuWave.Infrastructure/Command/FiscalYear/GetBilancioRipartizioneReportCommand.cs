using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class GetBilancioRipartizioneReportCommand : BaseCommand, IQuery<BilancioRipartizioneReportDto>
{
    public int FiscalYearId { get; set; }

    public GetBilancioRipartizioneReportCommand() { }

    public GetBilancioRipartizioneReportCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
    }
}
