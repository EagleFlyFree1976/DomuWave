namespace DomuWave.Services.Models
{
    /// <summary>
    /// Attività pianificabile dall'amministratore: titolo, priorità, stato, scadenza,
    /// assegnatario (collaboratore del tenant) e collegamento opzionale a 0/1/N condomìni.
    /// </summary>
    public class AdminTask : TenantEntity<int>
    {
        // Name (base) è mappato sulla colonna Title; Title è la copia read-only.
        public virtual string?        Title    { get; set; }
        public virtual new string?    Description { get; set; }

        public virtual AdminTaskPriorityLookup Priority { get; set; } = null!;
        public virtual AdminTaskStatusLookup   Status   { get; set; } = null!;

        public virtual DateTime? DueDate { get; set; }

        /// <summary>Id (AuthService) del collaboratore assegnatario. FK logica, no FK fisica.</summary>
        public virtual int?    AssignedToUserId   { get; set; }
        /// <summary>Snapshot del nome dell'assegnatario (evita round-trip all'AuthService in lettura).</summary>
        public virtual string? AssignedToFullName { get; set; }

        public virtual IList<AdminTaskCondominium> Condominiums { get; set; } = new List<AdminTaskCondominium>();

        public override int GetHashCode() => Id.GetHashCode();
    }
}
