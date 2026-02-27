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

public class UpdateFiscalYearCommandConsumer : InMemoryConsumerBase<UpdateFiscalYearCommand, FiscalYearReadDto>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService _userService;

    public UpdateFiscalYearCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService fiscalYearService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService = userService;
    }

    protected override async Task<FiscalYearReadDto> Consume(
        UpdateFiscalYearCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var updated = await _fiscalYearService
            .UpdateAsync(command.FiscalYearId, command.Dto, currentUser, cancellationToken)
            .ConfigureAwait(false);
        return updated?.ToReadDto();
    }
}
