using System.Globalization;
using System.Security.Cryptography;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.Beneficiary;
using DomuWave.Services.Models.Dto.Category;
using DomuWave.Services.Models.Dto.Currency;
using DomuWave.Services.Models.Dto.Import;
using DomuWave.Services.Models.Dto.PaymentMethod;
using DomuWave.Services.Models.Dto.Transaction;
using DomuWave.Services.Models.Import;
using Bogus.DataSets;
using CPQ.Core;
using CPQ.Core.DTO;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using DocumentFormat.OpenXml.Math;
using NHibernate.Engine.Query;
using NHibernate.Proxy;
using Currency = DomuWave.Services.Models.Currency;

namespace DomuWave.Services.Extensions;

public static class DtoExtensions
{
    public static BookEntityDto<T> fillBookEntityData<T>(this BookEntityDto<T> bookEntityDto, BookEntity<T> bookEntity)
    {
        bookEntityDto.Id = bookEntity.Id;
        if (bookEntity.Book != null && !bookEntity.Book.IsSystem)
        {
            bookEntityDto.BookId = bookEntity.Book.Id;
        }

        bookEntityDto.Name = bookEntity.Name;
        bookEntityDto.Description= bookEntity.Description;
        bookEntityDto.fillTraceData(bookEntity);

        return bookEntityDto;
    }
    public static TraceEntityDTO<T> fillTraceData<T>(this TraceEntityDTO<T> traceEntityDto, TraceEntity<T> traceEntity)
    {
        

        traceEntityDto.LastUpdatedByFullName = traceEntity.LastUpdatedByFullName;
        traceEntityDto.CreatedByFullName = traceEntity.CreatedByFullName;
        traceEntityDto.CreatedById = traceEntity.CreatedBy;
        traceEntityDto.LastUpdatedById= traceEntity.LastUpdatedBy;
        traceEntityDto.CreationDate = traceEntity.CreationDate.ToUniversalTime();
        traceEntityDto.LastUpdateDate = traceEntity.LastUpdateDate.ToUniversalTime();

        return traceEntityDto;
    }

    public static ImportDto ToDto(this Import import)
    {
        ImportDto importDto = new ImportDto()
        {
            BookId = import.Book.Id,

            Id = import.Id,
            Description = import.Description,
            TargetAccount = import.TargetAccount.ToLookupEntity(),
            Configuration = import.Configuration.NotIsNullOrEmpty()
                ? import.Configuration.parseJson<ImportConfigurationDto>()
                : null,
            IsEnabled = import.IsEnabled,

        };
        importDto.SetTraceInfo(import);
        return importDto;
    }
    
    public static ImportMinDto ToMinDto(this Import import)
    {

        ImportMinDto importDto = new ImportMinDto()
        {
            BookId = import.Book.Id,

            Id = import.Id,
            Description = import.Description,
            TargetAccount = import.TargetAccount.ToLookupEntity(),
             
            IsEnabled = import.IsEnabled,

        };
        importDto.SetTraceInfo(import);
        return importDto;
    }

    public static BookDto ToEditDto(this Book item)
    {
        return new BookDto()
        {
            Description = item.Description, Name = item.Name, OwnerId = item.OwnerId, IsPrimary = item.IsPrimary
        };
    }

    

    public static BookReadDto ToDto(this Book item)
    {
        if (item == null) return null;

        BookReadDto dto = new BookReadDto()
        {
            Description = item.Description,
            Id = item.Id, Name = item.Name, OwnerId = item.OwnerId, IsPrimary = item.IsPrimary
        };
        dto.fillTraceData(item);
        return dto;
    }

    public static AccountTypeReadDto ToDto(this AccountType item)
    {
        if (item == null) return null;


        AccountTypeReadDto readDto = new AccountTypeReadDto()
        {
            Id = item.Id,

            Code = item.Code,Description = item.Description
        };
        readDto.fillTraceData(item);
        return readDto;

    }
    
    public static CurrencyReadDto ToDto(this Currency item)
    {
        if (item == null) return null;


        CurrencyReadDto readDto = new CurrencyReadDto()
        {
            Code = item.Code, Id = item.Id, Name = item.Name,
            IsEnabled = item.IsEnabled, Symbol = item.Symbol,
            DecimalDigits = item.DecimalDigits
        };
        readDto.fillTraceData(item);
        return readDto;

    }
    public static string FormatAmout(this decimal? amount, Currency currency)
    {
        
        if (!amount.HasValue) return string.Empty;
        var ci = (CultureInfo)CultureInfo.GetCultureInfo("it-IT").Clone();
        ci.NumberFormat.CurrencySymbol = currency.Symbol;
        if (ci != null)
        {
            string formatted = amount.Value.ToString("C", ci).Trim();
            return formatted;
        }
        return amount.ToString();
    }

