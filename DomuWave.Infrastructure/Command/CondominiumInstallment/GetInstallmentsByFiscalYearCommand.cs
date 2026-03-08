using DomuWave.Services.Dto.CondominiumInstallment;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class GetInstallmentsByFiscalYearCommand : BaseCommand, IQuery<IList<CondominiumInstallmentReadDto>>
{
    public int CondominiumId { get; set; }
    public int FiscalYearId  { get; set; }

    public GetInstallmentsByFiscalYearCommand() { }

    public GetInstallmentsByFiscalYearCommand(int currentUserId) : base(currentUserId) { }
    public GetInstallmentsByFiscalYearCommand(int currentUserId, int condominiumId, int fiscalYearId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        FiscalYearId  = fiscalYearId;
    }
}
