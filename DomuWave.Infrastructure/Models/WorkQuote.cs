using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class WorkQuote : TenantEntity<int>
    {
        public virtual ExtraordinaryWork Work          { get; set; }
        public virtual Supplier          Supplier      { get; set; }
        public virtual string            QuoteNumber   { get; set; }
        public virtual DateTime          QuoteDate     { get; set; }
        public virtual DateTime?         ValidUntil    { get; set; }
        public virtual decimal           Amount        { get; set; }
        public virtual string            Status        { get; set; }
        public virtual string            Notes         { get; set; }
        public virtual bool              IsApproved    { get; set; }

        public virtual IList<WorkQuoteDocument> Documents { get; set; } = new List<WorkQuoteDocument>();

        public override int GetHashCode() => this.Id.GetHashCode();
    }
}
