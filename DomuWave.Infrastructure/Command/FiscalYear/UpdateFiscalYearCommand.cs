using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class UpdateFiscalYearCommand : BaseTenantRelatedCommand, IQuery<FiscalYearReadDto>
{
    public int FiscalYearId { get; set; }
    public FiscalYearUpdateDto Dto { get; set; }

    public UpdateFiscalYearCommand() { }

    public UpdateFiscalYearCommand(int currentUserId) : base(currentUserId) { }
    public UpdateFiscalYearCommand(int currentUserId, Guid tenantId, int fiscalYearId, FiscalYearUpdateDto dto) : base(currentUserId, tenantId)
    {
        FiscalYearId = fiscalYearId;
        Dto = dto;
    }
}
