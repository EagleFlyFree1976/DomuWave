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

public class CreateExpenseCommandConsumer : InMemoryConsumerBase<CreateExpenseCommand, ExpenseReadDto>
{
    private readonly IExpenseService _expenseService;
    private readonly IUserService    _userService;
    private readonly IMediator       _mediator;

    public CreateExpenseCommandConsumer(
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
        CreateExpenseCommand command,
        IMediationContext    mediationContext,
        CancellationToken    cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var dto = command.Dto;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidatorException("La descrizione della spesa è obbligatoria.");
        if (dto.ExpenseTypeId <= 0)
            throw new ValidatorException("Il tipo spesa è obbligatorio.");
        if (dto.DocumentDate == default)
            throw new ValidatorException("La data documento è obbligatoria.");
        if (dto.RegistrationDate == default)
            throw new ValidatorException("La data di registrazione è obbligatoria.");
        var calcGross = dto.TaxableAmount + dto.TaxableAmountVatExempt + dto.VatAmount + dto.PensionFund + dto.StampDuty - dto.WithholdingTax;
        if (calcGross <= 0)
            throw new ValidatorException("L'importo lordo calcolato deve essere maggiore di zero.");
        if (dto.VatAmount < 0)
            throw new ValidatorException("L'importo IVA non può essere negativo.");
        if (dto.VatAmount > calcGross)
            throw new ValidatorException("L'importo IVA non può essere superiore all'importo lordo.");

        var condominium = await session.Query<Models.Condominium>()
            .FirstOrDefaultAsync(x => x.Id == dto.CondominiumId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (condominium == null)
            throw new NotFoundException("Condominio non trovato.");

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
        if (!millesimalTable.IsEnabled)
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

        FiscalYear? fiscalYear = null;
        if (dto.FiscalYearId.HasValue)
        {
            fiscalYear = await session.Query<FiscalYear>()
                .FirstOrDefaultAsync(x => x.Id == dto.FiscalYearId.Value && !x.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (fiscalYear == null)
                throw new NotFoundException("Esercizio fiscale non trovato.");
        }
        else
        {
            fiscalYear = await session.Query<FiscalYear>()
                .Where(x => x.Condominium.Id == dto.CondominiumId && !x.IsDeleted && x.Status.Id != FiscalYearStatus.Locked)
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            if (fiscalYear == null)
                throw new ValidatorException("Nessun esercizio fiscale attivo trovato per questo condominio.");
        }

        // Non è possibile registrare spese su esercizi in stato Bozza, Chiuso o Bloccato
        if (fiscalYear.Status?.Id == FiscalYearStatus.Draft)
            throw new ValidatorException(
                "Non è possibile registrare spese su un esercizio in stato Bozza: aprire prima l'esercizio.");
        if (fiscalYear.Status?.Id == FiscalYearStatus.Closed)
            throw new ValidatorException(
                "Non è possibile registrare spese su un esercizio chiuso.");
        if (fiscalYear.Status?.Id == FiscalYearStatus.Locked)
            throw new ValidatorException(
                "Non è possibile registrare spese su un esercizio bloccato.");

        var expenseType       = session.Load<ExpenseType>(dto.ExpenseTypeId);
        var paymentStatus     = session.Load<ExpensePaymentStatus>(ExpensePaymentStatus.DaPagare);
        var chargeabilityType = session.Load<ChargeabilityType>(dto.ChargeabilityTypeId);
        var paymentMethod     = dto.PaymentMethodId.HasValue
            ? session.Load<ExpensePaymentMethod>(dto.PaymentMethodId.Value)
            : null;

        var entity  = dto.ToEntity(condominium, fiscalYear, account, millesimalTable, supplier, expenseType, paymentStatus, paymentMethod, chargeabilityType);
        var created = await _expenseService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        await ExpenseAllocationHelper
            .RegenerateAllocationsAsync(session, created, currentUser, cancellationToken)
            .ConfigureAwait(false);

        var consuntivo = await session.Query<Budget>()
            .Where(b => b.Condominium.Id == dto.CondominiumId
                     && b.FiscalYear.Id  == fiscalYear.Id
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

        return created.ToReadDto();
    }
}
