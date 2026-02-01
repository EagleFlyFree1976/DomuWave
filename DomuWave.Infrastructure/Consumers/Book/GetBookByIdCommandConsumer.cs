using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Book;

public class GetBookByIdCommandConsumer : InMemoryConsumerBase<GetBookByIdCommand, BookReadDto>
{
    private readonly IBookService _bookService;
    private readonly IUserService _userService;

    public GetBookByIdCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IBookService bookService, IUserService userService) : base(sessionFactoryProvider)
    {
        _bookService = bookService;
        _userService = userService;
    }

    protected override async Task<BookReadDto> Consume(GetBookByIdCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        Models.Book x = await _bookService.GetById(evt.BookId, currentUser, cancellationToken).ConfigureAwait(false);
        if (x == null)
        {
            throw new NotFoundException("Elemento non trovato");
        }

        return x.ToDto();

    }
}