using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetBudgetsByFiscalYearCommandConsumer
    : InMemoryConsumerBase<GetBudgetsByFiscalYearCommand, IList<BudgetReadDto>>
{
    private readonly IUserService _userService;

    public GetBudgetsByFiscalYearCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<IList<BudgetReadDto>> Consume(
        GetBudgetsByFiscalYearCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var budgets = await session.Query<Budget>()
            .Where(b => b.FiscalYear.Id == command.FiscalYearId && b.Tenant.Id == command.TenantId && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Fetch Ordinaria assemblies for the same fiscal year to link to Preventivo budgets
        var condominiumIds = budgets.Select(b => b.Condominium?.Id ?? 0).Distinct().ToList();
        var assemblies = await session.Query<Assembly>()
            .Where(a => a.FiscalYear != null
                     && a.FiscalYear.Id == command.FiscalYearId
                     && a.AssemblyType.Id == AssemblyTypeLookup.Ordinaria
                     && condominiumIds.Contains(a.Condominium.Id)
                     && !a.IsDeleted)
            .Select(a => new { a.Id, Title = a.Name, CondominiumId = a.Condominium.Id })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var assemblyByCondominium = assemblies.ToDictionary(a => a.CondominiumId);

        return budgets.Select(b =>
        {
            var dto = b.ToReadDto();
            if (b.Type == BudgetType.Preventivo && assemblyByCondominium.TryGetValue(b.Condominium?.Id ?? 0, out var asm))
            {
                dto.AssemblyId    = asm.Id;
                dto.AssemblyTitle = asm.Title;
            }
            return dto;
        }).ToList();
    }
}
