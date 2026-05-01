namespace DomuWave.Services.Models;

public class AgendaItemVoteResultLookup
{
    public virtual int    Id   { get; set; }
    public virtual string Name { get; set; }

    public const int NonVotato  = 0;
    public const int Approvato  = 1;
    public const int Respinto   = 2;
    public const int Rinviato   = 3;

    public override int GetHashCode() => Id.GetHashCode();
}
