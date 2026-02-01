using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Book;

public class SetBookaAsPrimaryConsumer : InMemoryConsumerBase<SetBookaAsPrimaryCommand, bool>
{
    private readonly IBookService _bookService;
    private readonly IUserService _userService;

    public SetBookaAsPrimaryConsumer(ISessionFactoryProvider sessionFactoryProvider, IBookService bookService, IUserService userService) : base(sessionFactoryProvider)
    {
        _bookService = bookService;
        _userService = userService;
    }

    protected override async Task<bool> Consume(SetBookaAsPrimaryCommand @event, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(@event.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        Models.Book targetBook = await _bookService.GetById(@event.BookId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (targetBook == null)
        {
            throw new NotFoundException("Elemento non disponibile");
        }

        if (targetBook.IsPrimary)
        {
            throw new ValidatorException("Il book specificato risulta già primario");
        }

        Models.Book oldPrimaryBook =
            await _bookService.GetPrimaryBook(currentUser, cancellationToken).ConfigureAwait(false);

        if (oldPrimaryBook != null)
        {
            oldPrimaryBook.IsPrimary = false;
            var editDto = oldPrimaryBook.ToEditDto();
            await _bookService.Update(oldPrimaryBook.Id, editDto, currentUser, cancellationToken)
                .ConfigureAwait(false);

            
        }

        targetBook.IsPrimary = true;
        var editTargetBookDto = targetBook.ToEditDto();
        await _bookService.Update(targetBook.Id, editTargetBookDto, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return true;
        return false;
    }
}