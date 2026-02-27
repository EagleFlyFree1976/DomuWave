using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.MillesimalTable;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetMillesimalTablesByCondominiumCommandConsumer : InMemoryConsumerBase<GetMillesimalTablesByCondominiumCommand, IList<MillesimalTable>>
{
    private readonly IMillesimalTableService _millesimalTableService;
    private readonly IUserService _userService;

    public GetMillesimalTablesByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IMillesimalTableService millesimalTableService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _millesimalTableService = millesimalTableService;
        _userService = userService;
    }

    protected override async Task<IList<MillesimalTable>> Consume(
        GetMillesimalTablesByCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _millesimalTableService
            .GetByCondominiumIdAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
