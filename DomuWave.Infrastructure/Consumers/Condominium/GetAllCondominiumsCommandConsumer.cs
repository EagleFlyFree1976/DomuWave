using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetAllCondominiumsCommandConsumer : InMemoryConsumerBase<GetAllCondominiumsCommand, IList<Condominium>>
{
    private readonly ICondominiumService _condominiumService;
    private readonly IUserService _userService;

    public GetAllCondominiumsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService condominiumService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _userService = userService;
    }

    protected override async Task<IList<Condominium>> Consume(
        GetAllCondominiumsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var result = await _condominiumService
            .GetByTenantIdAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