    public static AccountReadDto ToDto(this Account item)
    {
        if (item == null) return null;

        AccountReadDto dto = new AccountReadDto()
        {
            Description = item.Description,
            Id = item.Id, Name = item.Name, OwnerId = item.OwnerId
            , 
            Currency = new LookupEntityDto<int>(){Code = item.Currency.Code, Description = item.Currency.Name, Id = item.Currency.Id},
            InitialBalance = item.InitialBalance, 
            OpenDate = item.OpenDate.ToUniversalTime(), ClosedDate = item.ClosedDate?.ToUniversalTime(), Book = item.Book.ToDto(), IsActive = item.IsActive,
            AccountType = new LookupEntityDto<int>()
            {
                Code = item.AccountType.Code, Description = item.AccountType.Description, Id = item.AccountType.Id
            }

        };

        
        dto.fillTraceData(item);
        return dto;
    }
    public static MenuItemDto ToDto(this MenuItem item)
    {
        if (item == null) return null;

        MenuItemDto dto = new MenuItemDto
        {
            Id = item.Id,
            Icon = item.Icon,
            ParentMenuId = item.ParentMenuId,
            Action = item.Action,
            AuthorizationCode= item.AuthorizationCode,
            Description = item.Description
        };
        return dto;
    }


    public static PaymentMethodReadDto ToDto(this PaymentMethod item)
    {
        var dto = new PaymentMethodReadDto();
        dto.fillBookEntityData(item);
        dto.Enabled = item.IsEnabled;
        return dto;

    }

    public static CategoryMinReadDto ToMinDto(this Category item)
    {
        var dto = new CategoryMinReadDto();
        dto.Name = item.Name;
        dto.Id = item.Id;
        
        if (item.ParentCategory != null)
        {
            dto.Parent = item.ParentCategory.ToMinDto();
        }
        return dto;
    }
    public static CategoryReadDto ToDto(this Category item)
    {
        var dto = new CategoryReadDto();
        dto.fillBookEntityData(item);
        dto.Enabled = item.IsEnabled;
        if (item.ParentCategory != null)
        {
            dto.Parent = item.ParentCategory.ToMinDto();
        }
        return dto;

    }
    public static LookupEntityDto<long> ToLookupEntity(this Account account)
    {
        if (account == null)
            return null;

        var dto = new LookupEntityDto<long>();

        dto.Code = account.Name;
        dto.Description = account.Description;
        dto.Id = account.Id;
        return dto;
    }
    public static LookupEntityDto<int> ToLookupEntity(this PaymentMethod paymentMethod)
    {
        if (paymentMethod == null)
            return null;

        var dto = new LookupEntityDto<int>();

        dto.Code = paymentMethod.Name;
        dto.Description = paymentMethod.Description;
        dto.Id = paymentMethod.Id;
        return dto;
    }
    public static LookupEntityDto<int> ToLookupEntity(this Currency currency)
    {
        if (currency == null)
            return null;

        var dto = new LookupEntityDto<int>();

        dto.Code = currency.Code;
        dto.Description = currency.Name;
        dto.Id = currency.Id;
        return dto;
    }

    public static LookupEntityDto<long> ToLookupEntity(this Beneficiary beneficiary)
    {
        if (beneficiary == null)
            return null;

        var dto = new LookupEntityDto<long>();

        dto.Code = beneficiary.Name;
        dto.Description = beneficiary.Name;
        dto.Id = beneficiary.Id;
        
        return dto;
    }
    public static BeneficiaryReadDto ToDto(this Beneficiary item)
    {
        if (item == null)
            return null;

        var dto = new BeneficiaryReadDto();

        dto.Category = item.Category!= null ? item.Category.ToMinDto() : null;
        dto.Description = item.Description;
        dto.Name = item.Name;
        if (dto.Description.IsNullOrEmpty())
            dto.Description = item.Name;
        dto.Notes = item.Notes;
        dto.Iban = item.Iban;
        dto.IsEnabled = item.IsEnabled;
        dto.fillBookEntityData(item);
        
        dto.Id = item.Id;
        return dto;
    }

