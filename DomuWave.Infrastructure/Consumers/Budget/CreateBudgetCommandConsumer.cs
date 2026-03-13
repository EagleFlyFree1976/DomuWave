using CPQ.Core.Consumers;
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

        // Crea una voce di budget (Amount=0) per ogni conto attivo del condominio
        var accounts = await session.Query<ChartOfAccounts>()
            .Where(a => a.Condominium.Id == condominium.Id && !a.IsDeleted && a.IsActive)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var account in accounts)
        {
            var item = new BudgetItem
            {
                Budget = created,
                Tenant = created.Tenant,
                Account = account,
                Name   = string.Empty,
                Amount = 0,
            };
            item.Trace(currentUser);
            await session.SaveAsync(item, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return created.ToReadDto();
    }
}
