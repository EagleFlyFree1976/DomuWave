using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsCategory;

public class DeleteChartOfAccountsCategoryCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteChartOfAccountsCategoryCommand() { }
    public DeleteChartOfAccountsCategoryCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
