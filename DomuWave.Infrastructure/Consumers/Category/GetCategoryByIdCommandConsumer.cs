using DomuWave.Services.Command.Category;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Category;
using DomuWave.Services.Models.Dto.Category;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class GetCategoryByIdCommandConsumer : InMemoryConsumerBase<GetCategoryByIdCommand, CategoryReadDto>
{
    private IUserService _userService;
    private ICategoryService _CategoryService;
    private ICoreAuthorizationManager _authorizationManager;
    private IMediator _mediator;
    public GetCategoryByIdCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, ICategoryService CategoryService, ICoreAuthorizationManager authorizationManager, IMediator mediator) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _CategoryService = CategoryService;
        _authorizationManager = authorizationManager;
        _mediator = mediator;
    }

    protected override async Task<CategoryReadDto> Consume(GetCategoryByIdCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        // posso accedere al bookid specificato?
        CanAccessToCategoryCommand canAccessToCategoryCommand =
            new CanAccessToCategoryCommand(evt.CategoryId, evt.CurrentUserId, evt.BookId);
        bool canAccess = await _mediator.GetResponse(canAccessToCategoryCommand).ConfigureAwait(false);
        if (!canAccess)
        {
            throw new UserNotAuthorizedException("Non hai accesso alla risorsa richiesta");
        }


        Models.Category x = await _CategoryService.GetById(evt.CategoryId, currentUser, cancellationToken).ConfigureAwait(false);
        if (x == null)
            throw new NotFoundException("Elemento non trovato");




        return x.ToDto();

    }
}