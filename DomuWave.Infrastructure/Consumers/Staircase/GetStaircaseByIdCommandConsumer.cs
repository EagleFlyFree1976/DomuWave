using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Staircase;
using DomuWave.Services.Dto.Staircase;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetStaircaseByIdCommandConsumer : InMemoryConsumerBase<GetStaircaseByIdCommand, StaircaseReadDto>
{
    private readonly IStaircaseService _staircaseService;
    private readonly IUserService      _userService;

    public GetStaircaseByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IStaircaseService staircaseService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _staircaseService = staircaseService;
        _userService      = userService;
    }

    protected override async Task<StaircaseReadDto> Consume(
        GetStaircaseByIdCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _staircaseService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return entity?.ToReadDto();
    }
}
