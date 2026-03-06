using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccountsCategoryTemplate;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteChartOfAccountsCategoryTemplateCommandConsumer
    : InMemoryConsumerBase<DeleteChartOfAccountsCategoryTemplateCommand, bool>
{
    private readonly IChartOfAccountsCategoryTemplateService _templateService;
    private readonly IUserService                            _userService;

    public DeleteChartOfAccountsCategoryTemplateCommandConsumer(
        ISessionFactoryProvider                  sessionFactoryProvider,
        IChartOfAccountsCategoryTemplateService  templateService,
        IUserService                             userService) : base(sessionFactoryProvider)
    {
        _templateService = templateService;
        _userService     = userService;
    }

    protected override async Task<bool> Consume(
        DeleteChartOfAccountsCategoryTemplateCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var deleted = await _templateService
            .DeleteAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (!deleted)
            throw new NotFoundException("Template non trovato.");

        return true;
    }
}
