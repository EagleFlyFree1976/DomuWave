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

public class GetFiscalYearByIdCommandConsumer : InMemoryConsumerBase<GetFiscalYearByIdCommand, FiscalYearReadDto>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService _userService;

    public GetFiscalYearByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService fiscalYearService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService = userService;
    }

    protected override async Task<FiscalYearReadDto> Consume(
        GetFiscalYearByIdCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var fy = await _fiscalYearService
            .GetByIdAsync(command.FiscalYearId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        return fy?.ToReadDto();
    }
}
