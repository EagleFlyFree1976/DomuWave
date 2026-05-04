using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Building;

public class DeleteBuildingCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteBuildingCommand() { }
    public DeleteBuildingCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
