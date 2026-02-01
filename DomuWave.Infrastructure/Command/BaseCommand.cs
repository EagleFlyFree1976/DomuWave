namespace DomuWave.Services.Command;

public abstract class BaseCommand
{
    public int CurrentUserId { get; set; }

    protected BaseCommand()
    {
    }

    protected BaseCommand(int currentUserId)
    {
        CurrentUserId = currentUserId;
    }
}