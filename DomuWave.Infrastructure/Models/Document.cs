using System;
namespace DomuWave.Services.Models
{
    public class Document
    {
        public virtual int DocumentId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CondominiumId { get; set; }
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
        public virtual string Category { get; set; }
        public virtual string FileName { get; set; }
        public virtual string FilePath { get; set; }
        public virtual long FileSize { get; set; }
        public virtual string MimeType { get; set; }
        public virtual DateTime? DocumentDate { get; set; }
        public virtual bool IsVisibleToOwners { get; set; }
        public virtual bool IsArchived { get; set; }
        public virtual string Tags { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Condominium Condominium { get; set; }
        public Document()
        {
            IsVisibleToOwners = false;
            IsArchived = false;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
