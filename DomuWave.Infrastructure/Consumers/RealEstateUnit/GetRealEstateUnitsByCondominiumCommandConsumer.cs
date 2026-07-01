using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.RealEstateUnit;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetRealEstateUnitsByCondominiumCommandConsumer : InMemoryConsumerBase<GetRealEstateUnitsByCondominiumCommand, IList<RealEstateUnitReadDto>>
{
    private readonly IRealEstateUnitService _realEstateUnitService;
    private readonly IUserService           _userService;
    private readonly IUserTenantService     _userTenantService;

    public GetRealEstateUnitsByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IRealEstateUnitService realEstateUnitService,
        IUserService userService,
        IUserTenantService userTenantService) : base(sessionFactoryProvider)
    {
        _realEstateUnitService = realEstateUnitService;
        _userService           = userService;
        _userTenantService     = userTenantService;
    }

    protected override async Task<IList<RealEstateUnitReadDto>> Consume(
        GetRealEstateUnitsByCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var units = await _realEstateUnitService
            .GetByCondominiumIdAsync(command.CondominiumId, command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Condòmino NEL TENANT ATTIVO: restringe alle sole unità proprie.
        // (ruolo per-tenant: lo stesso utente può essere admin altrove)
        var isCondomino = await _userTenantService
            .IsCondominoInTenantAsync(command.CurrentUserId, command.TenantId, cancellationToken)
            .ConfigureAwait(false);
        if (isCondomino)
        {
            var ownUnitIds = await _realEstateUnitService
                .GetCondominoUnitIdsAsync(command.CurrentUserId, cancellationToken)
                .ConfigureAwait(false);
            units = units.Where(u => ownUnitIds.Contains(u.Id)).ToList();
        }

        var unitIds = units.Select(u => u.Id).ToList();

        var ownerCountsTask = session.Query<UnitOwner>()
            .Where(o => unitIds.Contains(o.Unit.Id) && o.IsActive && !o.IsDeleted)
            .GroupBy(o => o.Unit.Id)
            .Select(g => new { UnitId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var tenantCountsTask = session.Query<UnitTenant>()
            .Where(t => unitIds.Contains(t.Unit.Id) && t.IsActive && !t.IsDeleted)
            .GroupBy(t => t.Unit.Id)
            .Select(g => new { UnitId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        await Task.WhenAll(ownerCountsTask, tenantCountsTask).ConfigureAwait(false);

        var ownerCounts  = ownerCountsTask.Result.ToDictionary(x => x.UnitId, x => x.Count);
        var tenantCounts = tenantCountsTask.Result.ToDictionary(x => x.UnitId, x => x.Count);

        return units.Select(u =>
        {
            var dto = u.ToReadDto();
            dto.OwnersCount  = ownerCounts.GetValueOrDefault(u.Id);
            dto.TenantsCount = tenantCounts.GetValueOrDefault(u.Id);
            return dto;
        }).ToList();
    }
}
