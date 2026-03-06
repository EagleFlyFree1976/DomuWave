using CPQ.Core.Consumers;
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

        var condominium = await session.Query<Condominium>()
            .FirstOrDefaultAsync(x => x.Id == dto.CondominiumId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var account = await session.Query<ChartOfAccounts>()
            .FirstOrDefaultAsync(x => x.Id == dto.AccountId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var millesimalTable = await session.Query<MillesimalTable>()
            .FirstOrDefaultAsync(x => x.Id == dto.MillesimalTableId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        Supplier? supplier = null;
        if (dto.SupplierId.HasValue)
            supplier = await session.Query<Supplier>()
                .FirstOrDefaultAsync(x => x.Id == dto.SupplierId.Value && !x.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

        var expenseType = session.Load<ExpenseType>(dto.ExpenseTypeId);

        var entity  = dto.ToEntity(condominium, account, millesimalTable, supplier, expenseType);
        var created = await _expenseService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
