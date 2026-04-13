using DomuWave.Services.Models;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Interfaces.Extensions;
using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FiscalYear;
using DomuWave.Services.Interfaces;
using NHibernate.Linq;
using SimpleMediator.Core;
using Models = DomuWave.Services.Models;

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

        // Se c'è un esercizio precedente, propaga i ClosingBalance come saldi iniziali
        if (fy.PreviousFiscalYear != null)
        {
            await PropagateUnitOpeningBalances(fy, currentUser, cancellationToken)
                .ConfigureAwait(false);
        }

        return fy.ToReadDto();
    }

    private async Task PropagateUnitOpeningBalances(
        FiscalYear fiscalYear,
        object currentUser,
        CancellationToken cancellationToken)
    {
        var previousFiscalYearId = fiscalYear.PreviousFiscalYear.Id;

        // ClosingBalance dell'esercizio precedente per unità
        var previousBalances = await session.Query<Models.UnitOpeningBalance>()
            .Where(b => b.FiscalYear.Id == previousFiscalYearId && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!previousBalances.Any()) return;

        // Evita duplicati (nel caso il metodo venga richiamato due volte)
        var existingUnitIds = await session.Query<Models.UnitOpeningBalance>()
            .Where(b => b.FiscalYear.Id == fiscalYear.Id && !b.IsDeleted)
            .Select(b => b.Unit.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var user = currentUser as CPQ.Core.Memberships.IUser;

        foreach (var prev in previousBalances)
        {
            if (existingUnitIds.Contains(prev.Unit.Id)) continue;

            var newBalance = new Models.UnitOpeningBalance
            {
                Unit            = prev.Unit,
                FiscalYear      = fiscalYear,
                Tenant          = fiscalYear.Tenant,
                OpeningBalance  = prev.ClosingBalance,
                RateAddebitate  = 0m,
                RateIncassate   = 0m,
                QuotaConsuntiva = 0m,
                SaldoConguaglio = 0m,
                TotalMovements  = 0m,
                ClosingBalance  = 0m,
            };
            if (user != null) newBalance.Trace(user);
            await session.SaveAsync(newBalance, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
