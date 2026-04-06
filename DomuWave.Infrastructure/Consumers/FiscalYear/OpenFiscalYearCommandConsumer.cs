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

public class OpenFiscalYearCommandConsumer : InMemoryConsumerBase<OpenFiscalYearCommand, FiscalYearReadDto>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService _userService;

    public OpenFiscalYearCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService fiscalYearService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService = userService;
    }

    protected override async Task<FiscalYearReadDto> Consume(
        OpenFiscalYearCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var fy = await _fiscalYearService
            .OpenAsync(command.CondominiumId, command.Code, command.Description,
                command.StartDate, command.EndDate, currentUser, cancellationToken,
                command.PreviousFiscalYearId)
            .ConfigureAwait(false);
        return fy.ToReadDto();
    }
}
