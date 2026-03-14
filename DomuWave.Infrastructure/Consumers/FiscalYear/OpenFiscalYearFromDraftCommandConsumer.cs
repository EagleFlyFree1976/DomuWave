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

public class OpenFiscalYearFromDraftCommandConsumer : InMemoryConsumerBase<OpenFiscalYearFromDraftCommand, bool>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService       _userService;

    public OpenFiscalYearFromDraftCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService      fiscalYearService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService       = userService;
    }

    protected override async Task<bool> Consume(
        OpenFiscalYearFromDraftCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // Apertura dell'esercizio (Draft → Open)
        await _fiscalYearService
            .OpenFromDraftAsync(command.FiscalYearId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // ── Creazione dei record AccountBalance ────────────────────────────────
        var fiscalYear = await session.Query<FiscalYear>()
            .FirstOrDefaultAsync(f => f.Id == command.FiscalYearId && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (fiscalYear == null) return true; // non dovrebbe accadere

        // Tutti i ChartOfAccounts attivi del condominio
        var accounts = await session.Query<ChartOfAccounts>()
            .Where(a => a.Condominium.Id == fiscalYear.Condominium.Id && !a.IsDeleted && a.IsActive)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // È il primo esercizio per il condominio?
        var isFirstFiscalYear = !await session.Query<FiscalYear>()
            .AnyAsync(f => f.Condominium.Id == fiscalYear.Condominium.Id
                        && f.Id != fiscalYear.Id
                        && f.StartDate < fiscalYear.StartDate
                        && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        // Map accountId → ClosingBalance dell'esercizio precedente (se non primo esercizio)
        Dictionary<int, decimal> previousClosingMap = new();
        if (!isFirstFiscalYear)
        {
            // Esercizio più recente chiuso/bloccato con EndDate < StartDate di questo
            var previousFiscalYear = await session.Query<FiscalYear>()
                .Where(f => f.Condominium.Id == fiscalYear.Condominium.Id
                         && f.Id != fiscalYear.Id
                         && f.EndDate <= fiscalYear.StartDate
                         && (f.Status.Id == FiscalYearStatus.Closed || f.Status.Id == FiscalYearStatus.Locked)
                         && !f.IsDeleted)
                .OrderByDescending(f => f.EndDate)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (previousFiscalYear != null)
            {
                var prevBalances = await session.Query<AccountBalance>()
                    .Where(b => b.FiscalYear.Id == previousFiscalYear.Id && !b.IsDeleted)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                previousClosingMap = prevBalances.ToDictionary(b => b.Account.Id, b => b.ClosingBalance);
            }
        }

        // Evita duplicati se il metodo fosse chiamato due volte
        var existingAccountIds = await session.Query<AccountBalance>()
            .Where(b => b.FiscalYear.Id == fiscalYear.Id && !b.IsDeleted)
            .Select(b => b.Account.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var account in accounts)
        {
            if (existingAccountIds.Contains(account.Id)) continue;

            var opening = previousClosingMap.TryGetValue(account.Id, out var prev) ? prev : 0m;

            var balance = new AccountBalance
            {
                Tenant         = fiscalYear.Tenant,
                FiscalYear     = fiscalYear,
                Account        = account,
                OpeningBalance = opening,
                ClosingBalance = 0,
                IsDeleted      = false,
            };
            balance.Trace(currentUser);
            await session.SaveAsync(balance, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        // ── Propagazione UnitOpeningBalance dal ClosingBalance dell'esercizio precedente ──
        if (!isFirstFiscalYear)
        {
            await PropagateUnitOpeningBalances(fiscalYear, currentUser, cancellationToken)
                .ConfigureAwait(false);
        }

        return true;
    }

    private async Task PropagateUnitOpeningBalances(
        FiscalYear fiscalYear,
        object currentUser,
        CancellationToken cancellationToken)
    {
        // Esercizio precedente più recente (chiuso o bloccato)
        var previousFiscalYear = await session.Query<FiscalYear>()
            .Where(f => f.Condominium.Id == fiscalYear.Condominium.Id
                     && f.Id != fiscalYear.Id
                     && f.EndDate <= fiscalYear.StartDate
                     && (f.Status.Id == FiscalYearStatus.Closed || f.Status.Id == FiscalYearStatus.Locked)
                     && !f.IsDeleted)
            .OrderByDescending(f => f.EndDate)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (previousFiscalYear == null) return;

        // ClosingBalance precedente per unità
        var previousBalances = await session.Query<Models.UnitOpeningBalance>()
            .Where(b => b.FiscalYear.Id == previousFiscalYear.Id && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!previousBalances.Any()) return;

        // Evita duplicati
        var existingUnitIds = await session.Query<Models.UnitOpeningBalance>()
            .Where(b => b.FiscalYear.Id == fiscalYear.Id && !b.IsDeleted)
            .Select(b => b.Unit.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var user = currentUser as CPQ.Core.Memberships.IUser;

        foreach (var prev in previousBalances)
        {
            if (existingUnitIds.Contains(prev.Unit.Id)) continue;

            var newBalance = new Models.UnitOpeningBalance
            {
                Unit           = prev.Unit,
                FiscalYear     = fiscalYear,
                Tenant         = fiscalYear.Tenant,
                OpeningBalance = prev.ClosingBalance,   // propagazione automatica
                TotalMovements = 0m,
                ClosingBalance = 0m,
            };
            if (user != null) newBalance.Trace(user);
            await session.SaveAsync(newBalance, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
