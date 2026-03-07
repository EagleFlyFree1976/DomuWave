using DomuWave.Services.Dto.ExtraordinaryWork;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class AddQuoteDocumentCommand : BaseCommand, IQuery<WorkQuoteDocumentReadDto>
{
    public CreateWorkQuoteDocumentDto Dto { get; }
    public AddQuoteDocumentCommand(int currentUserId, CreateWorkQuoteDocumentDto dto) : base(currentUserId)
        => Dto = dto;
}
