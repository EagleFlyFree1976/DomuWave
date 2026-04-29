using DomuWave.Services.Dto.Communication;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class UpdateCommunicationCommand : BaseCommand, IQuery<CommunicationReadDto?>
{
    public int               Id  { get; set; }
    public UpdateCommunicationDto Dto { get; set; }

    public UpdateCommunicationCommand() { }
    public UpdateCommunicationCommand(int currentUserId, int id, UpdateCommunicationDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
