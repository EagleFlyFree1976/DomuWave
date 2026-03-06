using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class ApproveBudgetCommandConsumer
    : InMemoryConsumerBase<ApproveBudgetCommand, bool>
{
    private readonly IBudgetService _budgetService;
    private readonly IUserService   _userService;

    public ApproveBudgetCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBudgetService budgetService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _budgetService = budgetService;
        _userService   = userService;
    }

    protected override async Task<bool> Consume(
        ApproveBudgetCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var budget = await session.Query<Budget>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (budget == null)
            throw new NotFoundException("Budget non trovato.");

        if (budget.Status?.Id != BudgetStatus.Draft)
            throw new ValidatorException("Solo i budget in stato Bozza possono essere approvati.");

        // Verifica che non esista già un budget dello stesso tipo (Preventivo/Consuntivo)
        // approvato o chiuso per lo stesso condominio e lo stesso esercizio fiscale.
        var tipoLabel = budget.Type == BudgetType.Preventivo ? "preventivo" : "consuntivo";
        var conflicting = await session.Query<Budget>()
            .AnyAsync(x => x.Id             != command.Id
                        && x.Condominium.Id == budget.Condominium.Id
                        && x.FiscalYear.Id  == budget.FiscalYear.Id
                        && x.Type           == budget.Type
                        && (x.Status.Id == BudgetStatus.Approved || x.Status.Id == BudgetStatus.Closed)
                        && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (conflicting)
            throw new ValidatorException(
                $"Esiste già un budget {tipoLabel} approvato o chiuso per questo esercizio e condominio. " +
                "Non è possibile approvarne un altro.");

        return await _budgetService
            .ApproveBudgetAsync(command.Id, command.CurrentUserId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
