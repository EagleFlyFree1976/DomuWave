using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class GetFiscalYearsByCondominiumCommand : BaseCommand, IQuery<IList<FiscalYearListItemDto>>
{
    public int CondominiumId { get; set; }

    public GetFiscalYearsByCondominiumCommand() { }

    public GetFiscalYearsByCondominiumCommand(int currentUserId) : base(currentUserId) { }
    public GetFiscalYearsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
