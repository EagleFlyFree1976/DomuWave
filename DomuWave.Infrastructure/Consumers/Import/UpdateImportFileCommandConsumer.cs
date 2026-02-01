using System.Globalization;
using System.IO;
using System.IO.Compression;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.Import;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.Category;
using DomuWave.Services.Models.Dto.Import;
using DomuWave.Services.Models.Dto.PaymentMethod;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using CsvHelper;
using CsvHelper.Configuration;
using DocumentFormat.OpenXml.Spreadsheet;
using FluentNHibernate.Utils;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Category
{
    // <summary>
    // </summary>
    public class UpdateImportFileCommandConsumer : InMemoryConsumerBase<UpdateImportFileCommand, Models.Import.Import>
    {
        private IUserService _userService;
        private IImportService _importService;

        public UpdateImportFileCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IImportService importService) : base(sessionFactoryProvider)
        {
            _userService = userService;
            _importService = importService;
        }


        private byte[] CompressCsv(Stream csvStream, string fileName)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);

                    using (var entryStream = entry.Open())
                    {
                        csvStream.CopyTo(entryStream);
                    }
                }

                if (csvStream.CanSeek)
                {
                    csvStream.Seek(0, SeekOrigin.Begin);
                }
                return memoryStream.ToArray();
            }
        }

        private async Task<List<Dictionary<string, string>>> GetRecords(Stream stream, CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(stream);
            var records = new List<Dictionary<string, string>>();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                Delimiter = ";"
            };

       

            using var csv = new CsvHelper.CsvReader(reader, config);
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var record = csv.GetRecord<dynamic>();
                // Do something with the record.
                var dict = (IDictionary<string, object>)record;
                records.Add(dict.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? ""));
                }
             
          
            //await foreach (var row in csv.GetRecordsAsync<dynamic>(cancellationToken))
            //{
              
            //}

            return records;
        }
        protected override async Task<Models.Import.Import> Consume(UpdateImportFileCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
        {
            var currentUser =
                await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);


            var _fileData = CompressCsv(evt.csvStream, evt.FileName);
            
            Models.Import.Import import = await session.GetAsync<Models.Import.Import>(evt.ImportId, cancellationToken)
                .ConfigureAwait(false);
            if (import == null)
            {
                throw new NotFoundException("Elemento non trovato");
            }

            import.Name = evt.Name;
            import.FileData = _fileData;
            import.TargetAccount = await session.GetAsync<Account>(evt.TargetAccountId, cancellationToken)
                .ConfigureAwait(false);
            import.Book = await session.GetAsync<Models.Book>(evt.BookId, cancellationToken).ConfigureAwait(false);
            import.Description = evt.FileName;
            import.Trace(currentUser);
            await session.SaveAsync(import, cancellationToken).ConfigureAwait(false);

            return import;

        }
    }
}

