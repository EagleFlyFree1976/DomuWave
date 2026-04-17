using DomuWave.Services.Dto.CondominiumInstallment;
using DomuWave.Services.Models;
using NHibernate;
using NHibernate.Linq;

namespace DomuWave.Services.Consumers;

/// <summary>
/// Arricchisce una lista di <see cref="CondominiumInstallmentReadDto"/> con i totali
/// delle quote (<see cref="CondominiumFee"/>): <c>TotalPaid</c> e <c>TotalOverdue</c>.
/// Esegue un'unica query aggregata su tutti gli installment ID passati.
/// </summary>
internal static class InstallmentFeeEnricher
{
    public static async Task EnrichAsync(
        ISession session,
        IList<CondominiumInstallmentReadDto> dtos,
        CancellationToken cancellationToken)
    {
        if (dtos.Count == 0) return;

        var instIds = dtos.Select(d => d.Id).ToList();

        var feeAggregates = await session.Query<CondominiumFee>()
            .Where(f => instIds.Contains(f.Installment.Id) && !f.IsDeleted)
            .Select(f => new
            {
                InstallmentId = f.Installment.Id,
                f.AmountPaid,
                f.Balance,
                f.PaymentStatus,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var byInst = feeAggregates.GroupBy(f => f.InstallmentId).ToDictionary(g => g.Key);

        foreach (var dto in dtos)
        {
            if (!byInst.TryGetValue(dto.Id, out var group)) continue;
            dto.TotalPaid    = group.Sum(f => f.AmountPaid);
            dto.TotalOverdue = group.Where(f => f.PaymentStatus == "Overdue")
                                   .Sum(f => f.Balance);
        }
    }
}
