using System.Globalization;
using DomuWave.Services.Command.Beneficiary;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.Import;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Category;
using DomuWave.Services.Models.Dto.Currency;
using DomuWave.Services.Models.Dto.Import;
using DomuWave.Services.Models.Import;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using CsvHelper;
using MassTransit.Initializers;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class ValidateImportCommandConsumer : InMemoryConsumerBase<ValidateImportCommand, ImportDto>
{
    private IUserService _userService;
    private IImportService _importService;
    private IMediator mediator ;


    public ValidateImportCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IImportService importService, IUserService userService, IMediator mediator) : base(sessionFactoryProvider)
    {
        _importService = importService;
        _userService = userService;
        this.mediator = mediator;
    }

    protected override async Task<ImportDto> Consume(ValidateImportCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var toProcess = await session.GetAsync<Models.Import.Import>(evt.ImportId, cancellationToken)
            .ConfigureAwait(false);
        if (toProcess == null)
            throw new NotFoundException("Import non trovato");

        var importedRows = await session.Query<ImportTransaction>().Where(k => k.Import.Id == evt.ImportId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var defaultPaymentMethod = await session.Query<AccountPaymentMethod>()
            .Where(k => k.IsDefault && k.Account.Id == toProcess.TargetAccount.Id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (defaultPaymentMethod == null)
        {
            throw new ValidatorException("Non è stato impostato il metodo di pagamento di default");
        }
        if (!importedRows.Any())
        {
            throw new NotFoundException("Nessuna transazione da importare");
        }

        var importConfiguration = toProcess.ToDto().Configuration;

        var importTransactions = importedRows.Where(k=>
            k.ImportTransactionStatus != ImportTransactionStatus.Validated && k.ImportTransactionStatus != ImportTransactionStatus.Imported && k.ImportTransactionStatus != ImportTransactionStatus.Ignored).ToList();

        //var distinctCategories = importedRows.Where(row => !string.IsNullOrEmpty(row.InCategoryName)).ToList()
        //    .Select(row => row.InCategoryName).Distinct();

        List<Models.Beneficiary> allBeneficiaryEntity = await session.Query<Models.Beneficiary>()
            .Where(k => k.Book.Id == toProcess.Book.Id && !k.IsDeleted).ToListAsync(cancellationToken);

        var allBeneficiary = allBeneficiaryEntity.Select(k => k.ToDto()).ToList();
        var allCurrencies = await mediator.GetResponse(new FindCurrencyCommand(currentUser.Id), cancellationToken).ConfigureAwait(false);

        Dictionary<string, CategoryReadDto> categories = new Dictionary<string, CategoryReadDto>();
        Dictionary<string, CategoryReadDto> subCategories = new Dictionary<string, CategoryReadDto>();

        List<TransactionStatus> listTransactionStatus =
            await session.Query<TransactionStatus>().ToListAsync(cancellationToken).ConfigureAwait(false);
        CultureInfo cultureInfo = importConfiguration.CultureInfo.IsNullOrEmpty() ? System.Globalization.CultureInfo.InvariantCulture : new System.Globalization.CultureInfo(importConfiguration.CultureInfo);

        foreach (ImportTransaction row in importTransactions)
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrEmpty(row.InTransactionDate) ||
                !DateTime.TryParse(row.InTransactionDate,  out var transactionDate))
            {
                errors.Add($"Campo data non valido");
            }
            else
            {
                row.TransactionDate = transactionDate;
            }

            if (!string.IsNullOrEmpty(row.InDepositAmount))
            {
                if (decimal.TryParse(row.InDepositAmount, cultureInfo, out var depositAmount))
                {
                    row.DepositAmount = depositAmount;
                }
                else
                {
                    errors.Add($"Campo importo deposito non valido");
                }
            }
   
            if (!string.IsNullOrEmpty(row.InWithdrawalAmount))
            {
                if (decimal.TryParse(row.InWithdrawalAmount, cultureInfo, out var withdrawalAmount))
                {
                    row.WithdrawalAmount= withdrawalAmount;
                }
                else
                {
                    errors.Add($"Campo importo in uscita non valido");
                }
            }
            if (!string.IsNullOrEmpty(row.InAmount))
            {
                
                if (decimal.TryParse(row.InAmount, cultureInfo, out var amount))
                {
                    row.Amount= amount;
                }
                else
                {
                    errors.Add($"Campo importo non valido");
                }
            }



            if (!string.IsNullOrEmpty(row.InCategoryName))
            {
                CategoryReadDto category = null;
                if (categories.ContainsKey(row.InCategoryName))
                {
                    category = categories[row.InCategoryName];
                }
                else
                {
                    GetCategoryByNameCommand categoryByNameCommand =
                        new GetCategoryByNameCommand(currentUser.Id, toProcess.Book.Id) { Name = row.InCategoryName };

                    category = await mediator.GetResponse(categoryByNameCommand, cancellationToken)
                        .ConfigureAwait(false);
                    categories.Add(row.InCategoryName, category);
                }
                //if (category == null)
                //{
                //    CreateCategoryCommand createCategoryCommand =
                //        new CreateCategoryCommand(currentUser.Id, toProcess.Book.Id);
                //    createCategoryCommand.CreateDto = new CategoryCreateUpdateDto()
                //    {
                //        BookId = toProcess.Book.Id, Description = row.InCategoryName, Name = row.InCategoryName,
                //        ParentCategoryId = null
                //    };
                //    category = await mediator.GetResponse(createCategoryCommand, cancellationToken).ConfigureAwait(false);
                //}
                
                if (category != null)
                {
                    row.Category = new Models.Category() { Id = category.Id }; //await session.GetAsync<Models.Category>(category.Id, cancellationToken).ConfigureAwait(false);


                    if (!string.IsNullOrEmpty(row.InSubCategoryName))
                    {
                        string subCategoryKey = $"{row.InCategoryName}_{row.InSubCategoryName}";
                        CategoryReadDto subCategory = null;
                        if (subCategories
                            .ContainsKey(subCategoryKey))
                        {
                            subCategory = subCategories[subCategoryKey];
                        }
                        else
                        {
                            GetSubCategoryByNameCommand getSubCategoryBy =
                                new GetSubCategoryByNameCommand(currentUser.Id, toProcess.Book.Id)
                                    { Name = row.InSubCategoryName, ParentCategoryId = row.Category.Id };
                              subCategory = await mediator.GetResponse(getSubCategoryBy, cancellationToken).ConfigureAwait(false);
                            subCategories.Add(subCategoryKey, subCategory);
                        }
                       

                        //if (subCategory == null)
                        //{
                        //    CreateCategoryCommand createSubCategoryCommand =
                        //        new CreateCategoryCommand(currentUser.Id, toProcess.Book.Id);
                        //    createSubCategoryCommand.CreateDto = new CategoryCreateUpdateDto()
                        //    {
                        //        BookId = toProcess.Book.Id,
                        //        Description = row.InSubCategoryName,
                        //        Name = row.InSubCategoryName,
                        //        ParentCategoryId = row.Category.Id
                        //    };
                        //    subCategory = await mediator.GetResponse(createSubCategoryCommand, cancellationToken).ConfigureAwait(false);
                        //}

                        if (subCategory != null)
                        {
                            row.Category = new Models.Category() { Id = subCategory.Id };
                        }


                    }

                }

            }

            if (row.Category == null)
            {
                    errors.Add($"Categoria non valida");
            }
            row.PaymentMethod = defaultPaymentMethod?.PaymentMethod;
            row.Currency = toProcess.TargetAccount.Currency;

            if (!string.IsNullOrEmpty(row.InCurrency))
            {
                var currency = allCurrencies.FirstOrDefault(j => j.Code == row.InCurrency);
                if (currency != null)
                {
                    row.Currency = new Models.Currency() { Id = currency.Id };
                }
            }
            string beneficiaryName = row.InBeneficiary;

            if (string.IsNullOrEmpty(row.InBeneficiary))
            {
                beneficiaryName = "Sconosciuto";
            }

            
                var beneficiary = allBeneficiary.FirstOrDefault(k => k.Name.ToLower() == beneficiaryName.ToLower());
                if (beneficiary != null)
                {
                    row.Beneficiary = new Models.Beneficiary() { Id = beneficiary.Id };
                }
                else
                {
                    CreateBeneficiaryCommand createBeneficiaryCommand =
                        new CreateBeneficiaryCommand(currentUser.Id, toProcess.Book.Id);
                    createBeneficiaryCommand.CreateDto = new Models.Dto.Beneficiary.BeneficiaryCreateUpdateDto()
                    {
                        BookId = toProcess.Book.Id,
                        Name = row.InBeneficiary,
                        Description = row.InBeneficiary
                    };
                    var createdBeneficiary = await mediator.GetResponse(createBeneficiaryCommand, cancellationToken)
                        .ConfigureAwait(false);
                    row.Beneficiary = new Models.Beneficiary() { Id = createdBeneficiary.Id };
                    allBeneficiary.Add(createdBeneficiary);
                }


                if (!string.IsNullOrEmpty(row.InStatus))
                {
                    row.Status = listTransactionStatus.FirstOrDefault(j => j.Code == row.InStatus);
                }
                else
                {
                    row.Status = listTransactionStatus.FirstOrDefault(j => j.Code == Keys.REC);

                }


                if (row.Status == null)
            {
                row.Status = listTransactionStatus.FirstOrDefault(l => l.Code == Keys.UNREC);
            }
            if (row.WithdrawalAmount.GetValueOrDefault() != 0)
            {
                row.TransactionType = TransactionType.Uscita;
                row.FlowDirection = FlowDirection.Out;
            }
            else
            {
                row.TransactionType = TransactionType.Entrata;
                row.FlowDirection = FlowDirection.In;
            }


            if (errors.Any())
            {
                row.ImportTransactionStatus = ImportTransactionStatus.Error;
                row.ValidateErrors = errors.toJSon();
            }
            else
            {

                row.ImportTransactionStatus = ImportTransactionStatus.Validated;
                row.ValidateErrors = null;
            }

            row.Trace(currentUser);
            await session.UpdateAsync(row, cancellationToken).ConfigureAwait(false);
        }

        return toProcess.ToDto();
    }
}