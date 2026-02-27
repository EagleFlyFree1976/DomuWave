using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumFee;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class RecordFeePaymentCommandConsumer : InMemoryConsumerBase<RecordFeePaymentCommand, bool>
{
    private readonly ICondominiumFeeService _condominiumFeeService;
    private readonly IUserService _userService;

    public RecordFeePaymentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumFeeService condominiumFeeService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumFeeService = condominiumFeeService;
        _userService = userService;
    }

    protected override async Task<bool> Consume(
        RecordFeePaymentCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _condominiumFeeService
            .RecordPaymentAsync(command.FeeId, command.Amount, command.PaymentDate,
                command.PaymentMethod, currentUser.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
