using DomuWave.Services.Models.Dto.Import;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Import;


/// <summary>
///     prende il file dato input ed estrae i dati nella tabella imp_Transactions
///     non viene eseguito nessuna validazione
///     
/// </summary>
public class ExtractImportCommand : BaseBookRelatedCommand, IQuery<ImportDto>
{
    public long ImportId { get; set; }
}