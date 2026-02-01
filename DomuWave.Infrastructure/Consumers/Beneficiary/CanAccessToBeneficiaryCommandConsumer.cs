using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Beneficiary;

/// <summary>
///   L'utente puo accedere all'elemento specificato?
/// </summary>

public class CanAccessToBeneficiaryCommandConsumer : InMemoryConsumerBase<CanAccessToBeneficiaryCommand, bool>
{
    private ICoreAuthorizationManager _authorizationManager;
    private IMediator _mediator;
    private IBookService _bookService;
    private IBeneficiaryService _beneficiaryService;
    private IUserService _userService;

    public CanAccessToBeneficiaryCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ICoreAuthorizationManager authorizationManager, IMediator mediator, IBookService bookService, IUserService userService, IBeneficiaryService BeneficiaryService) : base(sessionFactoryProvider)
    {
        _authorizationManager = authorizationManager;
        _mediator = mediator;
        _bookService = bookService;
        _userService = userService;
        _beneficiaryService = BeneficiaryService;
    }

    protected override async Task<bool> Consume(CanAccessToBeneficiaryCommand evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var book = await _bookService.GetById(evt.BookId, cancellationToken).ConfigureAwait(false);

        if (book == null)
            return false;
        if (book.OwnerId == currentUser.Id)
        {
            if (evt.BeneficiaryId.HasValue)
            {
                var beneficiary = await _beneficiaryService
                    .GetById(evt.BeneficiaryId.Value, evt.BookId,currentUser, cancellationToken).ConfigureAwait(false);

                if (beneficiary == null)
                    return false;
                book = beneficiary.Book;
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