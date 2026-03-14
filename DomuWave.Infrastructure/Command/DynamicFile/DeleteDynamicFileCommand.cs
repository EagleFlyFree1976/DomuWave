using SimpleMediator.Queries;

namespace DomuWave.Services.Command.DynamicFile;

public class DeleteDynamicFileCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteDynamicFileCommand() { }

    public DeleteDynamicFileCommand(int currentUserId, int id)
        : base(currentUserId)
    {
        Id = id;
    }
}
