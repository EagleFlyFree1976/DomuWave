using DomuWave.Services.Interfaces;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class DeleteCategoryByIdCommandConsumer : InMemoryConsumerBase<DeleteCategoryByIdCommand, bool>
{
    private IUserService _userService;
    private ICategoryService _CategoryService;

    public DeleteCategoryByIdCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, ICategoryService CategoryService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _CategoryService = CategoryService;
    }

    protected override async Task<bool> Consume(DeleteCategoryByIdCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        await _CategoryService.Delete(evt.CategoryId, evt.BookId, currentUser, cancellationToken).ConfigureAwait(false);
        return true;

    }
}