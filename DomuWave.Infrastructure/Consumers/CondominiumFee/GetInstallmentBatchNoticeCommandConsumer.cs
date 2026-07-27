using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumFee;
using DomuWave.Services.Dto.PaymentNotice;
using DomuWave.Services.Models;
using DomuWave.Services.Pdf;
using NHibernate.Linq;
using QuestPDF.Fluent;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetInstallmentBatchNoticeCommandConsumer
    : InMemoryConsumerBase<GetInstallmentBatchNoticeCommand, byte[]>
{
    private readonly IUserService _userService;

    public GetInstallmentBatchNoticeCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<byte[]> Consume(
        GetInstallmentBatchNoticeCommand command,
        IMediationContext                 mediationContext,
        CancellationToken                 cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // 1. Load installment (with condominium + address + fiscal year)
        var installment = await session.Query<CondominiumInstallment>()
            .Where(i => i.Id == command.InstallmentId && !i.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (installment == null)
            throw new NotFoundException("Rata non trovata.");

        var condominium = installment.Condominium;
        var address     = condominium.Address;
        var fiscalYear  = installment.FiscalYear;

        // 2. Load all fees for this installment (with unit)
        var fees = await session.Query<CondominiumFee>()
            .Where(f => f.Installment.Id == command.InstallmentId && !f.IsDeleted && !f.Installment.IsDeleted)
            .OrderBy(f => f.Unit.InternalNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (fees.Count == 0)
            throw new ValidatorException("Nessuna quota trovata per questa rata.");

        var unitIds = fees.Select(f => f.Unit.Id).Distinct().ToList();

        // 3. Load active owners per unit
        var owners = await session.Query<UnitOwner>()
            .Where(o => unitIds.Contains(o.Unit.Id) && o.IsActive && !o.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var ownerByUnit = owners
            .GroupBy(o => o.Unit.Id)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(o => o.StartDate).First());

        // 4. Load billing groups for these units
        var billingGroups = await session.Query<BillingGroup>()
            .Where(bg => bg.Condominium.Id == condominium.Id && !bg.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var bg in billingGroups)
            await NHibernate.NHibernateUtil.InitializeAsync(bg.Units, cancellationToken)
                .ConfigureAwait(false);

        // Map unitId → billingGroup
        var billingGroupByUnit = new Dictionary<int, BillingGroup>();
        foreach (var bg in billingGroups)
            foreach (var u in bg.Units.Where(u => !u.IsDeleted))
                billingGroupByUnit[u.Id] = bg;

        // 5. Build address line
        var addressLine = address != null
            ? $"{address.Street} {address.StreetNumber}, {address.PostalCode} {address.City} ({address.Province})"
            : string.Empty;

        // 6. Group fees: billing-group units → aggregated notice; ungrouped → individual notice
        var notices = new List<PaymentNoticeData>();

        // Fees belonging to a billing group
        var feesByGroup = fees
            .Where(f => billingGroupByUnit.ContainsKey(f.Unit.Id))
            .GroupBy(f => billingGroupByUnit[f.Unit.Id].Id)
            .ToList();

        foreach (var group in feesByGroup)
        {
            var bg        = billingGroups.First(b => b.Id == group.Key);
            var groupFees = group.ToList();

            // Representative owner: first unit's owner in the group
            var firstUnitId = groupFees.First().Unit.Id;
            ownerByUnit.TryGetValue(firstUnitId, out var repOwner);

            // Unit description: all internal numbers joined
            var unitNumbers = string.Join(" + ", groupFees.Select(f => f.Unit.InternalNumber ?? f.Unit.Id.ToString()));

            notices.Add(new PaymentNoticeData
            {
                CondominiumName    = condominium.Name,
                CondominiumCode    = condominium.Code ?? string.Empty,
                CondominiumTaxCode = condominium.TaxCode ?? string.Empty,
                CondominiumAddress = addressLine,
                CondominiumEmail   = condominium.Email ?? string.Empty,
                CondominiumPhone   = condominium.Phone ?? string.Empty,
                Iban               = condominium.Iban ?? string.Empty,
                BankAccountHolder  = condominium.BankAccountHolder ?? string.Empty,
                BankName           = condominium.BankName ?? string.Empty,
                AdministratorName  = condominium.AdministratorName ?? string.Empty,
                AdministratorPhone = condominium.AdministratorPhone ?? string.Empty,
                AdministratorEmail = condominium.AdministratorEmail ?? string.Empty,
                FiscalYearCode     = fiscalYear.Name ?? fiscalYear.Id.ToString(),
                UnitInternalNumber = unitNumbers,
                UnitDisplayName    = bg.Name,
                UnitStaircase      = string.Empty,
                UnitFloor          = 0,
                OwnerFullName      = bg.Name,
                OwnerEmail         = bg.ContactEmail ?? repOwner?.Email ?? string.Empty,
                Rows =
                [
                    new PaymentNoticeRow
                    {
                        InstallmentNumber = installment.InstallmentNumber,
                        DueDate           = installment.DueDate,
                        AmountDue         = groupFees.Sum(f => f.AmountDue),
                        AmountPaid        = groupFees.Sum(f => f.AmountPaid),
                        Balance           = groupFees.Sum(f => f.Balance),
                        PaymentStatus     = groupFees.Any(f => f.Balance > 0) ? "Aperta" : "Saldata",
                        PaymentDate       = groupFees.Max(f => f.PaymentDate),
                        PaymentCode       = string.Join(" / ", groupFees.Select(f => f.PaymentCode).Where(c => !string.IsNullOrEmpty(c))),
                    }
                ],
            });
        }

        // Individual notices for units not in any billing group
        var ungroupedFees = fees.Where(f => !billingGroupByUnit.ContainsKey(f.Unit.Id)).ToList();
        foreach (var fee in ungroupedFees)
        {
            var unit = fee.Unit;
            ownerByUnit.TryGetValue(unit.Id, out var owner);

            notices.Add(new PaymentNoticeData
            {
                CondominiumName    = condominium.Name,
                CondominiumCode    = condominium.Code ?? string.Empty,
                CondominiumTaxCode = condominium.TaxCode ?? string.Empty,
                CondominiumAddress = addressLine,
                CondominiumEmail   = condominium.Email ?? string.Empty,
                CondominiumPhone   = condominium.Phone ?? string.Empty,
                Iban               = condominium.Iban ?? string.Empty,
                BankAccountHolder  = condominium.BankAccountHolder ?? string.Empty,
                BankName           = condominium.BankName ?? string.Empty,
                AdministratorName  = condominium.AdministratorName ?? string.Empty,
                AdministratorPhone = condominium.AdministratorPhone ?? string.Empty,
                AdministratorEmail = condominium.AdministratorEmail ?? string.Empty,
                FiscalYearCode     = fiscalYear.Name ?? fiscalYear.Id.ToString(),
                UnitInternalNumber = unit.InternalNumber ?? string.Empty,
                UnitDisplayName    = unit.DisplayName    ?? string.Empty,
                UnitStaircase      = unit.Staircase?.Name ?? string.Empty,
                UnitFloor          = unit.Floor,
                OwnerFullName      = owner != null ? $"{owner.FirstName} {owner.LastName}".Trim() : string.Empty,
                OwnerEmail         = owner?.Email ?? string.Empty,
                Rows =
                [
                    new PaymentNoticeRow
                    {
                        InstallmentNumber = installment.InstallmentNumber,
                        DueDate           = installment.DueDate,
                        AmountDue         = fee.AmountDue,
                        AmountPaid        = fee.AmountPaid,
                        Balance           = fee.Balance,
                        PaymentStatus     = fee.PaymentStatus ?? string.Empty,
                        PaymentDate       = fee.PaymentDate,
                        PaymentCode       = fee.PaymentCode,
                    }
                ],
            });
        }

        // 7. Sort notices by owner name and generate PDF
        notices = notices.OrderBy(n => n.OwnerFullName).ToList();

        var logo = await TenantLogoProvider
            .GetLogoForCondominiumAsync(session, condominium, cancellationToken)
            .ConfigureAwait(false);
        foreach (var n in notices) n.LogoContent = logo;

        var document = new PaymentNoticeDocument(notices);
        return document.GeneratePdf();
    }
}
