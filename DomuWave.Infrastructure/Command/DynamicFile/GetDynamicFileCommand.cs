using DomuWave.Services.Dto.DynamicFile;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.DynamicFile;

public class GetDynamicFileCommand : BaseCommand, IQuery<DynamicFileReadDto>
{
    public int  Id              { get; set; }
    /// <summary>Se true, carica anche il contenuto base64</summary>
    public bool IncludeContent  { get; set; }

    public GetDynamicFileCommand() { }

    public GetDynamicFileCommand(int currentUserId, int id, bool includeContent = false)
        : base(currentUserId)
    {
        Id             = id;
        IncludeContent = includeContent;
    }
}
