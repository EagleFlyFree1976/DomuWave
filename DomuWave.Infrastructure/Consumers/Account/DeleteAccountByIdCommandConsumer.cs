using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Book;

public class DeleteAccountByIdCommandConsumer : InMemoryConsumerBase<DeleteAccountByIdCommand, bool>
{
    private readonly IBookService _bookService;
    private readonly IAccountService _accountService;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;
    private readonly ICoreAuthorizationManager _authorizationManager;

    public DeleteAccountByIdCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IBookService bookService, IAccountService accountService, IUserService userService, IMediator mediator, ICoreAuthorizationManager authorizationManager) : base(sessionFactoryProvider)
    {
        _bookService = bookService;
        _accountService = accountService;
        _userService = userService;
        _mediator = mediator;
        _authorizationManager = authorizationManager;
    }

    protected override async Task<bool> Consume(DeleteAccountByIdCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);


        if (currentUser != null)
        {
            var bookId = evt.BookId;
            Models.Book book = book = await session.Query<Models.Book>().GetQueryable().FilterByOwner(currentUser)
                .Where(k => k.Id == bookId).FirstOrDefaultAsync(cancellationToken);

            if (book == null)
            {

                throw new NotFoundException("Elemento non trovato");
            }

            Models.Account account = await session.Query<Models.Account>().GetQueryable().FilterByBook(book)
                .Where(k => k.Id == evt.AccountId).FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (account == null)
            {
                throw new NotFoundException("Elemento non trovato");
            }

            await _accountService.Delete(account.Id, currentUser, cancellationToken).ConfigureAwait(
                false);

            return true;
        }

        return false;
    }
    }