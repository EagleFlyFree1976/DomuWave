using DomuWave.Services.Dto.ExtraordinaryWork;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class CreateQuoteCommand : BaseCommand, IQuery<WorkQuoteReadDto>
{
    public CreateWorkQuoteDto Dto { get; }
    public CreateQuoteCommand(int currentUserId, CreateWorkQuoteDto dto) : base(currentUserId)
        => Dto = dto;
}
