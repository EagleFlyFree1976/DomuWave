using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Category;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class FindCategoryCommandConsumer : InMemoryConsumerBase<FindCategoryCommand, IList<CategoryReadDto>>
{
    private IUserService _userService;
    private ICategoryService _CategoryService;

    public FindCategoryCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, ICategoryService CategoryService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _CategoryService = CategoryService;
    }

    protected override async Task<IList<CategoryReadDto>> Consume(FindCategoryCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        IList<Models.Category> all = await _CategoryService.Find(evt.BookId, evt.Q, true, currentUser, cancellationToken).ConfigureAwait(false);

        return all.Select(j => j.ToDto()).ToList();

    }
}