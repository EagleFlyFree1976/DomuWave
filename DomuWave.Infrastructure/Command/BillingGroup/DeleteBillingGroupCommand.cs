using SimpleMediator.Queries;

namespace DomuWave.Services.Command.BillingGroup;

public class DeleteBillingGroupCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteBillingGroupCommand() { }
    public DeleteBillingGroupCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
