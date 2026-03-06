using DomuWave.Services.Dto.CondominiumInstallment;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class UpdateCondominiumInstallmentCommand : BaseCommand, IQuery<CondominiumInstallmentReadDto>
{
    public int                             InstallmentId { get; set; }
    public UpdateCondominiumInstallmentDto Dto           { get; set; }

    public UpdateCondominiumInstallmentCommand() { }

    public UpdateCondominiumInstallmentCommand(int currentUserId) : base(currentUserId) { }
    public UpdateCondominiumInstallmentCommand(int currentUserId, int installmentId, UpdateCondominiumInstallmentDto dto) : base(currentUserId)
    {
        InstallmentId = installmentId;
        Dto           = dto;
    }
}