    public static ExchangeRateHistoryReadDto ToDto(this ExchangeRateHistory item)
    {
        if (item == null) return null;
        ExchangeRateHistoryReadDto dto = new ExchangeRateHistoryReadDto()
        {
            Id = item.Id,
            Rate = item.Rate,
            ValidFrom = item.ValidFrom,
            ValidTo = item.ValidTo,
            FromCurrency = item.FromCurrency.ToLookupEntity(),
            ToCurrency = item.ToCurrency.ToLookupEntity()
        };
        dto.fillTraceData(item);
        return dto;
    }
    public static (DateTime from, DateTime to) ToCurrencyRange(this DateTime targetDate)
    {
        (DateTime from, DateTime to) retValue;

        retValue.from = targetDate.Date;
        retValue.to = targetDate.Date.AddDays(1).AddSeconds(-1);
        return  retValue;
    }

    public static LookupEntityDtoExtended<int> ToLookupEntityDto(this TransactionStatus item)
    {
        return new LookupEntityDtoExtended<int>()
        {
            Code = item.Code,
            Description = item.Description,
            Id = item.Id,
            CssClass = item.CssClass
        };
    }
    public static BeneficiaryLookupReadDto ToLookupEntityDto(this BeneficiaryReadDto item)
    {
        return new BeneficiaryLookupReadDto()
        {
            Code = item.Name,
            Description = string.IsNullOrEmpty(item.Description) ? item.Name : item.Description,
            Id = item.Id,
            Category = item.Category
        };
    }
    public static LookupEntityDto<long> ToLookupEntityDto(this CategoryReadDto item)
    {
        return new LookupEntityDto<long>()
        {
            Code = item.Name,
            Description = string.IsNullOrEmpty(item.Description) ? item.Name : item.Description,
            Id = item.Id,
            
        };
    }

    public static TransactionUpdateDto ToUpdateDto(this Transaction item)
    {
        TransactionUpdateDto returnDto = new TransactionUpdateDto();

        returnDto.Description = item.Description;
        returnDto.AccountId = item.Account.Id;
        returnDto.Amount = item.Amount;
        returnDto.CurrencyId = item.Currency?.Id;
        returnDto.Beneficiary = item.Beneficiary?.ToLookupEntity();
        returnDto.PaymentMethodId = item.PaymentMethod?.Id;
        returnDto.DestinationAccountId = item.DestinationAccount?.Id;
        returnDto.CategoryId = item.Category?.Id ?? 0;
        returnDto.TransactionDate = item.TransactionDate;
        returnDto.TransactionType = item.TransactionType;
        returnDto.Status = item.Status.Id;
        

        return returnDto;
    }
    public static TransactionReadDto ToDto(this Transaction item)
    {
        return item.ToDto(null);
    }
    public static TransactionReadDto ToDto(this Transaction item, int? stringMaxLength)
    {
        if (item == null) return null;

        string desc = item.Description;

        if (stringMaxLength.HasValue && stringMaxLength.Value > 0 && desc.NotIsNullOrEmpty())
        {
            if (desc.Length > stringMaxLength.Value)
            {
                desc = $"{desc.Substring(0, stringMaxLength.Value)}...";
            }
        }

        TransactionReadDto dto = new TransactionReadDto()
        {
            Id = item.Id,
            Description = desc,
            Amount = item.Amount,
            AmountInAccountCurrency = item.AmountInAccountCurrency,
            FlowDirection = item.FlowDirection,
            BookId = item.Book.Id,
            Account = item.Account.ToLookupEntity(),
            DestinationAccount = item.DestinationAccount != null ? item.DestinationAccount.ToLookupEntity() : null,
            IsEnabled = item.IsEnabled,
            Category = item.Category?.ToMinDto(),
            PaymentMethod = item.PaymentMethod?.ToLookupEntity(),
            Beneficiary = item.Beneficiary?.ToLookupEntity(),
            TransactionDate = item.TransactionDate,
            TransactionType = item.TransactionType,
            Rate = item.AccountCurrencyExchangeRate,
            Status = item.Status.ToLookupEntityDto(),
            Currency = item.Currency?.ToLookupEntity(),
            AccountCurrency = item.Account.Currency.ToLookupEntity(),
            Tags = item.Tags?.Select(k => k.Tag.Name).ToList(),
            Name = item.Name,
            AccountBalance = item.AccountBalance
        };

        dto.fillTraceData(item);
        return dto;
    }

}