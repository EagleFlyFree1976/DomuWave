using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FiscalYear;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;
using Models = DomuWave.Services.Models;

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

        // ── Calcolo ClosingBalance per ogni UnitOpeningBalance dell'esercizio ──────
        await ComputeUnitClosingBalances(command.FiscalYearId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return true;
    }

    private async Task ComputeUnitClosingBalances(
        int fiscalYearId,
        object currentUser,
        CancellationToken cancellationToken)
    {
        // Somma quote addebitate e pagate per unità nell'esercizio
        // CondominiumFee → Installment → FiscalYear
        var feesByUnit = await session.Query<CondominiumFee>()
            .Where(f => f.Installment.FiscalYear.Id == fiscalYearId && !f.IsDeleted)
            .GroupBy(f => f.Unit.Id)
            .Select(g => new
            {
                UnitId     = g.Key,
                TotalDue   = g.Sum(f => f.AmountDue),
                TotalPaid  = g.Sum(f => f.AmountPaid),
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Carica i record UnitOpeningBalance esistenti per l'esercizio
        var unitBalances = await session.Query<Models.UnitOpeningBalance>()
            .Where(b => b.FiscalYear.Id == fiscalYearId && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var feeMap = feesByUnit.ToDictionary(f => f.UnitId);
        var user   = currentUser as CPQ.Core.Memberships.IUser;

        foreach (var balance in unitBalances)
        {
            if (feeMap.TryGetValue(balance.Unit.Id, out var fees))
                balance.TotalMovements = fees.TotalDue - fees.TotalPaid;
            else
                balance.TotalMovements = 0m;

            balance.ClosingBalance = balance.OpeningBalance + balance.TotalMovements;
            if (user != null) balance.Trace(user);
            await session.UpdateAsync(balance, cancellationToken).ConfigureAwait(false);
        }

        // Per le unità che hanno fee ma non ancora un record UnitOpeningBalance,
        // crearlo con OpeningBalance=0
        var existingUnitIds = unitBalances.Select(b => b.Unit.Id).ToHashSet();

        var fiscalYear = await session.Query<FiscalYear>()
            .FirstOrDefaultAsync(f => f.Id == fiscalYearId && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        foreach (var fee in feesByUnit.Where(f => !existingUnitIds.Contains(f.UnitId)))
        {
            var unit = await session.Query<RealEstateUnit>()
                .FirstOrDefaultAsync(u => u.Id == fee.UnitId && !u.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            if (unit == null || fiscalYear == null) continue;

            var newBalance = new Models.UnitOpeningBalance
            {
                Unit           = unit,
                FiscalYear     = fiscalYear,
                Tenant         = unit.Tenant,
                OpeningBalance = 0m,
                TotalMovements = fee.TotalDue - fee.TotalPaid,
                ClosingBalance = fee.TotalDue - fee.TotalPaid,
            };
            if (user != null) newBalance.Trace(user);
            await session.SaveAsync(newBalance, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
