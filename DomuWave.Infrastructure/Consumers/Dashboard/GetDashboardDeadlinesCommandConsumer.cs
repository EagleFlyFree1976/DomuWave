using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Dashboard;
using DomuWave.Services.Dto.Dashboard;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Dashboard;

/// <summary>
/// Aggrega le "prossime scadenze" del tenant da più fonti: attività (AdminTask),
/// rate (CondominiumInstallment) e assemblee (Assembly). Altre fonti (contratti,
/// manutenzioni, lavori straordinari) si aggiungono con lo stesso pattern.
/// </summary>
public class GetDashboardDeadlinesCommandConsumer
    : InMemoryConsumerBase<GetDashboardDeadlinesCommand, DashboardDeadlinesDto>
{
    private const int PerSourceCap = 50;

    private readonly IUserService _userService;

    public GetDashboardDeadlinesCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<DashboardDeadlinesDto> Consume(
        GetDashboardDeadlinesCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var dto   = new DashboardDeadlinesDto();
        var today = DateTime.Today;
        var limit = today.AddDays(command.Days <= 0 ? 30 : command.Days);

        // Mappa condominiumId → nome (tenant), caricata una volta
        var condominiums = await session.Query<Models.Condominium>()
            .Where(c => c.Tenant.Id == command.TenantId && !c.IsDeleted)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var condominiumName = condominiums.ToDictionary(c => c.Id, c => c.Name);
        var condominiumIds  = condominiums.Select(c => c.Id).ToList();

        var items = new List<DeadlineItemDto>();

        // ── Attività (AdminTask) ─────────────────────────────────────────────
        var tasks = await session.Query<Models.AdminTask>()
            .FetchMany(t => t.Condominiums).ThenFetch(c => c.Condominium)
            .Where(t => t.Tenant.Id == command.TenantId && !t.IsDeleted
                        && t.Status.Id != AdminTaskStatusLookup.Completata
                        && t.Status.Id != AdminTaskStatusLookup.Annullata
                        && t.DueDate != null && t.DueDate <= limit)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var taskItems = tasks.Distinct()
            .OrderBy(t => t.DueDate)
            .Take(PerSourceCap)
            .Select(t =>
            {
                var names = (t.Condominiums ?? new List<Models.AdminTaskCondominium>())
                    .Where(c => !c.IsDeleted && c.Condominium != null)
                    .Select(c => c.Condominium.Name)
                    .ToList();
                return new DeadlineItemDto
                {
                    Type               = "Task",
                    Id                 = t.Id,
                    Title              = t.Title ?? t.Name,
                    Description        = t.Description,
                    DueDate            = t.DueDate,
                    Status             = t.Status?.Name,
                    Priority           = t.Priority?.Name,
                    CondominiumName    = names.Count > 0 ? string.Join(", ", names) : null,
                    AssignedToFullName = t.AssignedToFullName,
                    FrontendLink       = "/attivita",
                    Urgency            = t.DueDate < today ? "Overdue" : "Upcoming",
                };
            })
            .ToList();

        dto.UpcomingTasks = taskItems;
        items.AddRange(taskItems);

        if (condominiumIds.Count > 0)
        {
            // ── Rate (CondominiumInstallment) ────────────────────────────────
            var installments = await session.Query<CondominiumInstallment>()
                .Where(i => condominiumIds.Contains(i.Condominium.Id) && !i.IsDeleted
                            && i.Status.Id != CondominiumInstallmentStatus.Paid
                            && i.Status.Id != CondominiumInstallmentStatus.Cancelled
                            && i.DueDate <= limit)
                .OrderBy(i => i.DueDate)
                .Take(PerSourceCap)
                .Select(i => new { i.Id, i.InstallmentNumber, i.DueDate, i.TotalAmount, CondId = i.Condominium.Id })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            items.AddRange(installments.Select(i => new DeadlineItemDto
            {
                Type            = "Installment",
                Id              = i.Id,
                Title           = $"Rata n. {i.InstallmentNumber}",
                Description     = $"Importo € {i.TotalAmount:N2}",
                DueDate         = i.DueDate,
                CondominiumId   = i.CondId,
                CondominiumName = condominiumName.GetValueOrDefault(i.CondId),
                FrontendLink    = "/rate",
                Urgency         = i.DueDate < today ? "Overdue" : "Upcoming",
            }));

            // ── Assemblee (Assembly) ─────────────────────────────────────────
            var assemblies = await session.Query<Assembly>()
                .Where(a => condominiumIds.Contains(a.Condominium.Id) && !a.IsDeleted
                            && (a.Status.Id == AssemblyStatusLookup.Pianificata
                                || a.Status.Id == AssemblyStatusLookup.Convocata
                                || a.Status.Id == AssemblyStatusLookup.InCorso)
                            && a.ScheduledDate >= today && a.ScheduledDate <= limit)
                .OrderBy(a => a.ScheduledDate)
                .Take(PerSourceCap)
                .Select(a => new { a.Id, a.Name, a.ScheduledDate, StatusName = a.Status.Name, CondId = a.Condominium.Id })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            items.AddRange(assemblies.Select(a => new DeadlineItemDto
            {
                Type            = "Assembly",
                Id              = a.Id,
                Title           = a.Name,
                Description     = "Assemblea",
                DueDate         = a.ScheduledDate,
                Status          = a.StatusName,
                CondominiumId   = a.CondId,
                CondominiumName = condominiumName.GetValueOrDefault(a.CondId),
                FrontendLink    = $"/condomini/{a.CondId}/assemblee",
                Urgency         = a.ScheduledDate < today ? "Overdue" : "Upcoming",
            }));
        }

        dto.Items = items
            .OrderBy(i => i.DueDate ?? DateTime.MaxValue)
            .ToList();

        return dto;
    }
}
