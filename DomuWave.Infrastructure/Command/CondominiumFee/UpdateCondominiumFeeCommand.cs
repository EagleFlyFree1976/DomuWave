using DomuWave.Services.Dto.CondominiumFee;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class UpdateCondominiumFeeCommand : BaseCommand, IQuery<CondominiumFeeReadDto>
{
    public long                    FeeId { get; set; }
    public UpdateCondominiumFeeDto Dto   { get; set; }

    public UpdateCondominiumFeeCommand() { }

    public UpdateCondominiumFeeCommand(int currentUserId, long feeId, UpdateCondominiumFeeDto dto) : base(currentUserId)
    {
        FeeId = feeId;
        Dto   = dto;
    }
}
