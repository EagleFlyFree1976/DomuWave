using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitOwner;
using DomuWave.Services.Dto.UnitOwner;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetUnitOwnerByIdCommandConsumer : InMemoryConsumerBase<GetUnitOwnerByIdCommand, UnitOwnerReadDto>
{
    private readonly IUnitOwnerService  _unitOwnerService;
    private readonly IUserService       _userService;

    public GetUnitOwnerByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitOwnerService unitOwnerService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _unitOwnerService = unitOwnerService;
        _userService      = userService;
    }

    protected override async Task<UnitOwnerReadDto> Consume(
        GetUnitOwnerByIdCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var owner = await _unitOwnerService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return owner?.ToReadDto();
    }
}
