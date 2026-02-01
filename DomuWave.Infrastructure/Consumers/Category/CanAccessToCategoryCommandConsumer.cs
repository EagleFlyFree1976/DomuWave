using DomuWave.Services.Command.Category;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

/// <summary>
///  Posso accedere alla categoria specificata?
/// </summary>

public class CanAccessToCategoryCommandConsumer : InMemoryConsumerBase<CanAccessToCategoryCommand, bool>
{
    private ICoreAuthorizationManager _authorizationManager;
    private IMediator _mediator;
    private IBookService _bookService;
    private ICategoryService _categoryService;
    private IUserService _userService;

    public CanAccessToCategoryCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ICoreAuthorizationManager authorizationManager, IMediator mediator, IBookService bookService, ICategoryService categoryService, IUserService userService) : base(sessionFactoryProvider)
    {
        _authorizationManager = authorizationManager;
        _mediator = mediator;
        _bookService = bookService;
        _categoryService = categoryService;
        _userService = userService;
    }

    protected override async Task<bool> Consume(CanAccessToCategoryCommand evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var book = await _bookService.GetById(evt.BookId, cancellationToken).ConfigureAwait(false);

        if (book == null)
            return false;
        if (book.OwnerId == currentUser.Id)
        {
            if (evt.CategoryId.HasValue)
            {
                var category = await _categoryService
                    .GetById(evt.CategoryId.Value, currentUser, cancellationToken).ConfigureAwait(false);

                if (category == null)
                    return false;
                book = category.Book;
                if (book.OwnerId == currentUser.Id)
                    return true;
                else if (book.IsSystem)
                {
                    return await _authorizationManager
                        .CanAction(currentUser, AuthorizationKeys.Admin, AuthorizationKeys.DomuWaveModule,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            return true;
        }
        if (book.IsSystem)
        {
            return await _authorizationManager
                .CanAction(currentUser, AuthorizationKeys.Admin, AuthorizationKeys.DomuWaveModule,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        return false;
    }
}