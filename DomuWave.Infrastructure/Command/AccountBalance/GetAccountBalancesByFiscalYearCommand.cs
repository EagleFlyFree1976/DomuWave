using DomuWave.Services.Dto.Contabilita.AccountBalance;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.AccountBalance;

public class GetAccountBalancesByFiscalYearCommand : BaseCommand, IQuery<IList<AccountBalanceReadDto>>
{
    public int FiscalYearId { get; set; }
    public GetAccountBalancesByFiscalYearCommand() { }
    public GetAccountBalancesByFiscalYearCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
        => FiscalYearId = fiscalYearId;
}
