using DomuWave.Services.Dto.CondominiumInstallment;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class CreateCondominiumInstallmentCommand : BaseCommand, IQuery<CondominiumInstallmentReadDto>
{
    public CreateCondominiumInstallmentDto Dto { get; set; }

    public CreateCondominiumInstallmentCommand() { }

    public CreateCondominiumInstallmentCommand(int currentUserId) : base(currentUserId) { }
    public CreateCondominiumInstallmentCommand(int currentUserId, CreateCondominiumInstallmentDto dto) : base(currentUserId)
    {
        Dto = dto;
    }
}
