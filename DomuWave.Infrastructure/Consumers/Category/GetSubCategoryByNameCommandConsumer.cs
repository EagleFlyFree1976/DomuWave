using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Category;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class GetSubCategoryByNameCommandConsumer : InMemoryConsumerBase<GetSubCategoryByNameCommand, CategoryReadDto>
{
    private IUserService _userService;
    private ICategoryService _CategoryService;
    private ICoreAuthorizationManager _authorizationManager;
    private IMediator _mediator;

    public GetSubCategoryByNameCommandConsumer(ISessionFactoryProvider sessionFactoryProvider) : base(sessionFactoryProvider)
    {
    }

    protected override async Task<CategoryReadDto> Consume(GetSubCategoryByNameCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var category = await session.Query<Models.Category>()
            .Where(k => k.Book.Id == evt.BookId && k.Name == evt.Name && k.ParentCategory != null && k.ParentCategory.Id == evt.ParentCategoryId)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);


        if (category == null)
        {
            return null;
        }
        else
        {
            return category.ToDto();
        }
    }
}