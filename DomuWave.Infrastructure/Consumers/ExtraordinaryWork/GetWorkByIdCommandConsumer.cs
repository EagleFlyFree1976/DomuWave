using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ExtraordinaryWork;
using DomuWave.Services.Dto.ExtraordinaryWork;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.ExtraordinaryWork;

public class GetWorkByIdCommandConsumer
    : InMemoryConsumerBase<GetWorkByIdCommand, ExtraordinaryWorkReadDto>
{
    private readonly IExtraordinaryWorkService _workService;
    private readonly IUserService              _userService;

    public GetWorkByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IExtraordinaryWorkService workService,
        IUserService userService) : base(sessionFactoryProvider)
    { _workService = workService; _userService = userService; }

    protected override async Task<ExtraordinaryWorkReadDto> Consume(
        GetWorkByIdCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity = await _workService.GetByIdAsync(command.WorkId, currentUser, cancellationToken).ConfigureAwait(false);
        return entity?.ToReadDto();
    }
}
