using DomuWave.Services.Command;
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

public class FindAccountsCommandConsumer : InMemoryConsumerBase<FindAccountsCommand, IList<AccountReadDto>>
{
    private readonly IBookService _bookService;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;
    private readonly ICoreAuthorizationManager _authorizationManager;

    public FindAccountsCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IBookService bookService, IUserService userService, ICoreAuthorizationManager authorizationManager, IMediator mediator) : base(sessionFactoryProvider)
    {
        _bookService = bookService;
        _userService = userService;
        _authorizationManager = authorizationManager;
        _mediator = mediator;
    }

    protected override async Task<IList<AccountReadDto>> Consume(FindAccountsCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);
            
       
        if (currentUser != null)
        {
            Models.Book book = null;
            var bookId = evt.BookId;
            if (!evt.BookId.HasValue)
            {
                BookReadDto? defaultBook = await _mediator.GetResponse(new GetPrimaryOrCreateBookCommand(evt.CurrentUserId)
                {
                    CurrentUserId = currentUser.Id,
                    OwnerId = currentUser.Id
                }, cancellationToken).ConfigureAwait(false);

                bookId = defaultBook.Id;
            }

            if (bookId.HasValue)
            {
                book = await session.Query<Models.Book>().GetQueryable().FilterByOwner(currentUser)
                    .Where(k => k.Id == bookId.Value).FirstOrDefaultAsync(cancellationToken);
                
            }

            var allAccounts = await session.Query<Models.Account>().GetQueryable().FilterByBook(book).ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return allAccounts.Select(j => j.ToDto()).ToList();
        }

        return null;
    }
}