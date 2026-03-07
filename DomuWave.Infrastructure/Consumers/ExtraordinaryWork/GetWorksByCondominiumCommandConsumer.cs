using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ExtraordinaryWork;
using DomuWave.Services.Dto.ExtraordinaryWork;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.ExtraordinaryWork;

public class GetWorksByCondominiumCommandConsumer
    : InMemoryConsumerBase<GetWorksByCondominiumCommand, IList<ExtraordinaryWorkReadDto>>
{
    private readonly IExtraordinaryWorkService _workService;
    private readonly IUserService              _userService;

    public GetWorksByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IExtraordinaryWorkService workService,
        IUserService userService) : base(sessionFactoryProvider)
    { _workService = workService; _userService = userService; }

    protected override async Task<IList<ExtraordinaryWorkReadDto>> Consume(
        GetWorksByCondominiumCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var list = await _workService.GetByCondominiumIdAsync(command.CondominiumId, currentUser, cancellationToken).ConfigureAwait(false);
        return list.Select(x => x.ToReadDto()).ToList();
    }
}
