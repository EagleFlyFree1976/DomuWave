
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Beneficiary;
using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Beneficiary;

public class CreateBeneficiaryByNameCommandConsumer : InMemoryConsumerBase<CreateBeneficiaryByNameCommand, BeneficiaryReadDto>
{
    private IUserService _userService;


    private IMediator _mediator;


    public CreateBeneficiaryByNameCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IMediator mediator) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _mediator = mediator;
    }

    protected override async Task<BeneficiaryReadDto> Consume(CreateBeneficiaryByNameCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        CreateBeneficiaryCommand beneficiaryCommand = new CreateBeneficiaryCommand(evt.CurrentUserId, evt.BookId);
        beneficiaryCommand.CreateDto = new BeneficiaryCreateUpdateDto()
        {
            BookId = evt.BookId,
            CategoryId = evt.CreateDto.CategoryId,
            Description = evt.CreateDto.Name,
            Name = evt.CreateDto.Name,
            Iban = null,
            Notes = null
        };

        return await _mediator.GetResponse(beneficiaryCommand, cancellationToken).ConfigureAwait(false);
    }
}