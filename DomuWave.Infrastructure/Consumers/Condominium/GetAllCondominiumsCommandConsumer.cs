using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetAllCondominiumsCommandConsumer : InMemoryConsumerBase<GetAllCondominiumsCommand, IList<CondominiumReadDto>>
{
    private readonly ICondominiumService _condominiumService;
    private readonly IUserService _userService;
    private readonly IUserTenantService _userTenantService;

    public GetAllCondominiumsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService condominiumService,
        IUserService userService,
        IUserTenantService userTenantService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _userService = userService;
        _userTenantService = userTenantService;
    }

    protected override async Task<IList<CondominiumReadDto>> Consume(
        GetAllCondominiumsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // Condòmino NEL TENANT ATTIVO: vede solo i condomìni dove ha unità
        // (proprietario o inquilino).
        var isCondomino = await _userTenantService
            .IsCondominoInTenantAsync(command.CurrentUserId, command.TenantId, cancellationToken)
            .ConfigureAwait(false);
        if (isCondomino)
        {
            long userId = command.CurrentUserId;
            var ownerCondoIds = await session.Query<UnitOwner>()
                .Where(o => o.UserId == userId && o.IsActive && !o.IsDeleted)
                .Select(o => o.Unit.Condominium.Id)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            var tenantCondoIds = await session.Query<UnitTenant>()
                .Where(t => t.UserId == userId && t.IsActive && !t.IsDeleted)
                .Select(t => t.Unit.Condominium.Id)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            var condominiumIds = ownerCondoIds.Concat(tenantCondoIds).Distinct().ToList();

            var result = await _condominiumService
                .GetByTenantIdAsync(command.TenantId, currentUser, cancellationToken)
                .ConfigureAwait(false);

            return result
                .Where(c => condominiumIds.Contains(c.Id))
                .Select(x => x.ToReadDto())
                .ToList();
        }

        var all = await _condominiumService
            .GetByTenantIdAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return all.Select(x => x.ToReadDto()).ToList();
    }
}
