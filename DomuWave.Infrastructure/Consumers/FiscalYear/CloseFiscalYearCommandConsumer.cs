using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FiscalYear;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CloseFiscalYearCommandConsumer : InMemoryConsumerBase<CloseFiscalYearCommand, bool>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService       _userService;

    public CloseFiscalYearCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService      fiscalYearService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService       = userService;
    }

    protected override async Task<bool> Consume(
        CloseFiscalYearCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // Chiusura dell'esercizio tramite il service (validazioni + cambio stato)
        await _fiscalYearService
            .CloseAsync(command.FiscalYearId, currentUser, command.Notes, cancellationToken)
            .ConfigureAwait(false);

        // ── Calcolo e salvataggio del ClosingBalance per ogni conto ────────────
        var balances = await session.Query<AccountBalance>()
            .Where(b => b.FiscalYear.Id == command.FiscalYearId && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Movimenti aggregati per conto nell'esercizio
        var movements = await session.Query<Expense>()
            .Where(e => e.FiscalYear.Id == command.FiscalYearId && !e.IsDeleted)
            .GroupBy(e => e.Account.Id)
            .Select(g => new { AccountId = g.Key, Total = g.Sum(e => e.NetAmount) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Arrotondamenti aggregati per conto (dalla tabella ExpenseAllocation)
        var roundingByAccount = await session.Query<ExpenseAllocation>()
            .Where(ea => ea.Expense.FiscalYear.Id == command.FiscalYearId && !ea.IsDeleted)
            .GroupBy(ea => ea.Expense.Account.Id)
            .Select(g => new { AccountId = g.Key, TotalRounding = g.Sum(ea => ea.RoundingAdjustment) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var movementMap = movements.ToDictionary(m => m.AccountId, m => m.Total);
        var roundingMap = roundingByAccount.ToDictionary(r => r.AccountId, r => r.TotalRounding);

        foreach (var balance in balances)
        {
            var movTotal  = movementMap.TryGetValue(balance.Account.Id, out var m) ? m : 0m;
            // Uscita: le spese riducono il saldo; Entrata/Patrimoniale: aumentano
            var signedMov = balance.Account.Type == ChartOfAccountsType.Uscita ? -movTotal : movTotal;

            balance.TotalBalance            = balance.OpeningBalance + signedMov;
            balance.ClosingBalance          = balance.TotalBalance;
            balance.TotalRoundingAdjustment = roundingMap.TryGetValue(balance.Account.Id, out var r) ? r : 0m;
            balance.Trace(currentUser);
            await session.UpdateAsync(balance, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
