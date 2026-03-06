using CPQ.Core.Extensions;
using DomuWave.Services.Dto.CondominiumFee;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class CondominiumFeeMappingExtensions
{
    public static CondominiumFeeReadDto ToReadDto(this CondominiumFee entity)
    {
        if (entity == null) return null;
        var dto = new CondominiumFeeReadDto
        {
            InstallmentId = entity.Installment?.Id ?? 0,
            UnitId        = entity.Unit?.Id ?? 0,
            UnitName      = entity.Unit?.Name,
            UserId        = entity.UserId,
            AmountDue     = entity.AmountDue,
            AmountPaid    = entity.AmountPaid,
            Balance       = entity.Balance,
            PaymentStatus = entity.PaymentStatus ?? string.Empty,
            PaymentDate   = entity.PaymentDate,
            PaymentMethod = entity.PaymentMethod,
            Notes         = entity.Notes,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static CondominiumFee ToEntity(this CreateCondominiumFeeDto dto,
        CondominiumInstallment installment, RealEstateUnit unit)
    {
        return new CondominiumFee
        {
            Tenant        = installment.Tenant,
            Installment   = installment,
            Unit          = unit,
            UserId        = dto.UserId,
            AmountDue     = dto.AmountDue,
            AmountPaid    = 0,
            Balance       = dto.AmountDue,
            PaymentStatus = "ToPay",
            Notes         = dto.Notes,
        };
    }
}
