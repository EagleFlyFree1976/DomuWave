using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Dashboard;
using DomuWave.Services.Dto.Dashboard;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Dashboard;

public class GetDashboardSummaryCommandConsumer
    : InMemoryConsumerBase<GetDashboardSummaryCommand, DashboardSummaryDto>
{
    private readonly IUserService _userService;

    public GetDashboardSummaryCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<DashboardSummaryDto> Consume(
        GetDashboardSummaryCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var dto = new DashboardSummaryDto();

        if (currentUser.IsSystemUser)
        {
            dto.IsSuperAdmin = true;

            dto.ActiveTenantsCount = await session.Query<Models.Tenant>()
                .CountAsync(t => t.IsActive && !t.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            dto.TotalActiveCondominiumsCount = await session.Query<Condominium>()
                .CountAsync(c => c.IsActive && !c.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            var condominiumIds = await session.Query<Condominium>()
                .Where(c => c.Tenant.Id == command.TenantId && c.IsActive && !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            dto.CondominiumsCount = condominiumIds.Count;

            if (condominiumIds.Count > 0)
            {
                dto.TotalUnitsCount = await session.Query<RealEstateUnit>()
                    .CountAsync(u => condominiumIds.Contains(u.Condominium.Id) && !u.IsDeleted, cancellationToken)
                    .ConfigureAwait(false);

                dto.OpenInstallmentsCount = await session.Query<CondominiumInstallment>()
                    .CountAsync(i => condominiumIds.Contains(i.Condominium.Id)
                        && i.Status != "Paid"
                        && !i.IsDeleted, cancellationToken)
                    .ConfigureAwait(false);

                dto.UnpaidExpensesCount = await session.Query<Expense>()
                    .CountAsync(e => condominiumIds.Contains(e.Condominium.Id)
                        && !e.PaymentDate.HasValue
                        && !e.IsDeleted, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return dto;
    }
}
