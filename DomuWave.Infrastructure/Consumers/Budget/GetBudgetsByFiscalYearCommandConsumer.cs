using CPQ.Core.Consumers;
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

public class GetBudgetsByFiscalYearCommandConsumer
    : InMemoryConsumerBase<GetBudgetsByFiscalYearCommand, IList<BudgetReadDto>>
{
    private readonly IUserService _userService;

    public GetBudgetsByFiscalYearCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<IList<BudgetReadDto>> Consume(
        GetBudgetsByFiscalYearCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var budgets = await session.Query<Budget>()
            .Where(b => b.FiscalYear.Id == command.FiscalYearId && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return budgets.Select(b => b.ToReadDto()).ToList();
    }
}
