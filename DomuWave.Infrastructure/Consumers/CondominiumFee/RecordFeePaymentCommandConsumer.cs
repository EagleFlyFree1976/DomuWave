using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumFee;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Interfaces;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class RecordFeePaymentCommandConsumer : InMemoryConsumerBase<RecordFeePaymentCommand, bool>
{
    private readonly ICondominiumFeeService _condominiumFeeService;
    private readonly IUserService           _userService;
    private readonly IMediator              _mediator;

    public RecordFeePaymentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumFeeService  condominiumFeeService,
        IUserService            userService,
        IMediator               mediator) : base(sessionFactoryProvider)
    {
        _condominiumFeeService = condominiumFeeService;
        _userService           = userService;
        _mediator              = mediator;
    }

    protected override async Task<bool> Consume(
        RecordFeePaymentCommand command,
        IMediationContext       mediationContext,
        CancellationToken       cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var result = await _condominiumFeeService
            .RecordPaymentAsync(command.FeeId, command.Amount, command.PaymentDate,
                command.PaymentMethod, currentUser.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Trigger consuntivo budget recalculation
        var feeInfo = await session.Query<CondominiumFee>()
            .Where(f => f.Id == command.FeeId && !f.IsDeleted)
            .Select(f => new { CondominiumId = f.Installment.Condominium.Id, FiscalYearId = f.Installment.FiscalYear.Id })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (feeInfo != null)
        {
            var budget = await session.Query<Budget>()
                .Where(b => b.Condominium.Id == feeInfo.CondominiumId
                         && b.FiscalYear.Id  == feeInfo.FiscalYearId
                         && b.Type           == BudgetType.Consuntivo
                         && b.Status.Id      != BudgetStatus.Closed
                         && !b.IsDeleted)
                .Select(b => new { b.Id })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (budget != null)
                await _mediator.GetResponse(new RecalculateBudgetItemsCommand(command.CurrentUserId, budget.Id), cancellationToken)
                    .ConfigureAwait(false);
        }

        return result;
    }
}
