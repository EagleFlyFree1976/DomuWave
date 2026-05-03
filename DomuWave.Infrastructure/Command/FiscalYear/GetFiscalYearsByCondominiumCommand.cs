using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class GetFiscalYearsByCondominiumCommand : BaseCommand, IQuery<IList<FiscalYearListItemDto>>
{
    public int  CondominiumId { get; set; }
    public Guid TenantId      { get; set; }

    public GetFiscalYearsByCondominiumCommand() { }

    public GetFiscalYearsByCondominiumCommand(int currentUserId, int condominiumId, Guid tenantId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        TenantId      = tenantId;
    }
}
