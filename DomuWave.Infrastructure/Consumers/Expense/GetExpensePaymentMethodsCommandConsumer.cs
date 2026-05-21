using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Command.Expense;
using DomuWave.Services.Dto.Expense;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetExpensePaymentMethodsCommandConsumer
    : InMemoryConsumerBase<GetExpensePaymentMethodsCommand, IList<ExpensePaymentMethodDto>>
{
    public GetExpensePaymentMethodsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider) : base(sessionFactoryProvider) { }

    protected override async Task<IList<ExpensePaymentMethodDto>> Consume(
        GetExpensePaymentMethodsCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
        => await session.Query<ExpensePaymentMethod>()
            .OrderBy(x => x.Id)
            .Select(x => new ExpensePaymentMethodDto { Id = x.Id, Name = x.Name })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
