 
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.MillesimalTable;
      
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetMillesimalTableByIdCommandConsumer : InMemoryConsumerBase<GetMillesimalTableByIdCommand, Models.MillesimalTable>
{
    private readonly IMillesimalTableService _millesimalTableService;
    private readonly IUserService _userService;

    public GetMillesimalTableByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IMillesimalTableService millesimalTableService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _millesimalTableService = millesimalTableService;
        _userService = userService;
    }

    protected override async Task<Models.MillesimalTable> Consume(
        GetMillesimalTableByIdCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _millesimalTableService
            .GetByIdAsync(command.TableId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
