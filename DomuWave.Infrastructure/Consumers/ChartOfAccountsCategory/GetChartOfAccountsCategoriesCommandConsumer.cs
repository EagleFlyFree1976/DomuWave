using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccountsCategory;
using DomuWave.Services.Dto.ChartOfAccountsCategory;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetChartOfAccountsCategoriesCommandConsumer
    : InMemoryConsumerBase<GetChartOfAccountsCategoriesCommand, IList<ChartOfAccountsCategoryReadDto>>
{
    private readonly IChartOfAccountsCategoryService _categoryService;
    private readonly IUserService                    _userService;

    public GetChartOfAccountsCategoriesCommandConsumer(
        ISessionFactoryProvider          sessionFactoryProvider,
        IChartOfAccountsCategoryService  categoryService,
        IUserService                     userService) : base(sessionFactoryProvider)
    {
        _categoryService = categoryService;
        _userService     = userService;
    }

    protected override async Task<IList<ChartOfAccountsCategoryReadDto>> Consume(
        GetChartOfAccountsCategoriesCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var list = await _categoryService
            .GetByTenantIdAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return list.Select(c => c.ToReadDto()).ToList();
    }
}
