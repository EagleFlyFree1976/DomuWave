using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Expense;
using DomuWave.Services.Dto.Expense;
using DomuWave.Services.Helper;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetPagedExpensesCommandConsumer
    : InMemoryConsumerBase<GetPagedExpensesCommand, PagedResult<ExpenseReadDto>>
{
    private readonly IUserService _userService;

    public GetPagedExpensesCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<PagedResult<ExpenseReadDto>> Consume(
        GetPagedExpensesCommand command,
        IMediationContext        mediationContext,
        CancellationToken       cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var query = session.Query<Expense>()
            .Where(e => e.Condominium.Id == command.CondominiumId
                     && e.Tenant.Id      == command.TenantId
                     && !e.IsDeleted);

        if (command.ExpenseTypeId.HasValue)
            query = query.Where(e => e.ExpenseType.Id == command.ExpenseTypeId.Value);

        if (command.PaymentStatusId.HasValue)
            query = query.Where(e => e.PaymentStatus.Id == command.PaymentStatusId.Value);

        if (command.SupplierId.HasValue)
            query = query.Where(e => e.Supplier.Id == command.SupplierId.Value);

        // Filtro per esercizio fiscale: si basa sull'ASSEGNAZIONE della spesa all'esercizio
        // (Expense.FiscalYear), non sull'intervallo di date dell'esercizio. Così le spese con
        // data di registrazione fuori dall'intervallo ma assegnate all'esercizio sono incluse.
        if (command.FiscalYearId.HasValue)
            query = query.Where(e => e.FiscalYear.Id == command.FiscalYearId.Value);

        if (command.DateFrom.HasValue)
            query = query.Where(e => e.DocumentDate >= command.DateFrom.Value);

        if (command.DateTo.HasValue)
            query = query.Where(e => e.DocumentDate <= command.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(command.Search))
        {
            var s = command.Search.ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(s)
                                  || e.DocumentNumber.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        query = command.SortField?.ToLower() switch
        {
            "documentdate"  => command.Asc ? query.OrderBy(e => e.DocumentDate)  : query.OrderByDescending(e => e.DocumentDate),
            "grossamount"   => command.Asc ? query.OrderBy(e => e.GrossAmount)   : query.OrderByDescending(e => e.GrossAmount),
            "suppliername"  => command.Asc ? query.OrderBy(e => e.Supplier.Name) : query.OrderByDescending(e => e.Supplier.Name),
            _               => query.OrderByDescending(e => e.DocumentDate),
        };

        var items = await query
            .Skip((command.PageNumber - 1) * command.PageSize)
            .Take(command.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<ExpenseReadDto>
        {
            PageNumber = command.PageNumber,
            PageSize   = command.PageSize,
            TotalCount = totalCount,
            Items      = items.Select(e => e.ToReadDto()).ToList(),
        };
    }
}
