using DomuWave.Services.Dto.Communication;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class CreateCommunicationCommand : BaseCommand, IQuery<CommunicationReadDto>
{
    public CreateCommunicationDto Dto { get; set; }

    public CreateCommunicationCommand() { }
    public CreateCommunicationCommand(int currentUserId, CreateCommunicationDto dto) : base(currentUserId)
        => Dto = dto;
}
