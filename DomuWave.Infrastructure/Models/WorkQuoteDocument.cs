using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class WorkQuoteDocument : TenantEntity<int>
    {
        public virtual WorkQuote Work         { get; set; }
        public virtual string    Title        { get; set; }
        public virtual string    FileName     { get; set; }
        public virtual string    FilePath     { get; set; }
        public virtual long      FileSize     { get; set; }
        public virtual string    MimeType     { get; set; }
        public virtual string    DocumentType { get; set; }
        public virtual string    Notes        { get; set; }

        public override int GetHashCode() => this.Id.GetHashCode();
    }
}
