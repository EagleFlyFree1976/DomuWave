using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Helpers;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class PreApproveBudgetCommandConsumer
    : InMemoryConsumerBase<PreApproveBudgetCommand, bool>
{
    private readonly IUserService _userService;

    public PreApproveBudgetCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<bool> Consume(
        PreApproveBudgetCommand command,
        IMediationContext        mediationContext,
        CancellationToken       cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var budget = await session.Query<Budget>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (budget == null)
            throw new NotFoundException("Budget non trovato.");

        if (budget.Type != BudgetType.Preventivo)
            throw new ValidatorException("La pre-approvazione è disponibile solo per i budget Preventivo.");

        if (budget.Status?.Id != BudgetStatus.Draft)
            throw new ValidatorException("Solo i budget in stato Bozza possono essere pre-approvati.");

        var conflicting = await session.Query<Budget>()
            .AnyAsync(x => x.Id             != command.Id
                        && x.Condominium.Id == budget.Condominium.Id
                        && x.FiscalYear.Id  == budget.FiscalYear.Id
                        && x.Type           == budget.Type
                        && (x.Status.Id == BudgetStatus.Approved
                         || x.Status.Id == BudgetStatus.Closed
                         || x.Status.Id == BudgetStatus.PendingApproval)
                        && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (conflicting)
            throw new ValidatorException(
                "Esiste già un budget preventivo in pre-approvazione, approvato o chiuso per questo esercizio e condominio.");

        var hasEntrata = await session.Query<ChartOfAccounts>()
            .AnyAsync(a => a.Condominium.Id == budget.Condominium.Id
                        && a.Type == ChartOfAccountsType.Entrata
                        && a.IsActive && !a.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        var hasUscita = await session.Query<ChartOfAccounts>()
            .AnyAsync(a => a.Condominium.Id == budget.Condominium.Id
                        && a.Type == ChartOfAccountsType.Uscita
                        && a.IsActive && !a.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        var hasPatrimoniale = await session.Query<ChartOfAccounts>()
            .AnyAsync(a => a.Condominium.Id == budget.Condominium.Id
                        && a.Type == ChartOfAccountsType.Patrimoniale
                        && a.IsActive && !a.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (!hasEntrata || !hasUscita || !hasPatrimoniale)
        {
            var missing = new List<string>();
            if (!hasEntrata)      missing.Add("Entrata");
            if (!hasUscita)       missing.Add("Uscita");
            if (!hasPatrimoniale) missing.Add("Patrimoniale");
            throw new ValidatorException(
                $"Il piano dei conti non è completo. Mancano conti di tipo: {string.Join(", ", missing)}.");
        }

        await MillesimalTableGuard
            .LoadAndValidateAsync(session, budget.Condominium.Id, cancellationToken)
            .ConfigureAwait(false);

        budget.Status = session.Load<BudgetStatus>(BudgetStatus.PendingApproval);
        budget.Trace(currentUser);
        await session.SaveOrUpdateAsync(budget, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
