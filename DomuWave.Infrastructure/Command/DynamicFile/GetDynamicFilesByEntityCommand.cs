using DomuWave.Services.Dto.DynamicFile;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.DynamicFile;

public class GetDynamicFilesByEntityCommand : BaseCommand, IQuery<IList<DynamicFileReadDto>>
{
    public string EntityName     { get; set; }
    public string EntityFullName { get; set; }
    public int    EntityId       { get; set; }

    public GetDynamicFilesByEntityCommand() { }

    public GetDynamicFilesByEntityCommand(int currentUserId, string entityName, string entityFullName, int entityId)
        : base(currentUserId)
    {
        EntityName     = entityName;
        EntityFullName = entityFullName;
        EntityId       = entityId;
    }
}
