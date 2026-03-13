using DomuWave.Services.Dto.Contabilita.AccountBalance;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.AccountBalance;

public class UpdateAccountBalanceOpeningCommand : BaseCommand, IQuery<AccountBalanceReadDto>
{
    public int                          Id  { get; set; }
    public UpdateAccountBalanceOpeningDto Dto { get; set; } = new();
    public UpdateAccountBalanceOpeningCommand() { }
    public UpdateAccountBalanceOpeningCommand(int currentUserId, int id, UpdateAccountBalanceOpeningDto dto)
        : base(currentUserId) { Id = id; Dto = dto; }
}
