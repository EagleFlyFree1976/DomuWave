using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class Maintenance : TenantEntity<int>
    {
        public virtual Condominium Condominium  { get; set; } = null!;
        public virtual Supplier?  Supplier     { get; set; }
        public virtual string     Title        { get; set; } = string.Empty;
        public virtual new string? Description  { get; set; }
        public virtual string?    Category     { get; set; }
        public virtual string?    Priority     { get; set; }
        public virtual string?    Status       { get; set; }
        public virtual DateTime   ReportedDate { get; set; }
        public virtual DateTime?  ScheduledDate { get; set; }
        public virtual DateTime?  CompletedDate { get; set; }
        public virtual decimal?   EstimatedCost { get; set; }
        public virtual decimal?   ActualCost    { get; set; }
        public virtual string?    Notes        { get; set; }
        public virtual string?    DocumentPath { get; set; }

        public override int GetHashCode() => this.Id.GetHashCode();
    }
}
