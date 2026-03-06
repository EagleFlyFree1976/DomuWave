using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccountsCategoryTemplate;
using DomuWave.Services.Dto.ChartOfAccountsCategoryTemplate;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetAllChartOfAccountsCategoryTemplatesCommandConsumer
    : InMemoryConsumerBase<GetAllChartOfAccountsCategoryTemplatesCommand, IList<ChartOfAccountsCategoryTemplateReadDto>>
{
    private readonly IChartOfAccountsCategoryTemplateService _templateService;
    private readonly IUserService                            _userService;

    public GetAllChartOfAccountsCategoryTemplatesCommandConsumer(
        ISessionFactoryProvider                  sessionFactoryProvider,
        IChartOfAccountsCategoryTemplateService  templateService,
        IUserService                             userService) : base(sessionFactoryProvider)
    {
        _templateService = templateService;
        _userService     = userService;
    }

    protected override async Task<IList<ChartOfAccountsCategoryTemplateReadDto>> Consume(
        GetAllChartOfAccountsCategoryTemplatesCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var list = await _templateService
            .GetAllAsync(currentUser, cancellationToken)
            .ConfigureAwait(false);

        return list.Select(t => t.ToReadDto()).ToList();
    }
}
