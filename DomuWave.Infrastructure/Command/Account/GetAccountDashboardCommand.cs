using DomuWave.Services.Models.Dto;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

public class GetAccountDashboardCommand : BaseBookRelatedCommand, IQuery<AccountDashboardDto>
{
    public GetAccountDashboardCommand()
    {
    }

    public GetAccountDashboardCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }

    public DateTime? TargetDate { get; set; } = DateTime.Today;
    public long BookId { get; set; }
    public long AccountId { get; set; }
    public int OwnerId { get; set; }
}