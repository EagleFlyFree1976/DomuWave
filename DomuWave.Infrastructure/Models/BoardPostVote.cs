namespace DomuWave.Services.Models
{
    /// <summary>
    /// Voto di un condòmino su un'opzione di un sondaggio in bacheca.
    /// Un utente può avere più voti solo se il sondaggio è a scelta multipla.
    /// Il cambio voto è gestito via soft-delete + insert (filtered unique index lato DB).
    /// </summary>
    public class BoardPostVote : TenantEntity<int>
    {
        public virtual BoardPost Post { get; set; } = null!;
        public virtual BoardPostOption Option { get; set; } = null!;
        public virtual long VoterUserId { get; set; }
        public virtual string VoterFullName { get; set; } = string.Empty;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
