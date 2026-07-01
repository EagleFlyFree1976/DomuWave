using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.BillingGroup;
using DomuWave.Services.Dto.PaymentNotice;
using DomuWave.Services.Models;
using DomuWave.Services.Pdf;
using NHibernate.Linq;
using QuestPDF.Fluent;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

// ── Avviso intero esercizio per billing group ─────────────────────────────────

public class GetBillingGroupPaymentNoticeCommandConsumer
    : InMemoryConsumerBase<GetBillingGroupPaymentNoticeCommand, byte[]>
{
    private readonly IUserService _userService;

    public GetBillingGroupPaymentNoticeCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<byte[]> Consume(
        GetBillingGroupPaymentNoticeCommand command,
        IMediationContext                    mediationContext,
        CancellationToken                    cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // 1. Load billing group with units
        var billingGroup = await session.Query<BillingGroup>()
            .Where(bg => bg.Id == command.BillingGroupId && !bg.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (billingGroup == null)
            throw new NotFoundException("Gruppo di fatturazione non trovato.");

        await NHibernate.NHibernateUtil.InitializeAsync(billingGroup.Units, cancellationToken)
            .ConfigureAwait(false);

        var unitIds = billingGroup.Units.Where(u => !u.IsDeleted).Select(u => u.Id).ToList();
        if (unitIds.Count == 0)
            throw new ValidatorException("Il gruppo non contiene unità.");

        var condominium = billingGroup.Condominium;
        var address     = condominium.Address;

        // 2. Load fiscal year
        var fiscalYear = await session.Query<FiscalYear>()
            .Where(fy => fy.Id == command.FiscalYearId && !fy.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (fiscalYear == null)
            throw new NotFoundException("Esercizio fiscale non trovato.");

        // 3. Load all fees for units in the group for this fiscal year
        var fees = await session.Query<CondominiumFee>()
            .Where(f => unitIds.Contains(f.Unit.Id)
                     && f.Installment.FiscalYear.Id == command.FiscalYearId
                     && !f.IsDeleted)
            .OrderBy(f => f.Unit.InternalNumber)
            .ThenBy(f => f.Installment.InstallmentNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (fees.Count == 0)
            throw new ValidatorException("Nessuna quota trovata per questo gruppo nell'esercizio selezionato.");

        // 4. Load active owners per unit
        var owners = await session.Query<UnitOwner>()
            .Where(o => unitIds.Contains(o.Unit.Id) && o.IsActive && !o.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var ownerByUnit = owners
            .GroupBy(o => o.Unit.Id)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.StartDate).First());

        var addressLine = address != null
            ? $"{address.Street} {address.StreetNumber}, {address.PostalCode} {address.City} ({address.Province})"
            : string.Empty;

        // 5. Build one PaymentNoticeData per unit (all installments)
        var notices = fees
            .GroupBy(f => f.Unit.Id)
            .Select(g =>
            {
                var unit = g.First().Unit;
                ownerByUnit.TryGetValue(unit.Id, out var owner);
                return new PaymentNoticeData
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
                    Rows = g.OrderBy(f => f.Installment.InstallmentNumber)
                             .Select(f => new PaymentNoticeRow
                             {
                                 InstallmentNumber = f.Installment.InstallmentNumber,
                                 DueDate           = f.Installment.DueDate,
                                 AmountDue         = f.AmountDue,
                                 AmountPaid        = f.AmountPaid,
                                 Balance           = f.Balance,
                                 PaymentStatus     = f.PaymentStatus ?? string.Empty,
                                 PaymentDate       = f.PaymentDate,
                                 PaymentCode       = f.PaymentCode,
                             }).ToList(),
                };
            })
            .OrderBy(n => n.UnitInternalNumber)
            .ToList();

        var logo = await TenantLogoProvider
            .GetLogoForCondominiumAsync(session, condominium, cancellationToken)
            .ConfigureAwait(false);
        foreach (var n in notices) n.LogoContent = logo;

        var document = new PaymentNoticeDocument(notices);
        return document.GeneratePdf();
    }
}

// ── Avviso singola rata per billing group ─────────────────────────────────────

public class GetBillingGroupInstallmentNoticeCommandConsumer
    : InMemoryConsumerBase<GetBillingGroupInstallmentNoticeCommand, byte[]>
{
    private readonly IUserService _userService;

    public GetBillingGroupInstallmentNoticeCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<byte[]> Consume(
        GetBillingGroupInstallmentNoticeCommand command,
        IMediationContext                        mediationContext,
        CancellationToken                        cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // 1. Load billing group with units
        var billingGroup = await session.Query<BillingGroup>()
            .Where(bg => bg.Id == command.BillingGroupId && !bg.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (billingGroup == null)
            throw new NotFoundException("Gruppo di fatturazione non trovato.");

        await NHibernate.NHibernateUtil.InitializeAsync(billingGroup.Units, cancellationToken)
            .ConfigureAwait(false);

        var unitIds = billingGroup.Units.Where(u => !u.IsDeleted).Select(u => u.Id).ToList();
        if (unitIds.Count == 0)
            throw new ValidatorException("Il gruppo non contiene unità.");

        // 2. Load installment
        var installment = await session.Query<CondominiumInstallment>()
            .Where(i => i.Id == command.InstallmentId && !i.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (installment == null)
            throw new NotFoundException("Rata non trovata.");

        var condominium = billingGroup.Condominium;
        var address     = condominium.Address;
        var fiscalYear  = installment.FiscalYear;

        // 3. Load fees for these units in this installment
        var fees = await session.Query<CondominiumFee>()
            .Where(f => unitIds.Contains(f.Unit.Id)
                     && f.Installment.Id == command.InstallmentId
                     && !f.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (fees.Count == 0)
            throw new ValidatorException("Nessuna quota trovata per questo gruppo in questa rata.");

        // 4. Load representative owner (first unit)
        var firstUnitId = fees.First().Unit.Id;
        var repOwner = await session.Query<UnitOwner>()
            .Where(o => o.Unit.Id == firstUnitId && o.IsActive && !o.IsDeleted)
            .OrderByDescending(o => o.StartDate)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var addressLine = address != null
            ? $"{address.Street} {address.StreetNumber}, {address.PostalCode} {address.City} ({address.Province})"
            : string.Empty;

        var unitNumbers = string.Join(" + ", fees.Select(f => f.Unit.InternalNumber ?? f.Unit.Id.ToString()));

        // 5. Build a single aggregated notice for the group
        var notice = new PaymentNoticeData
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
            UnitDisplayName    = billingGroup.Name,
            UnitStaircase      = string.Empty,
            UnitFloor          = 0,
            OwnerFullName      = billingGroup.Name,
            OwnerEmail         = billingGroup.ContactEmail ?? repOwner?.Email ?? string.Empty,
            Rows =
            [
                new PaymentNoticeRow
                {
                    InstallmentNumber = installment.InstallmentNumber,
                    DueDate           = installment.DueDate,
                    AmountDue         = fees.Sum(f => f.AmountDue),
                    AmountPaid        = fees.Sum(f => f.AmountPaid),
                    Balance           = fees.Sum(f => f.Balance),
                    PaymentStatus     = fees.Any(f => f.Balance > 0) ? "Aperta" : "Saldata",
                    PaymentDate       = fees.Max(f => f.PaymentDate),
                    PaymentCode       = string.Join(" / ", fees.Select(f => f.PaymentCode).Where(c => !string.IsNullOrEmpty(c))),
                }
            ],
        };

        notice.LogoContent = await TenantLogoProvider
            .GetLogoForCondominiumAsync(session, condominium, cancellationToken)
            .ConfigureAwait(false);

        var document = new PaymentNoticeDocument(new[] { notice });
        return document.GeneratePdf();
    }
}
