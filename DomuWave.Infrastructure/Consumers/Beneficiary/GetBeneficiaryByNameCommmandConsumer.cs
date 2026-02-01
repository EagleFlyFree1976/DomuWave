using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.Beneficiary;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.Beneficiary;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Consumers.Beneficiary;

public class GetBeneficiaryByNameCommmandConsumer : InMemoryConsumerBase<GetBeneficiaryByNameCommmand, BeneficiaryReadDto>
{
    private IUserService _userService;
    private IBeneficiaryService _beneficiaryService;
    private ICoreAuthorizationManager _authorizationManager;
    private IMediator _mediator;
    public GetBeneficiaryByNameCommmandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IBeneficiaryService beneficiaryService, ICoreAuthorizationManager authorizationManager, IMediator mediator) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _beneficiaryService = beneficiaryService;
        _authorizationManager = authorizationManager;
        _mediator = mediator;
    }

    protected override async Task<BeneficiaryReadDto> Consume(GetBeneficiaryByNameCommmand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        // posso accedere al bookid specificato?
        
        CanAccessToBeneficiaryCommand canAccessToBeneficiaryCommand =
            new CanAccessToBeneficiaryCommand(null,evt.CurrentUserId, evt.BookId);
        bool canAccess = await _mediator.GetResponse(canAccessToBeneficiaryCommand).ConfigureAwait(false);
        if (!canAccess)
        {
            throw new UserNotAuthorizedException("Non hai accesso alla risorsa richiesta");
        }
        

        Models.Beneficiary x = await _beneficiaryService.GetByName(evt.Name, evt.BookId, currentUser, cancellationToken).ConfigureAwait(false);
        if (x == null)
            return null;

        


        return x.ToDto();

    }
}

