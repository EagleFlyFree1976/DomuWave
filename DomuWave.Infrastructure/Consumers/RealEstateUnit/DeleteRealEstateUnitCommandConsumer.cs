using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.RealEstateUnit;
using DomuWave.Services.Interfaces;
using LicenseManager.Client.Context;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteRealEstateUnitCommandConsumer : InMemoryConsumerBase<DeleteRealEstateUnitCommand, bool>
{
    private readonly IRealEstateUnitService _realEstateUnitService;
    private readonly IUserService _userService;
    private readonly ILicenseContext _licenseContext;

    public DeleteRealEstateUnitCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IRealEstateUnitService realEstateUnitService,
        IUserService userService,
        ILicenseContext licenseContext) : base(sessionFactoryProvider)
    {
        _realEstateUnitService = realEstateUnitService;
        _userService = userService;
        _licenseContext = licenseContext;
    }

    protected override async Task<bool> Consume(
        DeleteRealEstateUnitCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var deleted = await _realEstateUnitService
            .DeleteAsync(command.UnitId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // UNITS è una feature "Resource": cancellando un'unità lo slot va liberato. Comunichiamo
        // il refund a LM, che decide ed esegue (per le Resource il refund è sempre applicato).
        if (deleted)
            await _licenseContext.RefundAsync(FeatureKeys.UNITS, 1, ct: cancellationToken)
                .ConfigureAwait(false);

        return deleted;
    }
}
