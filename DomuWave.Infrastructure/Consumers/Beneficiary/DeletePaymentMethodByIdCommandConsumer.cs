using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Beneficiary;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Beneficiary;

 
public class DeleteBeneficiaryByIdCommandConsumer : InMemoryConsumerBase<DeleteBeneficiaryByIdCommand, bool>
{
    private IUserService _userService;
    private IBeneficiaryService _paymentMethodService;

    public DeleteBeneficiaryByIdCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IBeneficiaryService paymentMethodService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _paymentMethodService = paymentMethodService;
    }

    protected override async Task<bool> Consume(DeleteBeneficiaryByIdCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        await _paymentMethodService.Delete(evt.BeneficiaryId, evt.BookId,currentUser, cancellationToken).ConfigureAwait(false);
        return true;

    }
}