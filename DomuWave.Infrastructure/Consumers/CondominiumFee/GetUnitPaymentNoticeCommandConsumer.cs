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

public class GetUnitPaymentNoticeCommandConsumer
    : InMemoryConsumerBase<GetUnitPaymentNoticeCommand, byte[]>
{
    private readonly IUserService _userService;

    public GetUnitPaymentNoticeCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<byte[]> Consume(
        GetUnitPaymentNoticeCommand command,
        IMediationContext            mediationContext,
        CancellationToken            cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // 1. Load unit (with condominium + address)
        var unit = await session.Query<RealEstateUnit>()
            .Where(u => u.Id == command.UnitId && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (unit == null)
            throw new NotFoundException("Unità immobiliare non trovata.");

        var condominium = unit.Condominium;
        var address     = condominium.Address;

        // 2. Load fiscal year
        var fiscalYear = await session.Query<FiscalYear>()
            .Where(fy => fy.Id == command.FiscalYearId && !fy.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (fiscalYear == null)
            throw new NotFoundException("Esercizio fiscale non trovato.");

        // 3. Load active owner (most recent active one)
        var owner = await session.Query<UnitOwner>()
            .Where(o => o.Unit.Id == command.UnitId && o.IsActive && !o.IsDeleted)
            .OrderByDescending(o => o.StartDate)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        // 4. Load all fees for this unit in this fiscal year
        var fees = await session.Query<CondominiumFee>()
            .Where(f => f.Unit.Id == command.UnitId
                     && f.Installment.FiscalYear.Id == command.FiscalYearId
                     && !f.IsDeleted)
            .OrderBy(f => f.Installment.InstallmentNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // 5. Build PaymentNoticeData
        var addressLine = address != null
            ? $"{address.Street} {address.StreetNumber}, {address.PostalCode} {address.City} ({address.Province})"
            : string.Empty;

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
            UnitInternalNumber = unit.InternalNumber ?? string.Empty,
            UnitDisplayName    = unit.DisplayName ?? string.Empty,
            UnitStaircase      = unit.Staircase?.Name ?? string.Empty,
            UnitFloor          = unit.Floor,
            OwnerFullName      = owner != null ? $"{owner.FirstName} {owner.LastName}".Trim() : string.Empty,
            OwnerEmail         = owner?.Email ?? string.Empty,
            Rows = fees.Select(f => new PaymentNoticeRow
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

        notice.LogoContent = await TenantLogoProvider
            .GetLogoForCondominiumAsync(session, condominium, cancellationToken)
            .ConfigureAwait(false);

        // 6. Generate PDF
        var document = new PaymentNoticeDocument(new[] { notice });
        return document.GeneratePdf();
    }
}

public class GetUnitInstallmentPaymentNoticeCommandConsumer
    : InMemoryConsumerBase<GetUnitInstallmentPaymentNoticeCommand, byte[]>
{
    private readonly IUserService _userService;

    public GetUnitInstallmentPaymentNoticeCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<byte[]> Consume(
        GetUnitInstallmentPaymentNoticeCommand command,
        IMediationContext                       mediationContext,
        CancellationToken                       cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var unit = await session.Query<RealEstateUnit>()
            .Where(u => u.Id == command.UnitId && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (unit == null)
            throw new NotFoundException("Unità immobiliare non trovata.");

        var installment = await session.Query<CondominiumInstallment>()
            .Where(i => i.Id == command.InstallmentId && !i.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (installment == null)
            throw new NotFoundException("Rata non trovata.");

        var fee = await session.Query<CondominiumFee>()
            .Where(f => f.Unit.Id == command.UnitId
                     && f.Installment.Id == command.InstallmentId
                     && !f.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (fee == null)
            throw new ValidatorException("Nessuna quota trovata per questa unità in questa rata.");

        var condominium = unit.Condominium;
        var address     = condominium.Address;
        var fiscalYear  = installment.FiscalYear;

        var owner = await session.Query<UnitOwner>()
            .Where(o => o.Unit.Id == command.UnitId && o.IsActive && !o.IsDeleted)
            .OrderByDescending(o => o.StartDate)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var addressLine = address != null
            ? $"{address.Street} {address.StreetNumber}, {address.PostalCode} {address.City} ({address.Province})"
            : string.Empty;

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
        };

        notice.LogoContent = await TenantLogoProvider
            .GetLogoForCondominiumAsync(session, condominium, cancellationToken)
            .ConfigureAwait(false);

        var document = new PaymentNoticeDocument(new[] { notice });
        return document.GeneratePdf();
    }
}
