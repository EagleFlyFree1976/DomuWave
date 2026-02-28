using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetPagedCondominiumsCommandConsumer : InMemoryConsumerBase<GetPagedCondominiumsCommand, object>
{
    private readonly ICondominiumService _condominiumService;
    private readonly IUserService _userService;

    public GetPagedCondominiumsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService condominiumService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _userService = userService;
    }

    protected override async Task<object> Consume(
        GetPagedCondominiumsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var (items, total) = await _condominiumService
            .GetPagedAsync(x => !x.IsDeleted, command.Page, command.PageSize,
                x => x.Name!, command.Ascending, currentUser, cancellationToken)
            .ConfigureAwait(false);

        var dtos = items.Select(x => x.ToReadDto()).ToList();
        return new { items = dtos, total, command.Page, command.PageSize };
    }
}
