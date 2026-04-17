using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumInstallment;
using DomuWave.Services.Dto.CondominiumInstallment;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetInstallmentsByBudgetCommandConsumer
    : InMemoryConsumerBase<GetInstallmentsByBudgetCommand, IList<CondominiumInstallmentReadDto>>
{
    private readonly IUserService _userService;

    public GetInstallmentsByBudgetCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<IList<CondominiumInstallmentReadDto>> Consume(
        GetInstallmentsByBudgetCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
    {
        await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entities = await session.Query<CondominiumInstallment>()
            .Where(x => x.Budget.Id == command.BudgetId && !x.IsDeleted)
            .OrderBy(x => x.InstallmentNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dtos = entities.Select(e => e.ToReadDto()).ToList();
        await InstallmentFeeEnricher.EnrichAsync(session, dtos, cancellationToken).ConfigureAwait(false);
        return dtos;
    }
}
