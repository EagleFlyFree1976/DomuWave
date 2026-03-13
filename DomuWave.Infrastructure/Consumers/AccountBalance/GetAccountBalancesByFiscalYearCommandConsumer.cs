using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.AccountBalance;
using DomuWave.Services.Dto.Contabilita.AccountBalance;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetAccountBalancesByFiscalYearCommandConsumer
    : InMemoryConsumerBase<GetAccountBalancesByFiscalYearCommand, IList<AccountBalanceReadDto>>
{
    private readonly IUserService _userService;

    public GetAccountBalancesByFiscalYearCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<AccountBalanceReadDto>> Consume(
        GetAccountBalancesByFiscalYearCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var fiscalYear = await session.Query<FiscalYear>()
            .FirstOrDefaultAsync(f => f.Id == command.FiscalYearId && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (fiscalYear == null)
            throw new NotFoundException("Esercizio fiscale non trovato.");

        // È il primo esercizio per il condominio?
        var isFirstFiscalYear = !await session.Query<FiscalYear>()
            .AnyAsync(f => f.Condominium.Id == fiscalYear.Condominium.Id
                        && f.Id != fiscalYear.Id
                        && f.StartDate < fiscalYear.StartDate
                        && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var isYearEditable = fiscalYear.Status?.Id != FiscalYearStatus.Closed
                          && fiscalYear.Status?.Id != FiscalYearStatus.Locked;

        var balances = await session.Query<AccountBalance>()
            .Where(b => b.FiscalYear.Id == command.FiscalYearId && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Movimenti per ciascun conto nell'esercizio (somma NetAmount spese non eliminate)
        var movements = await session.Query<Expense>()
            .Where(e => e.FiscalYear.Id == command.FiscalYearId && !e.IsDeleted)
            .GroupBy(e => e.Account.Id)
            .Select(g => new { AccountId = g.Key, Total = g.Sum(e => e.NetAmount) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Arrotondamenti aggregati per conto nell'esercizio
        var roundingByAccount = await session.Query<ExpenseAllocation>()
            .Where(ea => ea.Expense.FiscalYear.Id == command.FiscalYearId && !ea.IsDeleted)
            .GroupBy(ea => ea.Expense.Account.Id)
            .Select(g => new { AccountId = g.Key, TotalRounding = g.Sum(ea => ea.RoundingAdjustment) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var movementMap = movements.ToDictionary(m => m.AccountId, m => m.Total);
        var roundingMap = roundingByAccount.ToDictionary(r => r.AccountId, r => r.TotalRounding);

        var result = balances
            .OrderBy(b => b.Account.Code)
            .Select(b =>
            {
                var movTotal  = movementMap.TryGetValue(b.Account.Id, out var m) ? m : 0m;
                // Uscita: le spese riducono il saldo; Entrata/Patrimoniale: aumentano
                var signedMov = b.Account.Type == ChartOfAccountsType.Uscita ? -movTotal : movTotal;
                return new AccountBalanceReadDto
                {
                    Id                       = b.Id,
                    FiscalYearId             = fiscalYear.Id,
                    AccountId                = b.Account.Id,
                    AccountCode              = b.Account.Code ?? string.Empty,
                    AccountName              = b.Account.Name ?? string.Empty,
                    AccountType              = (int)b.Account.Type,
                    AccountTypeLabel         = b.Account.Type switch
                    {
                        ChartOfAccountsType.Entrata      => "Entrata",
                        ChartOfAccountsType.Patrimoniale => "Patrimoniale",
                        _                                => "Uscita",
                    },
                    OpeningBalance           = b.OpeningBalance,
                    MovementsTotal           = movTotal,
                    TotalBalance             = b.OpeningBalance + signedMov,
                    ClosingBalance           = b.ClosingBalance,
                    IsOpeningBalanceEditable = isFirstFiscalYear && isYearEditable,
                    TotalRoundingAdjustment  = roundingMap.TryGetValue(b.Account.Id, out var r) ? r : 0m,
                };
            })
            .ToList();

        return result;
    }
}
