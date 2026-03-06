using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsCategoryTemplate;

public class DeleteChartOfAccountsCategoryTemplateCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteChartOfAccountsCategoryTemplateCommand() { }
    public DeleteChartOfAccountsCategoryTemplateCommand(int currentUserId, int id)
        : base(currentUserId) => Id = id;
}
