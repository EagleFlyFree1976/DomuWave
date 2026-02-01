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

public class FindBeneficiaryCommandConsumer : InMemoryConsumerBase<FindBeneficiaryCommand, IList<BeneficiaryReadDto>>
{
    private IUserService _userService;
    private IBeneficiaryService _beneficiaryService;
    private ICoreAuthorizationManager _authorizationManager;
    private IMediator _mediator;
    public FindBeneficiaryCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IBeneficiaryService beneficiaryService, ICoreAuthorizationManager authorizationManager, IMediator mediator) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _beneficiaryService = beneficiaryService;
        _authorizationManager = authorizationManager;
        _mediator = mediator;
    }

    protected override async Task<IList<BeneficiaryReadDto>> Consume(FindBeneficiaryCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
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
        

        IList<Models.Beneficiary> x = await _beneficiaryService.Find(evt.Q, evt.BookId, currentUser, cancellationToken).ConfigureAwait(false);

        List<BeneficiaryReadDto> beneficiaryReadDtos = x.Select(k => k.ToDto()).ToList();
        if (evt.AddIfNotExists)
        {
            beneficiaryReadDtos.Add(new BeneficiaryReadDto(){BookId = evt.BookId, Category = null, Name = evt.Q, Description = evt.Q,Fake = true, Id = 0});
        }
        
        
            
            return beneficiaryReadDtos;
        

    }
}

