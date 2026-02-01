using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Consumers;
using CPQ.Core.DTO;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Book;


public class GetAccountByIdCommandConsumer : InMemoryConsumerBase<GetAccountByIdCommand, AccountReadDto>
{
    private readonly IBookService _bookService;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;
    private readonly ICoreAuthorizationManager _authorizationManager;

    public GetAccountByIdCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IBookService bookService, IUserService userService, ICoreAuthorizationManager authorizationManager, IMediator mediator) : base(sessionFactoryProvider)
    {
        _bookService = bookService;
        _userService = userService;
        _authorizationManager = authorizationManager;
        _mediator = mediator;
    }

    protected override async Task<AccountReadDto> Consume(GetAccountByIdCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);


        if (currentUser != null)
        {
            Models.Book book = null;
            var bookId = evt.BookId;

            book = await session.Query<Models.Book>().GetQueryable().FilterByOwner(currentUser)
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

            
            return account.ToDto();
        }
        else
        {
            throw new UserNotAuthorizedException("Utente non autorizzato");
        }

        return null;
    }
}