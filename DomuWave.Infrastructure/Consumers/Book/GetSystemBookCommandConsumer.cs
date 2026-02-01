using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Book;

public class GetSystemBookCommandConsumer : InMemoryConsumerBase<GetSystemBookCommand, BookReadDto>
{
    private IBookService _bookService;
    private IUserService _userService;
    public GetSystemBookCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IBookService bookService, IUserService userService) : base(sessionFactoryProvider)
    {
        _bookService = bookService;
        _userService = userService;
    }

    protected override async Task<BookReadDto> Consume(GetSystemBookCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);
        return (await _bookService.GetSystem(currentUser, cancellationToken).ConfigureAwait(false)).ToDto();
    }
}