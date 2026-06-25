using DomuWave.Services.Dto.Contabilita.Bilancio;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Contabilita.Bilancio;

public class GetSituazionePatrimonialeCommand : BaseCommand, IQuery<SituazionePatrimonialeDto>
{
    public int FiscalYearId { get; set; }

    public GetSituazionePatrimonialeCommand() { }
    public GetSituazionePatrimonialeCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
        => FiscalYearId = fiscalYearId;
}
