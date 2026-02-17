using System;
namespace DomuWave.Services.Models
{
    public class CommunicationRead
    {
        public virtual int ReadId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CommunicationId { get; set; }
        public virtual long UserId { get; set; }
        public virtual DateTime ReadDate { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Communication Communication { get; set; }
        public CommunicationRead()
        {
            ReadDate = DateTime.UtcNow;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
