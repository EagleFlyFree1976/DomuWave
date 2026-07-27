using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumFee;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetFeeByPaymentCodeCommandConsumer
    : InMemoryConsumerBase<GetFeeByPaymentCodeCommand, IList<FeeByPaymentCodeResultDto>>
{
    private readonly IUserService _userService;

    public GetFeeByPaymentCodeCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<IList<FeeByPaymentCodeResultDto>> Consume(
        GetFeeByPaymentCodeCommand command,
        IMediationContext           mediationContext,
        CancellationToken           cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // Splitta su "/" e spazi, normalizza uppercase, rimuove vuoti
        var codes = command.PaymentCode
            .Split(new[] { '/', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim().ToUpperInvariant())
            .Where(c => c.Length > 0)
            .Distinct()
            .ToList();

        if (codes.Count == 0)
            return [];

        var fees = await session.Query<CondominiumFee>()
            .Where(f => codes.Contains(f.PaymentCode) && !f.IsDeleted && !f.Installment.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (fees.Count == 0)
            return [];

        var unitIds = fees.Select(f => f.Unit.Id).Distinct().ToList();
        var owners = await session.Query<UnitOwner>()
            .Where(o => unitIds.Contains(o.Unit.Id) && o.IsActive && !o.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var ownerByUnit = owners
            .GroupBy(o => o.Unit.Id)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.StartDate).First());

        return fees
            .OrderBy(f => f.Unit.InternalNumber)
            .Select(f =>
            {
                ownerByUnit.TryGetValue(f.Unit.Id, out var owner);
                return new FeeByPaymentCodeResultDto
                {
                    FeeId              = f.Id,
                    PaymentCode        = f.PaymentCode,
                    UnitId             = f.Unit.Id,
                    UnitInternalNumber = f.Unit.InternalNumber ?? string.Empty,
                    UnitDisplayName    = f.Unit.DisplayName,
                    InstallmentId      = f.Installment.Id,
                    InstallmentNumber  = f.Installment.InstallmentNumber,
                    DueDate            = f.Installment.DueDate,
                    FiscalYearCode     = f.Installment.FiscalYear?.Code,
                    CondominiumName    = f.Installment.Condominium?.Name,
                    AmountDue          = f.AmountDue,
                    AmountPaid         = f.AmountPaid,
                    Balance            = f.Balance,
                    PaymentStatus      = f.PaymentStatus ?? string.Empty,
                    OwnerFullName      = owner != null ? $"{owner.FirstName} {owner.LastName}".Trim() : null,
                };
            })
            .ToList();
    }
}
