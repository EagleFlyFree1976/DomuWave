namespace DomuWave.Services.Command;

public abstract class BasePagedCommand : BaseCommand
{
    public string SortField { get; set; }
    public bool Asc { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    protected BasePagedCommand()
    {
    }
    protected BasePagedCommand(int currentUserId) : base(currentUserId)
    {
    }
}