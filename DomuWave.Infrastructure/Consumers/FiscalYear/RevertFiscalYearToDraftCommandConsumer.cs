using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FiscalYear;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class RevertFiscalYearToDraftCommandConsumer
    : InMemoryConsumerBase<RevertFiscalYearToDraftCommand, bool>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService       _userService;

    public RevertFiscalYearToDraftCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService      fiscalYearService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService       = userService;
    }

    protected override async Task<bool> Consume(
        RevertFiscalYearToDraftCommand command,
        IMediationContext              mediationContext,
        CancellationToken             cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _fiscalYearService
            .RevertToDraftAsync(command.FiscalYearId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
