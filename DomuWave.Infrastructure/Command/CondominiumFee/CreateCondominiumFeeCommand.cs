using DomuWave.Services.Dto.CondominiumFee;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class CreateCondominiumFeeCommand : BaseCommand, IQuery<CondominiumFeeReadDto>
{
    public CreateCondominiumFeeDto Dto { get; set; }

    public CreateCondominiumFeeCommand() { }

    public CreateCondominiumFeeCommand(int currentUserId) : base(currentUserId) { }
    public CreateCondominiumFeeCommand(int currentUserId, CreateCondominiumFeeDto dto) : base(currentUserId)
    {
        Dto = dto;
    }
}
