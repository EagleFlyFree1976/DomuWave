namespace DomuWave.Services.Models;

public class AssemblyAgendaItem : TenantEntity<int>
{
    // Name (base) → Titolo del punto OdG
    // Description (base) → Descrizione del punto

    public virtual Assembly                   Assembly   { get; set; }
    public virtual int                        OrderIndex { get; set; }
    public virtual AgendaItemVoteResultLookup VoteResult { get; set; }
    public virtual string?                    Resolution { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
}
