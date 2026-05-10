using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Staircase;
using DomuWave.Services.Dto.Staircase;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetStaircasesByCondominiumCommandConsumer : InMemoryConsumerBase<GetStaircasesByCondominiumCommand, IList<StaircaseReadDto>>
{
    private readonly IStaircaseService _staircaseService;
    private readonly IUserService      _userService;

    public GetStaircasesByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IStaircaseService staircaseService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _staircaseService = staircaseService;
        _userService      = userService;
    }

    protected override async Task<IList<StaircaseReadDto>> Consume(
        GetStaircasesByCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var items = await _staircaseService
            .GetByCondominiumAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return items.Select(x => x.ToReadDto()).ToList();
    }
}
