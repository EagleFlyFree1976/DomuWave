using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class GetActiveFiscalYearCommand : BaseCommand, IQuery<FiscalYearReadDto>
{
    public int CondominiumId { get; set; }

    public GetActiveFiscalYearCommand() { }

    public GetActiveFiscalYearCommand(int currentUserId) : base(currentUserId) { }
    public GetActiveFiscalYearCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
