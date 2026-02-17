using System;
namespace DomuWave.Services.Models
{
    public class DocumentAccess
    {
        public virtual long AccessId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int DocumentId { get; set; }
        public virtual long UserId { get; set; }
        public virtual DateTime AccessDate { get; set; }
        public virtual string AccessType { get; set; }
        public virtual string IpAddress { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Document Document { get; set; }
        public DocumentAccess()
        {
            AccessDate = DateTime.UtcNow;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
