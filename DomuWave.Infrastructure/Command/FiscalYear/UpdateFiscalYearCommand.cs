using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class UpdateFiscalYearCommand : BaseCommand, IQuery<FiscalYearReadDto>
{
    public int FiscalYearId { get; set; }
    public FiscalYearUpdateDto Dto { get; set; }

    public UpdateFiscalYearCommand() { }

    public UpdateFiscalYearCommand(int currentUserId) : base(currentUserId) { }
    public UpdateFiscalYearCommand(int currentUserId, int fiscalYearId, FiscalYearUpdateDto dto) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
        Dto = dto;
    }
}
