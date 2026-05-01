using DomuWave.Services.Dto.AssemblyAgendaItem;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.AssemblyAgendaItem;

public class UpdateAssemblyAgendaItemCommand : BaseCommand, IQuery<AssemblyAgendaItemReadDto>
{
    public int                        Id  { get; set; }
    public UpdateAssemblyAgendaItemDto Dto { get; set; }

    public UpdateAssemblyAgendaItemCommand() { }
    public UpdateAssemblyAgendaItemCommand(int currentUserId, int id, UpdateAssemblyAgendaItemDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
