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

    public GetAllCondominiumsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService condominiumService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _userService = userService;
    }

    protected override async Task<IList<CondominiumReadDto>> Consume(
        GetAllCondominiumsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        if (string.Equals(currentUser.Role?.Code, "condomino", StringComparison.OrdinalIgnoreCase))
        {
            long userId = command.CurrentUserId;
            var condominiumIds = await session.Query<UnitOwner>()
                .Where(o => o.UserId == userId && o.IsActive && !o.IsDeleted)
                .Select(o => o.Unit.Condominium.Id)
                .Distinct()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

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
