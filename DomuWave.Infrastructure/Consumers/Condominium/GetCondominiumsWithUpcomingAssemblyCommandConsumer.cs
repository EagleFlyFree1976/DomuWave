using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetCondominiumsWithUpcomingAssemblyCommandConsumer : InMemoryConsumerBase<GetCondominiumsWithUpcomingAssemblyCommand, IList<CondominiumReadDto>>
{
    private readonly ICondominiumService _condominiumService;
    private readonly IUserService _userService;

    public GetCondominiumsWithUpcomingAssemblyCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService condominiumService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _userService = userService;
    }

    protected override async Task<IList<CondominiumReadDto>> Consume(
        GetCondominiumsWithUpcomingAssemblyCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var result = await _condominiumService
            .GetCondominiumsWithUpcomingAssemblyAsync(command.TenantId, command.DaysAhead, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return result.Select(x => x.ToReadDto()).ToList();
    }
}
