using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Import;

public class FullImportFileCommand : BaseBookRelatedCommand, IQuery<Models.Import.Import>
{
    public long ImportId { get; set; }


    public long TargetAccountId { get; set; }




    public string FileName { get; set; }

    public Stream csvStream { get; set; }

    public bool HasHeader { get; set; }



}