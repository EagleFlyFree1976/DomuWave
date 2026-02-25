using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class Document : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual string Title { get; set; }
        public virtual string Category { get; set; }
        public virtual string FileName { get; set; }
        public virtual string FilePath { get; set; }
        public virtual long FileSize { get; set; }
        public virtual string MimeType { get; set; }
        public virtual DateTime? DocumentDate { get; set; }
        public virtual bool IsVisibleToOwners { get; set; }
        public virtual bool IsArchived { get; set; }
        public virtual string Tags { get; set; }

        public virtual IList<DocumentAccess> Accesses { get; set; } = new List<DocumentAccess>();
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
