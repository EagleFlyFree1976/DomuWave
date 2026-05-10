using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Staircase;

public class DeleteStaircaseCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteStaircaseCommand() { }
    public DeleteStaircaseCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
