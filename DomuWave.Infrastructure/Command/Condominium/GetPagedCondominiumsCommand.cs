using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class GetPagedCondominiumsCommand : BaseCommand, IQuery<object>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool Ascending { get; set; }

    public GetPagedCondominiumsCommand() { }

    public GetPagedCondominiumsCommand(int currentUserId) : base(currentUserId) { }
    public GetPagedCondominiumsCommand(int currentUserId, int page, int pageSize, bool ascending) : base(currentUserId)
    {
        Page = page;
        PageSize = pageSize;
        Ascending = ascending;
    }
}
