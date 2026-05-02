using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitOwner;
using DomuWave.Services.Dto.UnitOwner;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetUnitOwnersByCondominiumCommandConsumer : InMemoryConsumerBase<GetUnitOwnersByCondominiumCommand, IList<UnitOwnerReadDto>>
{
    private readonly IUserService _userService;

    public GetUnitOwnersByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<UnitOwnerReadDto>> Consume(
        GetUnitOwnersByCondominiumCommand command,
        IMediationContext                  mediationContext,
        CancellationToken                 cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var owners = await session.Query<UnitOwner>()
            .Where(o => o.Unit.Condominium.Id == command.CondominiumId && o.IsActive && !o.IsDeleted)
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return owners.Select(o => o.ToReadDto()).ToList();
    }
}
