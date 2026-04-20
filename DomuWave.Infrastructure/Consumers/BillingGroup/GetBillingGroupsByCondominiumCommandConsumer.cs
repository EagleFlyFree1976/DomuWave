using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.BillingGroup;
using DomuWave.Services.Dto.BillingGroup;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetBillingGroupsByCondominiumCommandConsumer
    : InMemoryConsumerBase<GetBillingGroupsByCondominiumCommand, IList<BillingGroupReadDto>>
{
    private readonly IUserService _userService;

    public GetBillingGroupsByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<IList<BillingGroupReadDto>> Consume(
        GetBillingGroupsByCondominiumCommand command,
        IMediationContext                    mediationContext,
        CancellationToken                   cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var groups = await session.Query<DomuWave.Services.Models.BillingGroup>()
            .Where(g => g.Condominium.Id == command.CondominiumId && !g.IsDeleted)
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Eager-load units for each group
        foreach (var g in groups)
            await NHibernate.NHibernateUtil.InitializeAsync(g.Units, cancellationToken)
                .ConfigureAwait(false);

        return groups.Select(g => g.ToReadDto()).ToList();
    }
}
