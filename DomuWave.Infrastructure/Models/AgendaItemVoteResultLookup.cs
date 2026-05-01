namespace DomuWave.Services.Models;

public class AgendaItemVoteResultLookup
{
    public virtual int    Id   { get; set; }
    public virtual string Name { get; set; }

    public const int NonVotato = 1;
    public const int Approvato = 2;
    public const int Respinto  = 3;
    public const int Rinviato  = 4;

    public override int GetHashCode() => Id.GetHashCode();
}
