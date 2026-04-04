using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateBudgetCommandConsumer
    : InMemoryConsumerBase<CreateBudgetCommand, BudgetReadDto>
{
    private readonly IBudgetService       _budgetService;
    private readonly ICondominiumService  _condominiumService;
    private readonly IFiscalYearService   _fiscalYearService;
    private readonly IUserService         _userService;

    public CreateBudgetCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBudgetService budgetService,
        ICondominiumService condominiumService,
        IFiscalYearService fiscalYearService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _budgetService      = budgetService;
        _condominiumService = condominiumService;
        _fiscalYearService  = fiscalYearService;
        _userService        = userService;
    }

    protected override async Task<BudgetReadDto> Consume(
        CreateBudgetCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var condominium = await _condominiumService
            .GetByIdAsync(command.Dto.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        var fiscalYear = await _fiscalYearService
            .GetByIdAsync(command.Dto.FiscalYearId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Unicità: al massimo un budget per tipo per condominio per esercizio
        var typeLabel = command.Dto.Type == BudgetType.Preventivo ? "previsionale" : "consuntivo";
        var duplicate = await session.Query<Budget>()
            .AnyAsync(b => b.Condominium.Id == command.Dto.CondominiumId
                        && b.FiscalYear.Id  == command.Dto.FiscalYearId
                        && b.Type           == command.Dto.Type
                        && !b.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (duplicate)
            throw new ValidatorException(
                $"Esiste già un budget {typeLabel} per questo condominio ed esercizio fiscale.");

        var budget = new Budget
        {
            Condominium   = condominium,
            FiscalYear    = fiscalYear,
            Tenant        = condominium.Tenant,
            Type          = command.Dto.Type,
            Status        = session.Load<BudgetStatus>(BudgetStatus.Draft),
            TotalIncome   = 0,
            TotalExpenses = 0,
            Description   = command.Dto.Notes,
        };

        var created = await _budgetService
            .CreateAsync(budget, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // ── Popolamento voci ───────────────────────────────────────────────────
        // Per un budget Preventivo: cerca il Consuntivo dell'esercizio precedente
        // e copia i suoi importi come punto di partenza.
        // Se non esiste, crea voci vuote per tutti i conti attivi (comportamento base).

        if (command.Dto.Type == BudgetType.Preventivo)
        {
            await CreatePreventivoItems(created, condominium, fiscalYear, command.Dto, currentUser, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            await CreateConsuntivoItemsAndDistribute(created, condominium, fiscalYear, currentUser, cancellationToken)
                .ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return created.ToReadDto();
    }

    // ── Preventivo: copia dal Consuntivo precedente o crea voci vuote ─────────

    private async Task CreatePreventivoItems(
        Budget budget,
        Models.Condominium condominium,
        FiscalYear fiscalYear,
        CreateBudgetDto dto,
        CPQ.Core.Memberships.IUser currentUser,
        CancellationToken ct)
    {
        // Determina il budget consuntivo sorgente:
        // 1. Se il client ha passato un SourceConsuntivoId esplicito, usa quello.
        // 2. Altrimenti cerca l'ultimo consuntivo dell'esercizio precedente (comportamento legacy).
        Budget? sourceConsuntivo = null;

        if (dto.SourceConsuntivoId.HasValue)
        {
            sourceConsuntivo = await session.Query<Budget>()
                .Where(b => b.Id == dto.SourceConsuntivoId.Value
                         && b.Type == BudgetType.Consuntivo
                         && b.Condominium.Id == condominium.Id
                         && !b.IsDeleted)
                .FetchMany(b => b.Items)
                .ThenFetch(i => i.Account)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }
        else
        {
            sourceConsuntivo = await session.Query<Budget>()
                .Where(b => b.Condominium.Id == condominium.Id
                         && b.Type           == BudgetType.Consuntivo
                         && b.FiscalYear.EndDate < fiscalYear.StartDate
                         && !b.IsDeleted)
                .OrderByDescending(b => b.FiscalYear.EndDate)
                .Take(1)
                .FetchMany(b => b.Items)
                .ThenFetch(i => i.Account)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        var sourceItems = sourceConsuntivo?.Items
            .Where(i => !i.IsDeleted && i.Account != null && i.Account.IsActive)
            .OrderBy(i => i.Account.Code)
            .ToList();

        if (sourceItems != null && sourceItems.Count > 0)
        {
            var multiplier = 1m + (dto.IncreasePercentage / 100m);

            foreach (var src in sourceItems)
            {
                var amount = Math.Round(src.Amount * multiplier, 2);
                var item = new BudgetItem
                {
                    Budget      = budget,
                    Tenant      = budget.Tenant,
                    Account     = src.Account,
                    // Fotografa codice e nome del conto al momento della creazione
                    AccountCode = src.Account.Code,
                    AccountName = src.Account.Name,
                    Name        = src.Name ?? string.Empty,
                    Amount      = amount,
                    Notes       = dto.IncreasePercentage != 0
                        ? $"Importato da consuntivo con maggiorazione {dto.IncreasePercentage:0.##}%"
                        : src.Notes,
                };
                item.Trace(currentUser);
                await session.SaveAsync(item, ct).ConfigureAwait(false);
            }
        }
        else
        {
            // Nessun consuntivo sorgente: voci vuote per ogni conto attivo
            var accounts = await session.Query<ChartOfAccounts>()
                .Where(a => a.Condominium.Id == condominium.Id && !a.IsDeleted && a.IsActive)
                .OrderBy(a => a.Code)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            foreach (var account in accounts)
            {
                var item = new BudgetItem
                {
                    Budget      = budget,
                    Tenant      = budget.Tenant,
                    Account     = account,
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    Name        = string.Empty,
                    Amount      = 0,
                };
                item.Trace(currentUser);
                await session.SaveAsync(item, ct).ConfigureAwait(false);
            }
        }
    }

    // ── Consuntivo: aggrega le spese per conto, calcola i totali ─────────────
    // La ripartizione per unità viene generata solo all'approvazione.

    private async Task CreateConsuntivoItemsAndDistribute(
        Budget budget,
        Models.Condominium condominium,
        FiscalYear fiscalYear,
        CPQ.Core.Memberships.IUser currentUser,
        CancellationToken ct)
    {
        // 1. Tutti i conti attivi del condominio
        var accounts = await session.Query<ChartOfAccounts>()
            .Where(a => a.Condominium.Id == condominium.Id && !a.IsDeleted && a.IsActive)
            .OrderBy(a => a.Code)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        // 2. Totale spese (NetAmount) per conto nell'esercizio
        var expensesByAccount = await session.Query<Expense>()
            .Where(e => e.FiscalYear.Id  == fiscalYear.Id
                     && e.Condominium.Id == condominium.Id
                     && !e.IsDeleted)
            .GroupBy(e => e.Account.Id)
            .Select(g => new { AccountId = g.Key, Total = g.Sum(e => e.NetAmount) })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var movementMap = expensesByAccount.ToDictionary(x => x.AccountId, x => x.Total);
        decimal totalExpenses = 0m;

        // 3. Crea un BudgetItem per ogni conto con il totale dei movimenti
        foreach (var account in accounts)
        {
            var amount = movementMap.TryGetValue(account.Id, out var m) ? m : 0m;
            totalExpenses += amount;

            var item = new BudgetItem
            {
                Budget      = budget,
                Tenant      = budget.Tenant,
                Account     = account,
                AccountCode = account.Code,
                AccountName = account.Name,
                Name        = string.Empty,
                Amount      = amount,
            };
            item.Trace(currentUser);
            await session.SaveAsync(item, ct).ConfigureAwait(false);
        }

        // 4. Aggiorna il totale uscite sul budget
        budget.TotalExpenses = totalExpenses;
        await session.SaveOrUpdateAsync(budget, ct).ConfigureAwait(false);
        // La ripartizione per unità viene generata all'approvazione (ApproveBudgetCommandConsumer)
    }
}
