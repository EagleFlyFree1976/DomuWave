using System;
namespace DomuWave.Services.Models
{
    public class Message
    {
        public virtual long MessageId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CondominiumId { get; set; }
        public virtual long SenderId { get; set; }
        public virtual long RecipientId { get; set; }
        public virtual string Subject { get; set; }
        public virtual string Content { get; set; }
        public virtual long? ParentMessageId { get; set; }
        public virtual bool IsRead { get; set; }
        public virtual DateTime? ReadDate { get; set; }
        public virtual string AttachmentPath { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Condominium Condominium { get; set; }
        public virtual Message ParentMessage { get; set; }
        public Message()
        {
            IsRead = false;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
