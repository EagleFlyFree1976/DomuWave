using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetCondominiumByCodeCommandConsumer : InMemoryConsumerBase<GetCondominiumByCodeCommand, Condominium>
{
    private readonly ICondominiumService _condominiumService;
    private readonly IUserService _userService;

    public GetCondominiumByCodeCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService condominiumService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _userService = userService;
    }

    protected override async Task<Condominium> Consume(
        GetCondominiumByCodeCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _condominiumService
            .GetByCodeAsync(command.TenantId, command.Code, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
