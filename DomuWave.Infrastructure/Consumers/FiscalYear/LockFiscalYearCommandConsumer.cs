using DomuWave.Services.Models;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Interfaces.Extensions;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FiscalYear;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class LockFiscalYearCommandConsumer : InMemoryConsumerBase<LockFiscalYearCommand, bool>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService _userService;

    public LockFiscalYearCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService fiscalYearService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService = userService;
    }

    protected override async Task<bool> Consume(
        LockFiscalYearCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _fiscalYearService
            .LockAsync(command.FiscalYearId, currentUser, command.Notes, cancellationToken)
            .ConfigureAwait(false);
    }
}
