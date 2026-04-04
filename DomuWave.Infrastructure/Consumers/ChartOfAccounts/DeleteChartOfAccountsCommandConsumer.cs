using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccounts;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteChartOfAccountsCommandConsumer
    : InMemoryConsumerBase<DeleteChartOfAccountsCommand, bool>
{
    private readonly IChartOfAccountsService _accountService;
    private readonly IUserService            _userService;

    public DeleteChartOfAccountsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IChartOfAccountsService accountService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _accountService = accountService;
        _userService    = userService;
    }

    protected override async Task<bool> Consume(
        DeleteChartOfAccountsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _accountService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null)
            throw new NotFoundException("Conto non trovato.");

        // Blocca sempre se ha sotto-conti: la gerarchia deve essere pulita dall'utente prima
        var hasChildren = await session.Query<ChartOfAccounts>()
            .AnyAsync(x => x.ParentAccount.Id == command.Id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (hasChildren)
            throw new ValidatorException("Impossibile eliminare: il conto ha sotto-conti associati.");

        // ── Analisi degli usi per stato del budget ────────────────────────────
        // Carica tutte le voci di budget non cancellate agganciata a questo conto,
        // proiettando solo i campi necessari per la classificazione.
        var budgetItemUsages = await session.Query<BudgetItem>()
            .Where(x => x.Account.Id == command.Id && !x.IsDeleted)
            .Select(x => new
            {
                ItemId   = x.Id,
                BudgetId = x.Budget.Id,
                StatusId = x.Budget.Status.Id,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var hasDraftUsages    = budgetItemUsages.Any(x => x.StatusId == BudgetStatus.Draft);
        var hasLockedUsages   = budgetItemUsages.Any(x => x.StatusId != BudgetStatus.Draft);

        // Se ci sono voci in budget bozza, il sostitutivo è obbligatorio
        if (hasDraftUsages && !command.ReplacementAccountId.HasValue)
            throw new ValidatorException(
                "Il conto è utilizzato in voci di budget in bozza. " +
                "Seleziona un conto sostitutivo per procedere con l'eliminazione.");

        ChartOfAccounts? replacement = null;
        if (command.ReplacementAccountId.HasValue)
        {
            replacement = await _accountService
                .GetByIdAsync(command.ReplacementAccountId.Value, currentUser, cancellationToken)
                .ConfigureAwait(false);
            if (replacement == null)
                throw new NotFoundException("Conto sostitutivo non trovato.");
            if (replacement.Id == command.Id)
                throw new ValidatorException("Il conto sostitutivo non può coincidere con il conto da eliminare.");
        }

        // ── Riassegna voci e spese dei budget in bozza al sostitutivo ─────────
        if (hasDraftUsages && replacement != null)
        {
            var draftItemIds = budgetItemUsages
                .Where(x => x.StatusId == BudgetStatus.Draft)
                .Select(x => x.ItemId)
                .ToList();

            // Carica le voci in bozza come proiezione scalare per evitare problemi
            // con lazy proxy di Budget: il BudgetId viene letto direttamente in SQL.
            var draftItemProjections = await session.Query<BudgetItem>()
                .Where(x => draftItemIds.Contains(x.Id))
                .Select(x => new { x.Id, BudgetId = x.Budget.Id, x.Amount })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // Per ogni budget in bozza: riassegna o accorpa la voce al sostitutivo
            var groupedByBudget = draftItemProjections.GroupBy(x => x.BudgetId);
            foreach (var group in groupedByBudget)
            {
                var budgetId = group.Key;

                // Verifica se esiste già una voce per il conto sostitutivo nello STESSO budget in bozza.
                // Il filtro su Budget.Status.Id == Draft è essenziale: senza di esso la query
                // potrebbe trovare una voce del conto sostitutivo in un budget approvato/chiuso
                // con lo stesso ID (impossibile per ID diversi, ma garantisce comunque
                // che non si tocchino budget non-bozza).
                var existingReplacement = await session.Query<BudgetItem>()
                    .FirstOrDefaultAsync(x => x.Budget.Id       == budgetId
                                           && x.Budget.Status.Id == BudgetStatus.Draft
                                           && x.Account.Id      == replacement.Id
                                           && !x.IsDeleted, cancellationToken)
                    .ConfigureAwait(false);

                if (existingReplacement != null)
                {
                    // Accorpa: somma gli importi nella voce sostitutiva esistente, poi cancella le originali
                    existingReplacement.Amount += group.Sum(x => x.Amount);
                    existingReplacement.Trace(currentUser);
                    await session.SaveOrUpdateAsync(existingReplacement, cancellationToken).ConfigureAwait(false);

                    foreach (var item in group)
                    {
                        item.IsDeleted = true;
                        item.Trace(currentUser);
                        await session.SaveOrUpdateAsync(item, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    // Riassegna: sposta le voci al conto sostitutivo (snapshot aggiornato)
                    foreach (var item in group)
                    {
                        item.Account     = replacement;
                        item.AccountCode = replacement.Code;
                        item.AccountName = replacement.Name;
                        item.Trace(currentUser);
                        await session.SaveOrUpdateAsync(item, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            // Riassegna anche le spese dei budget in bozza al conto sostitutivo
            // (le spese dei budget approvati/chiusi rimangono sul conto cancellato)
            var draftBudgetIds = budgetItemUsages
                .Where(x => x.StatusId == BudgetStatus.Draft)
                .Select(x => x.BudgetId)
                .Distinct()
                .ToList();

            // Le spese sono legate a condominio+esercizio, non direttamente al budget.
            // Consideriamo spese "in bozza" quelle del condominio+esercizio dei budget in bozza
            // che non siano già associate a un budget approvato/chiuso.
            // Strategia conservativa: riassegna le spese il cui esercizio fiscale corrisponde
            // a un budget in bozza e non a uno approvato/chiuso dello stesso condominio.
            var draftBudgetFiscalYears = await session.Query<Budget>()
                .Where(b => draftBudgetIds.Contains(b.Id))
                .Select(b => new { FiscalYearId = b.FiscalYear.Id, CondominiumId = b.Condominium.Id })
                .Distinct()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var ctx in draftBudgetFiscalYears)
            {
                // Verifica che non esista un budget approvato/chiuso per lo stesso esercizio+condominio:
                // se esiste, le spese sono "bloccate" e non vanno riassegnate
                var hasApprovedBudget = await session.Query<Budget>()
                    .AnyAsync(b => b.Condominium.Id == ctx.CondominiumId
                                && b.FiscalYear.Id  == ctx.FiscalYearId
                                && (b.Status.Id == BudgetStatus.Approved || b.Status.Id == BudgetStatus.Closed)
                                && !b.IsDeleted, cancellationToken)
                    .ConfigureAwait(false);

                if (!hasApprovedBudget)
                {
                    var expensesToReassign = await session.Query<Expense>()
                        .Where(e => e.Condominium.Id == ctx.CondominiumId
                                 && e.FiscalYear.Id  == ctx.FiscalYearId
                                 && e.Account.Id     == command.Id
                                 && !e.IsDeleted)
                        .ToListAsync(cancellationToken)
                        .ConfigureAwait(false);

                    foreach (var expense in expensesToReassign)
                    {
                        expense.Account = replacement;
                        expense.Trace(currentUser);
                        await session.SaveOrUpdateAsync(expense, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        // ── Cancellazione logica del conto ────────────────────────────────────
        // Se ci sono voci in budget approvati/chiusi, la FK rimane valida perché
        // IsDeleted non rompe la referenza — i movimenti storici restano agganciati.
        return await _accountService
            .DeleteAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
