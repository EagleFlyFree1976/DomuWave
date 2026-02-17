using System;
namespace DomuWave.Services.Models
{
    public class Communication
    {
        public virtual int CommunicationId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CondominiumId { get; set; }
        public virtual string Title { get; set; }
        public virtual string Content { get; set; }
        public virtual string CommunicationType { get; set; }
        public virtual string Priority { get; set; }
        public virtual DateTime PublicationDate { get; set; }
        public virtual DateTime? ExpirationDate { get; set; }
        public virtual bool SendEmail { get; set; }
        public virtual DateTime? EmailSentAt { get; set; }
        public virtual bool IsVisible { get; set; }
        public virtual string AttachmentPath { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Condominium Condominium { get; set; }
        public Communication()
        {
            Priority = "Normal";
            PublicationDate = DateTime.UtcNow;
            SendEmail = true;
            IsVisible = true;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
