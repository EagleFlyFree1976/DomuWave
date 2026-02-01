using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Consumers;
using CPQ.Core.DTO;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Book;

public class FindBooksCommandConsumer : InMemoryConsumerBase<FindBooksCommand, IList<BookReadDto>>
{
    private readonly IBookService _bookService;
    private readonly IUserService _userService;
    
    private readonly ICoreAuthorizationManager _authorizationManager;

    public FindBooksCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IBookService bookService, IUserService userService, ICoreAuthorizationManager authorizationManager) : base(sessionFactoryProvider)
    {
        _bookService = bookService;
        _userService = userService;
        _authorizationManager = authorizationManager;
    }

    protected override async Task<IList<BookReadDto>> Consume(FindBooksCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);
            
        var ownerUser = await _userService.GetByIdAsync(evt.OwnerId, cancellationToken)
            .ConfigureAwait(false);

        if (currentUser != null)
        {
            if (currentUser.Id != evt.OwnerId)
            {
                // se non sono l'owner allora devo avere l'accesso speciale
                bool canAction = await _authorizationManager.CanAction(currentUser, AuthorizationKeys.System, AuthorizationKeys.DomuWaveModule).ConfigureAwait(false);
                if (!canAction)
                {
                    throw new UserNotAuthorizedException("Non hai accesso alla risorsa specificata");
                }
            }


            var books = await _bookService.GetAll(ownerUser, cancellationToken).ConfigureAwait(false);

            return books.Select(k=>k.ToDto()).ToList();
        }

        return null;
    }
}