using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.TenantDisplay;
using DomuWave.Services.Dto.TenantDisplay;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

// ── GET ───────────────────────────────────────────────────────────────────────

public class GetTenantDisplaySettingsCommandConsumer
    : InMemoryConsumerBase<GetTenantDisplaySettingsCommand, TenantDisplaySettingsReadDto?>
{
    private readonly IUserService _userService;

    public GetTenantDisplaySettingsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<TenantDisplaySettingsReadDto?> Consume(
        GetTenantDisplaySettingsCommand command,
        IMediationContext               mediationContext,
        CancellationToken               cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<TenantDisplaySettings>()
            .Where(s => s.Tenant.Id == command.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        // Nessun record → restituisci i default (SoloColore) così il client ha sempre una risposta.
        if (entity == null)
            return new TenantDisplaySettingsReadDto
            {
                AccountingSignConvention     = (int)AccountingSignConvention.SoloColore,
                AccountingSignConventionName = AccountingSignConvention.SoloColore.ToString(),
            };

        return entity.ToReadDto();
    }
}

// ── UPSERT ────────────────────────────────────────────────────────────────────

public class UpsertTenantDisplaySettingsCommandConsumer
    : InMemoryConsumerBase<UpsertTenantDisplaySettingsCommand, TenantDisplaySettingsReadDto>
{
    private readonly IUserService _userService;

    public UpsertTenantDisplaySettingsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<TenantDisplaySettingsReadDto> Consume(
        UpsertTenantDisplaySettingsCommand command,
        IMediationContext                  mediationContext,
        CancellationToken                  cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (!Enum.IsDefined(typeof(AccountingSignConvention), command.Dto.AccountingSignConvention))
            throw new ValidatorException("Convenzione segno non valida.");

        var existing = await session.Query<TenantDisplaySettings>()
            .Where(s => s.Tenant.Id == command.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (existing == null)
        {
            var tenant = await session.GetAsync<Models.Tenant>(command.TenantId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException("Tenant non trovato.");

            var entity = command.Dto.ToEntity(tenant);
            entity.Trace(currentUser);
            await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
            return entity.ToReadDto();
        }

        existing.ApplyUpdate(command.Dto);
        existing.Trace(currentUser);
        await session.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return existing.ToReadDto();
    }
}
