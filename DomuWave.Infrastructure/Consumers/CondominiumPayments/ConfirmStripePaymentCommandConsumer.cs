using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Command.CondominiumPayments;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

/// <summary>
/// Conferma un pagamento Stripe ricevuto dal webhook: riconcilia la quota, crea il Payment
/// e ricalcola il consuntivo. Idempotente rispetto al PaymentIntent id (Payment.BankReference):
/// se l'evento viene ritrasmesso, non duplica nulla.
/// </summary>
public class ConfirmStripePaymentCommandConsumer
    : InMemoryConsumerBase<ConfirmStripePaymentCommand, bool>
{
    private readonly ICondominiumFeeService _condominiumFeeService;
    private readonly IUserService           _userService;
    private readonly IMediator              _mediator;

    public ConfirmStripePaymentCommandConsumer(
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
        ConfirmStripePaymentCommand command,
        IMediationContext           mediationContext,
        CancellationToken           cancellationToken)
    {
        // ── Idempotenza: l'evento Stripe può arrivare più volte. ──────────────────
        var alreadyProcessed = await session.Query<Payment>()
            .AnyAsync(p => p.BankReference == command.ProviderTransactionId && !p.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (alreadyProcessed)
            return true;

        var fee = await session.Query<CondominiumFee>()
            .FirstOrDefaultAsync(f => f.Id == command.FeeId && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Quota non trovata per la conferma del pagamento.");

        var installment = fee.Installment
            ?? throw new NotFoundException("Rata della quota non trovata.");
        var condominium = installment.Condominium;
        var fiscalYear  = installment.FiscalYear;

        // L'attore del pagamento è il condòmino pagante: lo usiamo per il Trace.
        var payer = await _userService
            .GetByIdAsync((int)command.PayerUserId, cancellationToken).ConfigureAwait(false);

        var now = DateTime.UtcNow;

        // ── 1. Riconcilia la quota (riuso della logica esistente). ────────────────
        await _condominiumFeeService.RecordPaymentAsync(
            fee.Id, command.Amount, now, "Stripe", command.PayerUserId, payer, cancellationToken)
            .ConfigureAwait(false);

        // ── 2. Crea il Payment riconciliato (traccia contabile dell'incasso). ─────
        var payment = new Payment
        {
            Tenant               = fee.Tenant,
            FiscalYear           = fiscalYear,
            Condominium          = condominium,
            Unit                 = fee.Unit,
            Fee                  = fee,
            UserId               = command.PayerUserId,
            PaymentDate          = now,
            RegistrationDate     = now,
            Amount               = command.Amount,
            PaymentMethod        = "Stripe",
            BankReference        = command.ProviderTransactionId,
            ReconciliationStatus = "Reconciled",
            Notes                = "Pagamento online Stripe",
        };
        payment.Trace(payer);
        await session.SaveAsync(payment, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        // ── 3. Ricalcola il consuntivo aperto, se presente (come RecordFeePayment). ─
        var budget = await session.Query<Budget>()
            .Where(b => b.Condominium.Id == condominium.Id
                     && b.FiscalYear.Id  == fiscalYear.Id
                     && b.Type           == BudgetType.Consuntivo
                     && b.Status.Id      != BudgetStatus.Closed
                     && !b.IsDeleted)
            .Select(b => new { b.Id })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (budget != null)
            await _mediator.GetResponse(
                new RecalculateBudgetItemsCommand(payer.Id, budget.Id), cancellationToken)
                .ConfigureAwait(false);

        return true;
    }
}
