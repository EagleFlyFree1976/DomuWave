using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
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

    public CreateExpenseCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IExpenseService         expenseService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _expenseService = expenseService;
        _userService    = userService;
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
        if (dto.GrossAmount <= 0)
            throw new ValidatorException("L'importo lordo deve essere maggiore di zero.");
        if (dto.VatAmount < 0)
            throw new ValidatorException("L'importo IVA non può essere negativo.");
        if (dto.VatAmount > dto.GrossAmount)
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

        var expenseType       = session.Load<ExpenseType>(dto.ExpenseTypeId);
        var paymentStatus     = session.Load<ExpensePaymentStatus>(ExpensePaymentStatus.DaPagare);
        var chargeabilityType = session.Load<ChargeabilityType>(dto.ChargeabilityTypeId);

        var entity  = dto.ToEntity(condominium, fiscalYear, account, millesimalTable, supplier, expenseType, paymentStatus, chargeabilityType);
        var created = await _expenseService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
