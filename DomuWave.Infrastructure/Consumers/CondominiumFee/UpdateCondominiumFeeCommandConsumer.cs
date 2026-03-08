using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumFee;
using DomuWave.Services.Dto.CondominiumFee;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateCondominiumFeeCommandConsumer : InMemoryConsumerBase<UpdateCondominiumFeeCommand, CondominiumFeeReadDto>
{
    private readonly ICondominiumFeeService _condominiumFeeService;
    private readonly IUserService           _userService;

    public UpdateCondominiumFeeCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumFeeService  condominiumFeeService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _condominiumFeeService = condominiumFeeService;
        _userService           = userService;
    }

    protected override async Task<CondominiumFeeReadDto> Consume(
        UpdateCondominiumFeeCommand command,
        IMediationContext            mediationContext,
        CancellationToken            cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var fee = await _condominiumFeeService
            .GetByIdAsync(command.FeeId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (fee == null) return null;

        fee.AmountDue = command.Dto.AmountDue;
        fee.Balance   = fee.AmountDue - fee.AmountPaid;
        fee.Notes     = command.Dto.Notes;

        var updated = await _condominiumFeeService
            .UpdateAsync(fee, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
