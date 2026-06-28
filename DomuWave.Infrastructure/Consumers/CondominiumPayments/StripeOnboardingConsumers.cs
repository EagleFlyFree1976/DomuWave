using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Clients;
using DomuWave.Services.Command.CondominiumPayments;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

// ── Avvio onboarding (admin) ────────────────────────────────────────────────────

public class StartStripeOnboardingCommandConsumer
    : InMemoryConsumerBase<StartStripeOnboardingCommand, string>
{
    private readonly IUserService   _userService;
    private readonly IStripeService _stripeService;

    public StartStripeOnboardingCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService,
        IStripeService          stripeService) : base(sessionFactoryProvider)
    {
        _userService   = userService;
        _stripeService = stripeService;
    }

    protected override async Task<string> Consume(
        StartStripeOnboardingCommand command,
        IMediationContext            mediationContext,
        CancellationToken            cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var condominium = await session.Query<Models.Condominium>()
            .FirstOrDefaultAsync(c => c.Id == command.CondominiumId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Condominio non trovato.");

        // Crea il connected account la prima volta.
        if (string.IsNullOrWhiteSpace(condominium.StripeConnectedAccountId))
        {
            var accountId = await _stripeService
                .CreateConnectedAccountAsync(condominium, cancellationToken).ConfigureAwait(false);

            condominium.StripeConnectedAccountId = accountId;
            condominium.StripeOnboardingComplete = false;
            condominium.Trace(currentUser);
            await session.UpdateAsync(condominium, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        return await _stripeService
            .CreateAccountLinkAsync(condominium.StripeConnectedAccountId, cancellationToken)
            .ConfigureAwait(false);
    }
}

// ── Refresh stato onboarding (admin) ────────────────────────────────────────────

public class RefreshStripeAccountStatusCommandConsumer
    : InMemoryConsumerBase<RefreshStripeAccountStatusCommand, bool>
{
    private readonly IUserService   _userService;
    private readonly IStripeService _stripeService;

    public RefreshStripeAccountStatusCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService,
        IStripeService          stripeService) : base(sessionFactoryProvider)
    {
        _userService   = userService;
        _stripeService = stripeService;
    }

    protected override async Task<bool> Consume(
        RefreshStripeAccountStatusCommand command,
        IMediationContext                 mediationContext,
        CancellationToken                 cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var condominium = await session.Query<Models.Condominium>()
            .FirstOrDefaultAsync(c => c.Id == command.CondominiumId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Condominio non trovato.");

        if (string.IsNullOrWhiteSpace(condominium.StripeConnectedAccountId))
            return false;

        var onboarded = await _stripeService
            .IsAccountOnboardedAsync(condominium.StripeConnectedAccountId, cancellationToken)
            .ConfigureAwait(false);

        if (condominium.StripeOnboardingComplete != onboarded)
        {
            condominium.StripeOnboardingComplete = onboarded;
            condominium.Trace(currentUser);
            await session.UpdateAsync(condominium, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        return onboarded;
    }
}
