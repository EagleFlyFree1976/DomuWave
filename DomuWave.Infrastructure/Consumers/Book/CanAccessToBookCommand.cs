using DomuWave.Services.Command.Book;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Book;

public class CanAccessToBookCommandConsumer : InMemoryConsumerBase<CanAccessToBookCommand, bool>
{
    private IBookService _bookService;
    private IUserService _userService;
    private ICoreAuthorizationManager _authorizationManager;
    public CanAccessToBookCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IBookService bookService, IUserService userService, ICoreAuthorizationManager authorizationManager) : base(sessionFactoryProvider)
    {
        _bookService = bookService;
        _userService = userService;
        _authorizationManager = authorizationManager;
    }

    protected override async Task<bool> Consume(CanAccessToBookCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetById(evt.BookId, cancellationToken).ConfigureAwait(false);

        if (book == null)
            return false;
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (book.OwnerId == currentUser.Id)
        {
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