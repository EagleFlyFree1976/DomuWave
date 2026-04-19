using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class ExtraordinaryWork : TenantEntity<int>
    {
        public virtual Condominium Condominium    { get; set; } = null!;
        public virtual string      Title          { get; set; } = string.Empty;
        public virtual new string? Description    { get; set; }
        public virtual string?     Category       { get; set; }
        public virtual string?     Status         { get; set; }
        public virtual string?     Priority       { get; set; }
        public virtual DateTime    RequestedDate  { get; set; }
        public virtual DateTime?   ApprovedDate   { get; set; }
        public virtual DateTime?   StartDate      { get; set; }
        public virtual DateTime?   CompletedDate  { get; set; }
        public virtual decimal?    ApprovedAmount { get; set; }
        public virtual decimal?    ActualCost     { get; set; }
        public virtual string?     Notes          { get; set; }

        public virtual IList<WorkQuote> Quotes { get; set; } = new List<WorkQuote>();

        public override int GetHashCode() => this.Id.GetHashCode();
    }
}
