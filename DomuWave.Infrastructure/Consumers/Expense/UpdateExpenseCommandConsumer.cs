using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Command.Expense;
using DomuWave.Services.Dto.Expense;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateExpenseCommandConsumer : InMemoryConsumerBase<UpdateExpenseCommand, ExpenseReadDto>
{
    private readonly IExpenseService _expenseService;
    private readonly IUserService    _userService;
    private readonly IMediator       _mediator;

    public UpdateExpenseCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IExpenseService         expenseService,
        IUserService            userService,
        IMediator               mediator) : base(sessionFactoryProvider)
    {
        _expenseService = expenseService;
        _userService    = userService;
        _mediator       = mediator;
    }

    protected override async Task<ExpenseReadDto> Consume(
        UpdateExpenseCommand command,
        IMediationContext    mediationContext,
        CancellationToken    cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _expenseService
            .GetByIdAsync(command.ExpenseId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null)
            throw new NotFoundException("Spesa non trovata.");

        var dto = command.Dto;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidatorException("La descrizione della spesa è obbligatoria.");
        if (dto.ExpenseTypeId <= 0)
            throw new ValidatorException("Il tipo spesa è obbligatorio.");
        if (dto.DocumentDate == default)
            throw new ValidatorException("La data documento è obbligatoria.");
        if (dto.RegistrationDate == default)
            throw new ValidatorException("La data di registrazione è obbligatoria.");
        if (dto.GrossAmount <= 0)
            throw new ValidatorException("L'importo lordo deve essere maggiore di zero.");
        if (dto.VatAmount < 0)
            throw new ValidatorException("L'importo IVA non può essere negativo.");
        if (dto.VatAmount > dto.GrossAmount)
            throw new ValidatorException("L'importo IVA non può essere superiore all'importo lordo.");

        var account = await session.Query<ChartOfAccounts>()
            .FirstOrDefaultAsync(x => x.Id == dto.AccountId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (account == null)
            throw new NotFoundException("Conto del piano dei conti non trovato.");

        var millesimalTable = await session.Query<MillesimalTable>()
            .FirstOrDefaultAsync(x => x.Id == dto.MillesimalTableId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (millesimalTable == null)
            throw new NotFoundException("Tabella millesimale non trovata.");
        if (!millesimalTable.IsEnabled && millesimalTable.Id != entity.MillesimalTable?.Id)
            throw new ValidatorException("La tabella millesimale selezionata è disabilitata.");

        Supplier? supplier = null;
        if (dto.SupplierId.HasValue)
        {
            supplier = await session.Query<Supplier>()
                .FirstOrDefaultAsync(x => x.Id == dto.SupplierId.Value && !x.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (supplier == null)
                throw new NotFoundException("Fornitore non trovato.");
        }

        var expenseType       = session.Load<ExpenseType>(dto.ExpenseTypeId);
        var paymentStatus     = session.Load<ExpensePaymentStatus>(dto.PaymentStatusId > 0 ? dto.PaymentStatusId : entity.PaymentStatus?.Id ?? ExpensePaymentStatus.DaPagare);
        var chargeabilityType = session.Load<ChargeabilityType>(dto.ChargeabilityTypeId);

        var condominiumId = entity.Condominium?.Id ?? 0;
        var fiscalYearId  = entity.FiscalYear?.Id  ?? 0;

        entity.ApplyUpdate(dto, account, millesimalTable, supplier, expenseType, paymentStatus, chargeabilityType);
        var updated = await _expenseService
            .UpdateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        var consuntivo = await session.Query<Budget>()
            .Where(b => b.Condominium.Id == condominiumId
                     && b.FiscalYear.Id  == fiscalYearId
                     && b.Type           == BudgetType.Consuntivo
                     && b.Status.Id      != BudgetStatus.Closed
                     && !b.IsDeleted)
            .Select(b => new { b.Id })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (consuntivo != null)
            await _mediator
                .GetResponse(new RecalculateBudgetItemsCommand(command.CurrentUserId, consuntivo.Id), cancellationToken)
                .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
