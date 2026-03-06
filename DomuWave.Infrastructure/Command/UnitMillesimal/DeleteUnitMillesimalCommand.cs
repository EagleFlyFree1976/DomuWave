using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitMillesimal;

public class DeleteUnitMillesimalCommand : BaseCommand, IQuery<bool>
{
    public int EntryId { get; set; }

    public DeleteUnitMillesimalCommand() { }
    public DeleteUnitMillesimalCommand(int currentUserId, int entryId) : base(currentUserId)
    {
        EntryId = entryId;
    }
}
