using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Tenant;
using DomuWave.Services.Dto.Tenant;
using DomuWave.Services.Helper;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Tenant;

public class GetPagedTenantsCommandConsumer
    : InMemoryConsumerBase<GetPagedTenantsCommand, PagedResult<TenantReadDto>>
{
    private readonly ITenantService _tenantService;
    private readonly IUserService _userService;

    public GetPagedTenantsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ITenantService tenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _tenantService = tenantService;
        _userService   = userService;
    }

    protected override async Task<PagedResult<TenantReadDto>> Consume(
        GetPagedTenantsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var (items, totalCount) = await _tenantService.GetPagedAsync(
            filter:      x => !x.IsDeleted
                              && (string.IsNullOrEmpty(command.Search) || x.Name.Contains(command.Search))
                              && (!command.IsActive.HasValue || x.IsActive == command.IsActive),
            pageNumber:  command.PageNumber,
            pageSize:    command.PageSize,
            orderBy:     x => x.Name!,
            ascending:   command.Asc,
            currentUser: currentUser,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return new PagedResult<TenantReadDto>
        {
            PageNumber = command.PageNumber,
            PageSize   = command.PageSize,
            TotalCount = totalCount,
            Items      = items.Select(t => t.ToReadDto()).ToList(),
        };
    }
}
