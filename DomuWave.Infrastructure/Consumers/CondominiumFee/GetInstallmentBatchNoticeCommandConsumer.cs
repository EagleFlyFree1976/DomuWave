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
            .Where(f => f.Installment.Id == command.InstallmentId && !f.IsDeleted)
            .OrderBy(f => f.Unit.InternalNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (fees.Count == 0)
            throw new ValidatorException("Nessuna quota trovata per questa rata.");

        // 3. Load all active owners for the units in this installment, grouped by unit
        var unitIds = fees.Select(f => f.Unit.Id).Distinct().ToList();

        var owners = await session.Query<UnitOwner>()
            .Where(o => unitIds.Contains(o.Unit.Id) && o.IsActive && !o.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var ownerByUnit = owners
            .GroupBy(o => o.Unit.Id)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(o => o.StartDate).First());

        // 4. Build address line
        var addressLine = address != null
            ? $"{address.Street} {address.StreetNumber}, {address.PostalCode} {address.City} ({address.Province})"
            : string.Empty;

        // 5. Build one PaymentNoticeData per unit
        var notices = fees.Select(fee =>
        {
            var unit = fee.Unit;
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
                UnitDisplayName    = unit.DisplayName ?? string.Empty,
                UnitStaircase      = unit.Staircase ?? string.Empty,
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
                    }
                ],
            };
        }).ToList();

        // 6. Generate multi-page PDF
        var document = new PaymentNoticeDocument(notices);
        return document.GeneratePdf();
    }
}
