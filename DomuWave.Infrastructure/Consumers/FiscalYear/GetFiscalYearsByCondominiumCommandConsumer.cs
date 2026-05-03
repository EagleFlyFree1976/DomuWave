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

public class GetFiscalYearsByCondominiumCommandConsumer : InMemoryConsumerBase<GetFiscalYearsByCondominiumCommand, IList<FiscalYearListItemDto>>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService _userService;

    public GetFiscalYearsByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService fiscalYearService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService = userService;
    }

    protected override async Task<IList<FiscalYearListItemDto>> Consume(
        GetFiscalYearsByCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var list = await _fiscalYearService
            .GetByCondominiumAsync(command.CondominiumId, command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        return list.Select(fy => fy.ToListItemDto()).ToList();
    }
}
