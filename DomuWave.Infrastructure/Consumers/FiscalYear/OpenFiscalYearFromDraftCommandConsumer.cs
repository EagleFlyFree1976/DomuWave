using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FiscalYear;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class OpenFiscalYearFromDraftCommandConsumer : InMemoryConsumerBase<OpenFiscalYearFromDraftCommand, bool>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService _userService;

    public OpenFiscalYearFromDraftCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService fiscalYearService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService = userService;
    }

    protected override async Task<bool> Consume(
        OpenFiscalYearFromDraftCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _fiscalYearService
            .OpenFromDraftAsync(command.FiscalYearId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
