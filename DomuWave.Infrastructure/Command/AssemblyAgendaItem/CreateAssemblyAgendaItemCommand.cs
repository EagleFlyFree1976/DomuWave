using DomuWave.Services.Dto.AssemblyAgendaItem;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.AssemblyAgendaItem;

public class CreateAssemblyAgendaItemCommand : BaseCommand, IQuery<AssemblyAgendaItemReadDto>
{
    public CreateAssemblyAgendaItemDto Dto { get; set; }

    public CreateAssemblyAgendaItemCommand() { }
    public CreateAssemblyAgendaItemCommand(int currentUserId, CreateAssemblyAgendaItemDto dto) : base(currentUserId)
        => Dto = dto;
}
