using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class DeleteCondominiumCommand : BaseCommand, IQuery<bool>
{
    public int CondominiumId { get; set; }

    public DeleteCondominiumCommand() { }

    public DeleteCondominiumCommand(int currentUserId) : base(currentUserId) { }
    public DeleteCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
