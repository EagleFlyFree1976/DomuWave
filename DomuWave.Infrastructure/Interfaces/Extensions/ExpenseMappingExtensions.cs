using CPQ.Core.Extensions;
using DomuWave.Services.Dto.Expense;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class ExpenseMappingExtensions
{
    public static ExpenseReadDto ToReadDto(this Expense entity)
    {
        if (entity == null) return null;
        var dto = new ExpenseReadDto
        {
            CondominiumId           = entity.Condominium?.Id ?? 0,
            SupplierId              = entity.Supplier?.Id,
            SupplierName            = entity.Supplier?.Name,
            AccountId               = entity.Account?.Id ?? 0,
            AccountName             = entity.Account?.Name ?? string.Empty,
            MillesimalTableId       = entity.MillesimalTable?.Id ?? 0,
            MillesimalTableName     = entity.MillesimalTable?.Name ?? string.Empty,
            Name                    = entity.Name ?? string.Empty,
            DocumentNumber          = entity.DocumentNumber,
            DocumentDate            = entity.DocumentDate,
            RegistrationDate        = entity.RegistrationDate,
            GrossAmount             = entity.GrossAmount,
            VatAmount               = entity.VatAmount,
            NetAmount               = entity.NetAmount,
            ExpenseTypeId           = entity.ExpenseType?.Id ?? 0,
            ExpenseTypeName         = entity.ExpenseType?.Name ?? string.Empty,
            PaymentStatusId         = entity.PaymentStatus?.Id ?? 0,
            PaymentStatusName       = entity.PaymentStatus?.Name ?? string.Empty,
            PaymentDate             = entity.PaymentDate,
            PaymentMethod           = entity.PaymentMethod,
            Description             = entity.Description,
            ChargeabilityTypeId     = entity.ChargeabilityType?.Id ?? ChargeabilityType.Owner,
            ChargeabilityTypeName   = entity.ChargeabilityType?.Name ?? string.Empty,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static Expense ToEntity(this CreateExpenseDto dto,
        Condominium condominium, FiscalYear fiscalYear, ChartOfAccounts account,
        MillesimalTable millesimalTable, Supplier? supplier,
        ExpenseType expenseType, ExpensePaymentStatus paymentStatus,
        ChargeabilityType chargeabilityType)
    {
        return new Expense
        {
            Tenant             = condominium.Tenant,
            Condominium        = condominium,
            FiscalYear         = fiscalYear,
            Account            = account,
            MillesimalTable    = millesimalTable,
            Supplier           = supplier,
            Name               = dto.Name,
            DocumentNumber     = dto.DocumentNumber,
            DocumentDate       = dto.DocumentDate,
            RegistrationDate   = dto.RegistrationDate,
            GrossAmount        = dto.GrossAmount,
            VatAmount          = dto.VatAmount,
            NetAmount          = dto.NetAmount,
            ExpenseType        = expenseType,
            PaymentStatus      = paymentStatus,
            PaymentMethod      = dto.PaymentMethod,
            Description        = dto.Description,
            ChargeabilityType  = chargeabilityType,
        };
    }

    public static void ApplyUpdate(this Expense entity, UpdateExpenseDto dto,
        ChartOfAccounts account, MillesimalTable millesimalTable, Supplier? supplier,
        ExpenseType expenseType, ExpensePaymentStatus paymentStatus,
        ChargeabilityType chargeabilityType)
    {
        entity.Account            = account;
        entity.MillesimalTable    = millesimalTable;
        entity.Supplier           = supplier;
        entity.Name               = dto.Name;
        entity.DocumentNumber     = dto.DocumentNumber;
        entity.DocumentDate       = dto.DocumentDate;
        entity.RegistrationDate   = dto.RegistrationDate;
        entity.GrossAmount        = dto.GrossAmount;
        entity.VatAmount          = dto.VatAmount;
        entity.NetAmount          = dto.NetAmount;
        entity.ExpenseType        = expenseType;
        entity.PaymentStatus      = paymentStatus;
        entity.PaymentMethod      = dto.PaymentMethod;
        entity.Description        = dto.Description;
        entity.ChargeabilityType  = chargeabilityType;
    }
}
