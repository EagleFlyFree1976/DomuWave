using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using DomuWave.Services.Command.Import;
using DomuWave.Services.Extensions;
using DomuWave.Services.Helper;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Import;
using DomuWave.Services.Models.Import;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using CsvHelper;
using CsvHelper.Configuration;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class ExtractImportCommandConsumer : InMemoryConsumerBase<ExtractImportCommand, ImportDto>
{
    private IUserService _userService;
    private IImportService _importService;

    public ExtractImportCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IImportService importService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _importService = importService;
    }

    private Stream DecompressByte(byte[] zipBytes)
    {
        using (var zipStream = new MemoryStream(zipBytes))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            ZipArchiveEntry? entry = archive.Entries.FirstOrDefault();
            if (entry == null)
                throw new InvalidOperationException("L'archivio ZIP non contiene file.");

            return entry.Open();
        }
    }
    private string DecompressCsvToString(byte[] zipBytes)
    {
        using (var zipStream = new MemoryStream(zipBytes))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            var entry = archive.Entries.FirstOrDefault();
            if (entry == null)
                throw new InvalidOperationException("L'archivio ZIP non contiene file.");

            using (var entryStream = entry.Open())
            using (var reader = new StreamReader(entryStream))
            {
                return reader.ReadToEnd();
            }
        }
    }



    protected override async Task<ImportDto> Consume(ExtractImportCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var toProcess = await session.GetAsync<Models.Import.Import>(evt.ImportId, cancellationToken)
            .ConfigureAwait(false);
        if (toProcess == null)
            throw new NotFoundException("Import non trovato");



        var alreadyExistImportRow = await session.Query<ImportTransaction>().Where(k => k.Import.Id == evt.ImportId)
            .AnyAsync(cancellationToken);
        if (alreadyExistImportRow)
        {
            await session.CreateQuery("delete from DomuWave.Services.Models.Import.ImportTransaction where Import.Id = :importId")
                .SetParameter("importId", evt.ImportId)
                .ExecuteUpdateAsync(cancellationToken).ConfigureAwait(false);
        }
        var records = new List<Dictionary<string, string>>();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.ToLower(),
            Delimiter = ";"
        };

        using (var zipStream = new MemoryStream(toProcess.FileData))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            var entry = archive.Entries.FirstOrDefault();
            if (entry == null)
                throw new InvalidOperationException("L'archivio ZIP non contiene file CSV.");

            using (var entryStream = entry.Open())
            using (var reader = new StreamReader(entryStream))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var record = csv.GetRecord<dynamic>();
                    // Do something with the record.
                    var dict = (IDictionary<string, object>)record;
                    records.Add(dict.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? ""));
                }
            }
        }


        foreach (Dictionary<string, string> dictionary in records)
        {
            ImportTransaction transaction = new ImportTransaction();

            var actualConfiguration = toProcess.Configuration;
            ImportConfigurationDto actualImportConfigurationDto = actualConfiguration.parseJson<ImportConfigurationDto>();

            transaction.Import = toProcess;


                transaction.InTransactionDate = GetString(actualImportConfigurationDto, dictionary, TargetFieldCodes.TransactionDate);

                
                transaction.InDepositAmount = GetString(actualImportConfigurationDto, dictionary, TargetFieldCodes.DepositAmount);
                transaction.InWithdrawalAmount = GetString(actualImportConfigurationDto, dictionary, TargetFieldCodes.WithdrawalAmount);
                transaction.InAmount = GetString(actualImportConfigurationDto, dictionary, TargetFieldCodes.Amount);
                
                transaction.InCategoryName= GetString(actualImportConfigurationDto, dictionary, TargetFieldCodes.CategoryName);
                transaction.InSubCategoryName= GetString(actualImportConfigurationDto, dictionary, TargetFieldCodes.SubCategoryName);
                transaction.Description= GetString(actualImportConfigurationDto, dictionary, TargetFieldCodes.Description);
                transaction.InCurrency= GetString(actualImportConfigurationDto, dictionary, TargetFieldCodes.Currency);
                transaction.InBeneficiary= GetString(actualImportConfigurationDto, dictionary, TargetFieldCodes.Beneficiary);
                transaction.InType= GetString(actualImportConfigurationDto, dictionary, TargetFieldCodes.Type);
                transaction.InStatus= GetString(actualImportConfigurationDto, dictionary, TargetFieldCodes.Status);

                transaction.ImportTransactionStatus = ImportTransactionStatus.New;

                transaction.Trace(currentUser);
            await session.SaveAsync(transaction, cancellationToken).ConfigureAwait(false);
        }
        //

        return toProcess.ToDto();
    }

    private string GetString(ImportConfigurationDto actualImportConfigurationDto, Dictionary<string, string> dictionary, string targetFieldCode)
    {
        ImportField field = actualImportConfigurationDto.Fields.FirstOrDefault(k => k.TargetField != null &&  k.TargetField.Code == targetFieldCode);
        if (field == null)
            return string.Empty;
        dictionary.TryGetValue(
            field.FieldName,
            out var fieldValueStr);
        return fieldValueStr;
    }
}