using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccountsCategory;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteChartOfAccountsCategoryCommandConsumer
    : InMemoryConsumerBase<DeleteChartOfAccountsCategoryCommand, bool>
{
    private readonly IChartOfAccountsCategoryService _categoryService;
    private readonly IUserService                    _userService;

    public DeleteChartOfAccountsCategoryCommandConsumer(
        ISessionFactoryProvider          sessionFactoryProvider,
        IChartOfAccountsCategoryService  categoryService,
        IUserService                     userService) : base(sessionFactoryProvider)
    {
        _categoryService = categoryService;
        _userService     = userService;
    }

    protected override async Task<bool> Consume(
        DeleteChartOfAccountsCategoryCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var inUse = await session.Query<ChartOfAccounts>()
            .AnyAsync(x => x.Category != null
                        && x.Category.Id == command.Id
                        && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (inUse)
            throw new ValidatorException("Impossibile eliminare la categoria: è utilizzata da uno o più conti.");

        var deleted = await _categoryService
            .DeleteAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (!deleted)
            throw new NotFoundException("Categoria non trovata.");

        return true;
    }
}
