using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Dashboard;
using DomuWave.Services.Dto.Dashboard;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Dashboard;

public class GetDashboardSummaryCommandConsumer
    : InMemoryConsumerBase<GetDashboardSummaryCommand, DashboardSummaryDto>
{
    private readonly IUserService           _userService;
    private readonly IUserTenantService     _userTenantService;
    private readonly IRealEstateUnitService _realEstateUnitService;

    public GetDashboardSummaryCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService,
        IUserTenantService userTenantService,
        IRealEstateUnitService realEstateUnitService) : base(sessionFactoryProvider)
    {
        _userService           = userService;
        _userTenantService     = userTenantService;
        _realEstateUnitService = realEstateUnitService;
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

            dto.TotalActiveCondominiumsCount = await session.Query<Models.Condominium>()
                .CountAsync(c => c.IsActive && !c.IsDeleted
                    && c.Tenant.IsActive && !c.Tenant.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            var condominiumIds = await session.Query<Models.Condominium>()
                .Where(c => c.Tenant.Id == command.TenantId && c.IsActive && !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // Condòmino NEL TENANT ATTIVO: restringe ai soli condomìni delle proprie unità.
            var isCondomino = await _userTenantService
                .IsCondominoInTenantAsync(command.CurrentUserId, command.TenantId, cancellationToken)
                .ConfigureAwait(false);
            if (isCondomino)
            {
                var ownUnitIds = await _realEstateUnitService
                    .GetCondominoUnitIdsAsync(command.CurrentUserId, cancellationToken)
                    .ConfigureAwait(false);
                var ownCondoIds = await session.Query<RealEstateUnit>()
                    .Where(u => ownUnitIds.Contains(u.Id) && !u.IsDeleted)
                    .Select(u => u.Condominium.Id)
                    .Distinct()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                condominiumIds = condominiumIds.Where(cid => ownCondoIds.Contains(cid)).ToList();
            }

            dto.CondominiumsCount = condominiumIds.Count;

            if (condominiumIds.Count > 0)
            {
                dto.TotalUnitsCount = await session.Query<RealEstateUnit>()
                    .CountAsync(u => condominiumIds.Contains(u.Condominium.Id) && !u.IsDeleted, cancellationToken)
                    .ConfigureAwait(false);

                dto.OpenInstallmentsCount = await session.Query<CondominiumInstallment>()
                    .CountAsync(i => condominiumIds.Contains(i.Condominium.Id)
                        && i.Status.Id != CondominiumInstallmentStatus.Paid
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
