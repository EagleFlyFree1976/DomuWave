using SimpleMediator.Queries;

namespace DomuWave.Services.Command.MillesimalTable;

public class SetMillesimalTableEnabledCommand : BaseCommand, IQuery<bool>
{
    public int  TableId   { get; set; }
    public bool IsEnabled { get; set; }

    public SetMillesimalTableEnabledCommand(int currentUserId, int tableId, bool isEnabled) : base(currentUserId)
    {
        TableId   = tableId;
        IsEnabled = isEnabled;
    }
}
