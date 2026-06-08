using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Assembly;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using LicenseManager.Client.Context;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteAssemblyCommandConsumer : InMemoryConsumerBase<DeleteAssemblyCommand, bool>
{
    private readonly IAssemblyService _assemblyService;
    private readonly IUserService     _userService;
    private readonly ILicenseContext  _licenseContext;

    public DeleteAssemblyCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyService assemblyService,
        IUserService userService,
        ILicenseContext licenseContext) : base(sessionFactoryProvider)
    {
        _assemblyService = assemblyService;
        _userService     = userService;
        _licenseContext  = licenseContext;
    }

    protected override async Task<bool> Consume(
        DeleteAssemblyCommand command,
        IMediationContext      mediationContext,
        CancellationToken     cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity      = await _assemblyService.GetByIdAsync(command.AssemblyId, currentUser, cancellationToken).ConfigureAwait(false);
        if (entity == null) return false;

        // ASSEMBLY è una feature "a evento" (Event): LM di norma non rimborsa. Ma se l'assemblea
        // era ancora una bozza (mai convocata/aperta/chiusa) — quindi creata per errore e mai
        // realmente usata — chiediamo a LM il refund forzato (forceRefund). È comunque LM a
        // eseguire il decremento ed è la fonte di verità del contatore.
        var wasDraft = entity.Status?.Id == AssemblyStatusLookup.Bozza;

        entity.SoftDelete(currentUser);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        if (wasDraft)
            await _licenseContext.RefundAsync(FeatureKeys.ASSEMBLY, 1, forceRefund: true, cancellationToken)
                .ConfigureAwait(false);

        return true;
    }
}
