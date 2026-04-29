using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class ArchiveCommunicationCommand : BaseCommand, IQuery<bool>
{
    public int  CommunicationId { get; set; }
    public bool Archive         { get; set; }

    public ArchiveCommunicationCommand() { }
    public ArchiveCommunicationCommand(int currentUserId, int communicationId, bool archive) : base(currentUserId)
    {
        CommunicationId = communicationId;
        Archive         = archive;
    }
}
