using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetActiveCondominiumsCommandConsumer : InMemoryConsumerBase<GetActiveCondominiumsCommand, IList<CondominiumReadDto>>
{
    private readonly ICondominiumService _condominiumService;
    private readonly IUserService _userService;

    public GetActiveCondominiumsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService condominiumService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _userService = userService;
    }

    protected override async Task<IList<CondominiumReadDto>> Consume(
        GetActiveCondominiumsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var result = await _condominiumService
            .GetActiveCondominiumsAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return result.Select(x => x.ToReadDto()).ToList();
    }
}
