using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.Beneficiary;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Beneficiary;
 

/// <summary>
/// Crea un nuovo metodo di pagamento con im parametri impostati
/// </summary>
public class UpdateBeneficiaryCommandConsumer : InMemoryConsumerBase<UpdateBeneficiaryCommand, BeneficiaryReadDto>
{
    private IUserService _userService;
    private IBeneficiaryService _paymentMethodService;

    public UpdateBeneficiaryCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IBeneficiaryService paymentMethodService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _paymentMethodService = paymentMethodService;
    }

    protected override async Task<BeneficiaryReadDto> Consume(UpdateBeneficiaryCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        Models.Beneficiary x = await _paymentMethodService.Update(evt.BeneficiaryId,evt.UpdateDto, currentUser, cancellationToken).ConfigureAwait(false);
        return x.ToDto();

    }
}
