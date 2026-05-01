using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.RealEstateUnit;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetCondominiumPanoramaCommandConsumer : InMemoryConsumerBase<GetCondominiumPanoramaCommand, IList<RealEstateUnitPanoramaDto>>
{
    private readonly IRealEstateUnitService _realEstateUnitService;
    private readonly IUserService           _userService;

    public GetCondominiumPanoramaCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IRealEstateUnitService realEstateUnitService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _realEstateUnitService = realEstateUnitService;
        _userService           = userService;
    }

    protected override async Task<IList<RealEstateUnitPanoramaDto>> Consume(
        GetCondominiumPanoramaCommand command,
        IMediationContext              mediationContext,
        CancellationToken             cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var units = await _realEstateUnitService
            .GetByCondominiumIdAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        var unitIds = units.Select(u => u.Id).ToHashSet();

        var owners = await session.Query<Models.UnitOwner>()
            .Where(o => o.Unit != null && unitIds.Contains(o.Unit.Id) && !o.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var tenants = await session.Query<Models.UnitTenant>()
            .Where(t => t.Unit != null && unitIds.Contains(t.Unit.Id) && !t.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var ownersByUnit  = owners.GroupBy(o => o.Unit.Id).ToDictionary(g => g.Key, g => g.Select(o => o.ToReadDto()).ToList());
        var tenantsByUnit = tenants.GroupBy(t => t.Unit.Id).ToDictionary(g => g.Key, g => g.Select(t => t.ToReadDto()).ToList());

        return units.Select(u => u.ToPanoramaDto(
            ownersByUnit.TryGetValue(u.Id,  out var ow) ? ow : [],
            tenantsByUnit.TryGetValue(u.Id, out var te) ? te : []
        )).ToList();
    }
}
