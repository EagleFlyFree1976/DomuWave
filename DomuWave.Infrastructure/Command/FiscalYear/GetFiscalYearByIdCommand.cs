using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class GetFiscalYearByIdCommand : BaseTenantRelatedCommand, IQuery<FiscalYearReadDto>
{
    public int FiscalYearId { get; set; }

    public GetFiscalYearByIdCommand() { }

    public GetFiscalYearByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetFiscalYearByIdCommand(int currentUserId, Guid tenantId, int fiscalYearId) : base(currentUserId, tenantId)
    {
        FiscalYearId = fiscalYearId;
    }
}
